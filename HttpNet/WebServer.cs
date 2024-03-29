﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace HttpNet
{
	public class WebServer 
	{
		HttpListener server;
		SessionManager sessionManager;

		volatile bool running;
		
		public event EventHandler<LogEventArgs> OnLog;
		public event Action<Exception> OnException;
		public LogLevels LogLevel;

		int sessionLifetime;

		Router rootRouter;
		internal Type SessionBehaviorType;

		private WebServer(string host, int port, int sessionLifetime = 300)
		{
			this.sessionLifetime = sessionLifetime;
			
			server = new HttpListener();
			server.Prefixes.Add(string.Format("http://{0}:{1}/", host, port));

			LogLevel = LogLevels.Warning | LogLevels.Error;

			rootRouter = new Router(this, "");
		}

		public static WebServer Create<Behavior>(string host, int port, int sessionLifetime = 300)
			where Behavior : SessionBehavior, new()
		{
			WebServer server = new WebServer(host, port, sessionLifetime);
			server.SessionBehaviorType = typeof(Behavior);
			return server;
		}

		public static WebServer Create(string host, int port, int sessionLifetime = 300)
		{
			return Create<SessionBehavior>(host, port, sessionLifetime);
		}

		public WebServer Add(string path, Func<HttpRequest, Task> handler)
		{
			rootRouter.Add(path, handler);
			return this;
		}

		public Router AddRouter(string path)
		{
			return rootRouter.CreateRouter(path);
		}

		public void Start()
		{
			Log(LogLevels.Info, "Web Server started on: " + server.Prefixes.First());

			running = true;

			sessionManager = new SessionManager(sessionLifetime);
			server.Start();
			ServerLoop();
		}

		public void Stop()
		{
			running = false;
			server.Stop();
		}

		async void ServerLoop()
		{
			while (running)
			{
				try
				{
					HttpListenerContext connection = await server.GetContextAsync();
					HandleRequest(connection);
				}
				catch (Exception ex)
				{
					OnException?.Invoke(ex);
				}
			}
		}

		void HandleRequest(HttpListenerContext connection)
		{
			Log(LogLevels.Debug, "GET " + connection.Request.RawUrl);

			Session session = GetOrSetSession(connection.Request, connection.Response);
			HttpRequest request = new HttpRequest(this ,connection.Request, connection.Response, session);


			rootRouter.Handle(request, session).ContinueWith(
				t =>
				{
					// If the handling task died, don't forget to close the stream.
					if (t.IsFaulted)
					{
						OnException?.Invoke(t.Exception);
						Log(LogLevels.Error, t.Exception.ToString());
						request.SetStatusCode(HttpStatusCode.InternalServerError);
						request.Close();
					}
				});
		}

		Session GetOrSetSession(HttpListenerRequest request, HttpListenerResponse response)
		{
			/*
			 * Debt: The browser doesn't send any cookies when requesting favicon.ico so we start a new session for each.
			 */

			Session session = null;
			Cookie sessCookie = request.Cookies["SESSID"];

			if (sessCookie != null)
			{
				session = sessionManager.GetSessionWithId(sessCookie.Value);
			}

			if (session == null)
			{
				session = sessionManager.CreateSession(request.RemoteEndPoint);
				Log(LogLevels.Debug, "New session: " + session.SessionID);
			}

			sessCookie = session.GetCookie();
			sessCookie.Expires = DateTime.Now + TimeSpan.FromSeconds(sessionLifetime);
			response.SetCookie(sessCookie);
			return session;
		}

		internal void Log(LogLevels level, string message)
		{
			if (LogLevel.HasFlag(level))
			{
				OnLog?.Invoke(this, new LogEventArgs(level, message));
			}
		}

		internal void Log(LogLevels level, string messageFormat, params object[] args)
		{
			Log(level, string.Format(messageFormat, args));
		}

		bool Match(string pattern, string path)
		{
			pattern = Utils.WildcardRegex(pattern);
			Match match = Regex.Match(path, pattern);
			return match.Success;
		}
	}
}
