interface JQuery {
    datetimepicker(options: any): JQuery;
    datepickerInit(userOptions: any): JQuery;
    timepickerInit(userOptions: any): JQuery;
    datetimepickerInit(userOptions?: any): JQuery;
    datepicker(userOptions: any): JQuery;
}

(function ($) {

    jQuery.fn.datepickerInit = function (userOptions) {
        var $element = $(this);
        var options = {
            locale: abp.helperConfiguration.getCurrentLanguageLocale(),
            format: 'L'
        };
        options = $.extend(options, userOptions);

        var result = $element.datetimepicker(options);
        return result;
    };

    jQuery.fn.timepickerInit = function (userOptions) {
        var $element = $(this);
        if ($element.val()) {
            $element.val(moment($element.val(), ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A", 'hh:mm A']).format("LT"));
        }
        var options = $.extend(
            {
                format: 'LT',
                useCurrent: false,
                stepping: 5, //Number of minutes the up/down arrow's will move the minutes value in the time picker
                allowInputToggle: true //show picker on input click
            },
            userOptions);
        return $element.datetimepicker(options);
    };

    jQuery.fn.datetimepickerInit = function (userOptions?: any) {
        var $element = $(this);
        if ($element.val()) {
            $element.val(moment($element.val(), ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A", 'hh:mm A']).format("L LT"));
        }
        var options = $.extend(
            {
                format: 'L LT',
                useCurrent: false,
                stepping: 1, //Number of minutes the up/down arrow's will move the minutes value in the time picker
                allowInputToggle: true //show picker on input click
            },
            userOptions);
        return $element.datetimepicker(options);
    };

    jQuery.fn.datepicker = function (options) {
        var $element = $(this);
        options = options || { format: 'L' };
        return $element.datetimepicker(options);
    };

})(jQuery);