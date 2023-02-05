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

    abp.enums = abp.enums || {};
    enum Select2ItemKind {
        addNewItem = -1,
    };
    abp.enums.select2ItemKind = Select2ItemKind;

    function escapeRegExp(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
    }

    function escapeRegExpReplacement(string) {
        return string.replace(/\$/g, '$$$$');
    }

    function renderSelect2MatchedText(text: string, match: string) {
        if (!match || !text) {
            return $('<span>').text(text);
        }

        var safeTextHtml = $('<span>').text(text).html();
        var safeMatchHtml = $('<span>').text(match).html();

        var safeResultHtml = safeTextHtml.replace(
            new RegExp(escapeRegExp(safeMatchHtml), 'gi'),
            (safeCaseSensitiveMatchHtml) => `<span class="select2-rendered__match">${safeCaseSensitiveMatchHtml}</span>`
        );

        return $(`<span>${safeResultHtml}</span>`);
    }

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

                    if (userOptions.addItemCallback
                        && params.page === 1
                        && params.term
                        && !data.items.some(i => (i.name || i.text || '').toLowerCase() === params.term?.toLowerCase())) {
                        data.items = [
                            {
                                id: Select2ItemKind.addNewItem,
                                name: params.term,
                                select2ItemKind: Select2ItemKind.addNewItem,
                                select2PreventHighlightingByDefault: true
                            },
                            ...data.items
                        ];
                    }

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
                if (data.loading) {
                    return 'Loading...';
                }
                if (!data.text && !data.name && data.element && data.element.is('option')) {
                    data.text = data.element.text();
                }
                data.text = data.name;

                var select2 = $element.data('select2');
                var term = select2?.results?.lastParams?.term || select2?.$dropdown.find("input").val();

                if (data.select2ItemKind) {
                    switch (data.select2ItemKind) {
                        case Select2ItemKind.addNewItem:
                            return $('<span>').append(
                                $('<span class="select2-results__add-new-icon">').append(
                                    $('<i class="fa fa-plus">')
                                )
                            ).append(
                                $('<span>').text('Add ')
                            ).append(
                                //renderSelect2MatchedText(data.text, term)
                                $('<span>').text(data.text)
                            );
                    }
                }

                return renderSelect2MatchedText(data.text, term);
            }
        };
        if (!userOptions.abpServiceMethod) {
            //defaultOptions.ajax.transport = undefined;
            defaultOptions.ajax = undefined;
            //defaultOptions.templateResult = undefined;
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

        if (userOptions.addItemCallback) {
            $element.on('select2:selecting', function (e) {
                var selectedOption = (e.params as any)?.args?.data;
                var select2 = $element.data('select2');
                if (selectedOption?.select2ItemKind === Select2ItemKind.addNewItem) {
                    e.preventDefault();
                    (select2 as any)?.close();
                    abp.ui.setBusy(select2?.$container);
                    userOptions.addItemCallback(selectedOption.text).then(result => {
                        if (result) {
                            abp.helper.ui.addAndSetDropdownValue($element, result.id, result.name);
                        }
                    }).finally(() => {
                        abp.ui.clearBusy(select2?.$container);
                    });
                }
            });
        }

        $element.change(async function () {
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