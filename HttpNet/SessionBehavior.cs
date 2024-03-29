﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HttpNet
{
	public class SessionBehavior
	{
		Session session;

		/// <summary>
		/// The unique session id associated with this session
		/// </summary>
		public string SessionID { get; private set; }

		/// <summary>
		/// The endpoint of the client
		/// </summary>
		public IPEndPoint RemoteEndPoint { get; private set; }

		internal Task OnCreate(Session session, string sessionId, IPEndPoint remoteEndPoint)
		{
			this.session = session;
			SessionID = sessionId;
			RemoteEndPoint = remoteEndPoint;
			return OnCreate();
		}

		/// <summary>
		/// Will fire when this session is created and all the protected fields have been set
		/// </summary>
		/// <returns></returns>
		protected virtual Task OnCreate()
		{
			return Task.CompletedTask;
		}

		public void Destroy()
		{
			session.Destroy();
		}
	}
}
