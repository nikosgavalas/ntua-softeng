var slider = "#price-max";
var slider_output = "#price-slider-value";

function update_slider_value() {
	$(slider_output).html($(slider).val()).append('&nbsp;<span class="fa fa-money"></span>');
}


$(slider).change(update_slider_value);
