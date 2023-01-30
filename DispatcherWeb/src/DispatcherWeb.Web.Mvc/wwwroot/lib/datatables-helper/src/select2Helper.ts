interface JQuery {
    getDropdownOption(val: any): JQuery;
    getSelectedDropdownOption(): JQuery;
    removeUnselectedOptions(): JQuery;
    select2Init(userOptions?: any): JQuery<any>;
}

(function ($) {
    abp = abp || {};
    abp.helper = abp.helper || {};
    abp.helper.ui = abp.helper.ui || {};

    jQuery.fn.select2Init = function (userOptions) {
        userOptions = userOptions || {};
        var $element = $(this);
        var select2PageSize = 20;
        var placeholder = $(this).data('placeholder') || $(this).find('option[value=""]').text() || '';
        $element.attr('data-placeholder', placeholder);
        var defaultOptions: Select2.Options<any, any> = {
            minimumInputLength: 1,
            allowClear: true,
            //placeholder: placeholder,
            width: "100%",
            ajax: {
                delay: 250,
                data: function (params) {
                    params.page = params.page || 1;
                    return {
                        term: params.term,
                        skipCount: (params.page - 1) * select2PageSize,
                        maxResultCount: select2PageSize
                    };
                },
                transport: function (params, success, failure) {
                    var additionalParams = {};
                    if (userOptions.abpServiceParamsGetter) {
                        additionalParams = userOptions.abpServiceParamsGetter(params);
                    }
                    return userOptions.abpServiceMethod($.extend({}, params.data, userOptions.abpServiceParams, additionalParams)).done(success).fail(failure);
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data.items,
                        pagination: {
                            more: params.page * select2PageSize < data.totalCount
                        }
                    };
                },
                cache: false
            },
            templateResult: function (data) {
                if (data.id === '') {
                    return placeholder;
                }
                data.text = data.name;
                return data.name;
            }
        };
        if (!userOptions.abpServiceMethod) {
            //defaultOptions.ajax.transport = undefined;
            defaultOptions.ajax = undefined;
            defaultOptions.templateResult = undefined;
            if (userOptions.showAll === undefined && userOptions.minimumInputLength === undefined) {
                userOptions.showAll = true;
            }
        }
        if (userOptions.dropdownParent !== undefined) {
            defaultOptions.dropdownParent = userOptions.dropdownParent;
        }
        if (userOptions.showAll) {
            defaultOptions.minimumInputLength = 0;
        }
        if (userOptions.noSearch) {
            defaultOptions.minimumResultsForSearch = Infinity;
        }

        var options = $.extend(true, {}, defaultOptions, userOptions) as Select2.Options<any, any>;

        //clear the results before the next opening, so it would calculate the height of popup correctly.
        $element.on("select2:close", function () { $(this).data("select2").$results.empty(); });
        //same, but for the case when a term is entered but popup is closed before an ajax call is complete
        $element.on("select2:opening", function () { $(this).data("select2").$results.empty(); });

        $element.change(function () {
            if ($element.closest(".form-group").hasClass("has-danger")) {
                $(this).closest("form").valid();
            }
        });

        return $element.select2(options);
    };

    jQuery.fn.getDropdownOption = function (val) {
        return $(this).find('option[value="' + val + '"]');
    };

    jQuery.fn.getSelectedDropdownOption = function () {
        return $(this).getDropdownOption($(this).val());
    };

    //only usefull for the dynamic select2 inputs
    jQuery.fn.removeUnselectedOptions = function () {
        //remove the initially rendered <option> when the value changes,
        //so that the next time it is selected it would have its 'item' property populated
        let val = $(this).val();
        $(this).find('option').not(`[value=""],[value="${val}"]`).remove();
        return $(this);
    };

    abp.helper.ui.addAndSetDropdownValue = function addAndSetDropdownValue(dropdown, value, label) {
        if (value === undefined || value === null) {
            value = '';
        }
        if (label === undefined || label === null) {
            label = '';
        }
        if (dropdown.find('option[value="' + value + '"]').length === 0) {
            $('<option></option>').text(label).attr('value', value).appendTo(dropdown);
        }
        if (dropdown.val() !== value.toString()) {
            dropdown.val(value).change();
        }
    };

    abp.helper.ui.addAndSetSelect2ValueSilently = function addAndSetSelect2ValueSilently(dropdown, value, label) {
        if (!dropdown || !dropdown.length) {
            return;
        }
        if (value === undefined || value === null) {
            value = '';
        }
        if (label === undefined || label === null) {
            label = '';
        }
        if (dropdown.find('option[value="' + value + '"]').length === 0) {
            $('<option></option>').text(label).attr('value', value).appendTo(dropdown);
        }
        if (dropdown.val() !== value.toString()) {
            dropdown.val(value).trigger('change.select2');
        }
    };

    abp.helper.ui.getDropdownValueAndLabel = function getDropdownValueAndLabel(dropdown) {
        var value = dropdown.val();
        var option = dropdown.find('option[value="' + value + '"]');
        if (option.length === 0) {
            return { value };
        }
        return { value, label: option.text() };
    };

    //fix the issue of select2 inputs not working on bootstrap popups
    let fnModal = $.fn.modal as any;
    if (fnModal?.Constructor?.prototype._enforceFocus) {
        fnModal.Constructor.prototype._enforceFocus = function () { };
        //fnModal.Constructor.prototype.enforceFocus = function () {
        //    $(document)
        //      .off('focusin.bs.modal') // guard against infinite focus loop
        //      .on('focusin.bs.modal', $.proxy(function (e) {
        //          if (document !== e.target && this.$element[0] !== e.target && !this.$element.has(e.target).length) {
        //              if ($(e.target).hasClass('select2-input')) {
        //                  return;
        //              }
        //              this.$element.trigger('focus');
        //          }
        //      }, this));
        //};
    }

})(jQuery);