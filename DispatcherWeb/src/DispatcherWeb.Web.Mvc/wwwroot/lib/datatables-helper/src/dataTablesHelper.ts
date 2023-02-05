/// <reference path="../typings/moment.d.ts"/>
/// <reference path="../typings/moment-timezone.d.ts"/>
(function ($) {
    //(window as any).abp = (window as any).abp || {};
    //var abp = (window as any).abp; //todo add the interface
    abp = abp || {};
    abp.helper = abp.helper || {}; //todo add the interface
    abp.helperConfiguration = abp.helperConfiguration || {}; //todo add the interface
    abp.helperConfiguration.dataTables = abp.helperConfiguration.dataTables || {};
    abp.helperConfiguration.dataTables.beforeInit = (abp.helperConfiguration.dataTables.beforeInit || []) as ((options: any) => void)[];
    abp.helperConfiguration.dataTables.afterInit = (abp.helperConfiguration.dataTables.afterInit || []) as ((table, grid, options) => void)[];
    abp.helperConfiguration.dataTables.getNewCheckboxContainer = function (options) {
        options = options || {};
        var container, input;
        container = $('<label class="m-checkbox cell-centered-checkbox">').append(
            input = $('<input type="checkbox" class="minimal">')
        ).append(
            $('<span>')
        );

        if (options.disabled) {
            input.prop('disabled', true);
        }

        if (options.checked) {
            input.attr('checked', 'checked');
        }

        return {
            container,
            input
        };
    };

    abp.helper.dataTables = function () {

        function toAbpData(data) {
            return {
                sorting: getAbpSortingString(data),
                skipCount: data.start,
                maxResultCount: data.length
            };
        }

        function fromAbpResult(result) {
            console.log('fromAbpResult called for:', result);
            if (result && result.items && result.items.length) {
                console.log(result.items[0]);
            }
            return {
                data: result.items,
                recordsTotal: result.totalCount,
                recordsFiltered: result.filteredCount === undefined ? result.totalCount : result.filteredCount
            };
        }

        function getFilterData(filterControls) {
            var result = {};
            $(filterControls || ".filter").each(function () {
                var input = $(this);
                var inputName = abp.utils.toCamelCase(input.attr("name"));
                if (input.is('[type="checkbox"]')) {
                    result[inputName] = input.is(':checked');
                } else {
                    result[inputName] = input.val();
                }

                if (input.is('select') && input.attr('data-display-name')) {
                    var inputDisplayName = abp.utils.toCamelCase(input.attr('data-display-name'));
                    result[inputDisplayName] = input.getSelectedDropdownOption().text();
                }

                if (input.hasClass('filter-date')) {
                    var momentDate = moment(input.val(), "MM/DD/YYYY");
                    result[inputName] = input.val() && momentDate.isValid() ? momentDate.format('L') : '';
                }
            });
            return result;
        }
        function getAbpSortingString(data) {
            var result: string[] = [];
            if (data.order && data.order.length > 0) {
                $.each(data.order, function (index, value) {
                    //value.column = 0
                    //value.dir = asc
                    var columnName = data.columns[value.column].data;
                    columnName = abp.utils.toPascalCaseRecursive(columnName);
                    if (value.dir === "desc")
                        columnName += " DESC";
                    result.push(columnName);
                });
            }
            if (result.length === 0)
                return null;
            return result.join();
        }

        function getRowData(item) {
            var row = $(item).closest('tr');
            if (row.hasClass('child')) {
                row = row.prev('tr.parent');
            }
            var grid = row.closest('table').DataTable();
            return grid.row(row).data() || {};
        }

        function getResponsiveColumn() {
            return {
                data: null,
                width: '20px',
                className: 'control responsive',
                orderable: false,
                render: function () {
                    return '';
                }
            };
        }

        function isValidRowClick(e) {
            return $(e.target).closest('button,a,th,.dataTables_empty').length === 0;
        }

        function renderCheckbox(value) {
            let container = abp.helperConfiguration.dataTables.getNewCheckboxContainer({
                disabled: true,
                checked: value === true
            }).container;
            return $('<div>').append(container).html();
        }

        function renderDate(value) {
            if (value) {
                return moment(value).format('L');
            }
            return '-';
        }

        function renderUtcDate(value) {
            if (value) {
                return moment(value).utc().format('L');
            }
            return '-';
        }

        function renderDateTime(value, emptyValue) {
            if (value) {
                return moment(value).format('L LTS');
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function renderDateShortTime(value, emptyValue) {
            if (value) {
                return moment(value).format('L LT');
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function renderUtcDateTime(value, emptyValue) {
            if (value) {
                var localValue = parseUtcDateTime(value) as moment.Moment;
                return localValue.format('L LTS');
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function renderActualUtcDateTime(value, emptyValue) {
            if (value) {
                return moment(value).utc().format('L') + " " + moment(value).utc().format('LTS');
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function renderActualUtcDateShortTime(value, emptyValue) {
            if (value) {
                return moment(value).utc().format('L') + " " + moment(value).utc().format('LT');
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function parseUtcDateTime(value, emptyValue = undefined) {
            if (value) {
                var localValue = moment
                    .tz(value, ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A"], 'UTC')
                    .tz(abp.helperConfiguration.getIanaTimezoneId());
                return localValue;
            }
            return emptyValue !== undefined ? emptyValue : '';
        }

        function parseDateTimeAsUtc(value, emptyValue) {
            if (value) {
                var utcValue = moment
                    .tz(value, ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A"], 'UTC');
                return utcValue;
            }
            return emptyValue !== undefined ? emptyValue : '';
        }

        function renderTime(value, emptyValue) {
            if (value) {
                return moment(value, ['YYYY-MM-DDTHH:mm:ss', 'hh:mm A']).format("LT");
            }
            return emptyValue !== undefined ? emptyValue : '-';
        }

        function renderMoney(value) {
            return renderText(abp.setting.get('App.General.CurrencySymbol')) + (abp.utils.round(value) || 0).toFixed(2);
            //return $.fn.dataTable.render.number( '\'', '.', 2, abp.setting.get('App.General.CurrencySymbol') ).display(value);
        }

        function formatNumber(value) {
            return ('' + value).replace(/(\d)(?=(?:\d{3})+(?:\.|$))|(\.\d\d?)\d*$/g,
                function (m, s1, s2) {
                    return s2 || (s1 + ',');
                }
            );
        }

        function renderRate(value) {
            return (abp.utils.round(value) || 0).toFixed(2);
        }

        function renderText(d) {
            //code is taken from DataTable's __htmlEscapeEntities
            return typeof d === 'string' ?
                d.replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;') :
                d;
        }

        function renderPhone(number) {
            return number ? '<a href="tel:' + renderText(number) + '">' + renderText(number) + '</a>' : '';
        }

        function getEmptyResult() {
            return {
                data: [],
                recordsTotal: 0,
                recordsFiltered: 0
            };
        }

        function getDateRangeObject(dateRangeString, dateBeginProperyName, dateEndPropertyName) {
            var dateObject = {};
            if (dateRangeString) {
                let dateStringArray = dateRangeString.split(' - ');
                dateObject[dateBeginProperyName] = abp.helper.parseDateToJsonWithoutTime(dateStringArray[0]);
                dateObject[dateEndPropertyName] = abp.helper.parseDateToJsonWithoutTime(dateStringArray[1]);
            }
            return dateObject;
        }

        function getGridEditors() {

            function quantity(editorOptions, gridOptions) {
                let defaultOptions = $.extend({
                    minValue: 0,
                    maxValue: 1000000,
                    allowNull: false
                }, editorOptions);
                $.extend(editorOptions, defaultOptions);
                return decimal(editorOptions, gridOptions);
            }

            function decimal(editorOptions, gridOptions) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = gridOptions.editable.saveCallback;
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let getDisplayValue = editorOptions.getDisplayValue || function (rowData, fieldName) { return rowData[fieldName]; };
                let validate = editorOptions.validate || function () { return true; };
                let selectTextOnFocus = editorOptions.selectTextOnFocus;
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    var editor = $('<input type="text">').appendTo($(cell));
                    var editorClosedCallback = function () {
                        editor.val(getDisplayValue(rowData, fieldName));
                    };
                    editor.val(getDisplayValue(rowData, fieldName));
                    editor.focus(function () {
                        editor.val(rowData[fieldName]);
                        if (selectTextOnFocus) {
                            editor.select();
                        }
                    });
                    editor.focusout(function () {
                        var newValue = $(this).val();
                        if (newValue === (rowData[fieldName] || "0").toString()) {
                            editorClosedCallback();
                            return;
                        }
                        if (isNaN(newValue as number)) {
                            abp.message.error('Please enter a valid number!');
                            editorClosedCallback();
                            return;
                        }
                        if (editorOptions.maxValue !== undefined && parseFloat(newValue as string) > editorOptions.maxValue) {
                            abp.message.error('Please enter a valid number less than ' + editorOptions.maxValue + '!');
                            editorClosedCallback();
                            return;
                        }
                        if (editorOptions.minValue !== undefined && parseFloat(newValue as string) < editorOptions.minValue) {
                            abp.message.error('Please enter a valid number greater than ' + editorOptions.minValue + '!');
                            editorClosedCallback();
                            return;
                        }
                        if (editorOptions.allowNull === false && newValue === "") {
                            newValue = 0;
                            $(this).val(0);
                        }
                        newValue = abp.utils.round(parseFloat(newValue as string));
                        Promise.resolve(validate(rowData, newValue)).then(function (isValid) {
                            if (!isValid) {
                                editorClosedCallback();
                                return;
                            }
                            rowData[fieldName] = newValue;
                            editorClosedCallback();
                            saveCallback(rowData, cell).then(function (editResult) {
                                if (editCompleteCallback) {
                                    editCompleteCallback(editResult, rowData, $(cell));
                                }
                                return editResult;
                            });
                        });
                    });
                };
            }

            function text(editorOptions, gridOptions) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = gridOptions.editable.saveCallback;
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let maxLength = editorOptions.maxLength;
                let isRequired = editorOptions.required;
                let selectTextOnFocus = editorOptions.selectTextOnFocus;
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    var editor = $('<input type="text">').appendTo($(cell));
                    editor.val(rowData[fieldName]);
                    editor.focus(function () {
                        if (selectTextOnFocus) {
                            editor.select();
                        }
                    });
                    editor.focusout(function () {
                        var newValue = $(this).val() as string;
                        if (newValue === (rowData[fieldName] || "").toString()) {
                            return;
                        }
                        if (!newValue && isRequired) {
                            abp.message.error('Value is required');
                            $(this).val(rowData[fieldName]);
                            return;
                        }
                        if (newValue && maxLength && newValue.length > maxLength) {
                            abp.message.error('Value cannot exceed maximum character limit of ' + maxLength);
                            newValue = newValue.substring(0, maxLength);
                            $(this).val(newValue);
                        }

                        rowData[fieldName] = newValue;
                        saveCallback(rowData, cell).then(function (editResult) {
                            if (editCompleteCallback) {
                                editCompleteCallback(editResult, rowData, $(cell));
                            }
                            return editResult;
                        });
                    });
                };
            }

            function textarea(editorOptions, gridOptions) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = gridOptions.editable.saveCallback;
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let maxLength = editorOptions.maxLength;
                let isRequired = editorOptions.required;
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    var cellHeight = Math.max(50 + 6, $(cell).outerHeight() || 0);
                    $(cell).empty();
                    $(cell).addClass('text-area-editable'); //todo see if we still need it
                    var editor = $('<textarea>').height(cellHeight - 6).css('min-height', '50px').appendTo($(cell));
                    if (maxLength) {
                        editor.attr('maxlength', maxLength);
                    }
                    editor.val(rowData[fieldName]);
                    editor.focusout(function () {
                        var newValue = $(this).val() as string;
                        if (newValue === (rowData[fieldName] || "").toString()) {
                            return;
                        }
                        if (!newValue && isRequired) {
                            abp.message.error('Value is required');
                            $(this).val(rowData[fieldName]);
                            return;
                        }
                        if (newValue && maxLength && newValue.length > maxLength) {
                            abp.message.error('Value cannot exceed maximum character limit of ' + maxLength);
                            newValue = newValue.substring(0, maxLength);
                            $(this).val(newValue);
                            return;
                        }

                        rowData[fieldName] = newValue;
                        saveCallback(rowData, cell).then(function (editResult) {
                            if (editCompleteCallback) {
                                editCompleteCallback(editResult, rowData, $(cell));
                            }
                            return editResult;
                        });
                    });
                };
            }

            function time(editorOptions, gridOptions) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = gridOptions.editable.saveCallback;
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let convertDisplayValueToData = editorOptions.convertDisplayValueToData || function (displayValue, rowData) { return displayValue; };
                let convertDataToDisplayValue = editorOptions.convertDataToDisplayValue || function (data) { return abp.helper.dataTables.renderTime(data, '') };
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    var oldDisplayValue = convertDataToDisplayValue(rowData[fieldName]);
                    var setRowValue = function (value) {
                        //rowData[fieldName] = value;
                        oldDisplayValue = value;
                    };
                    var editor = $('<input type="text">').appendTo($(cell));
                    editor.val(oldDisplayValue);
                    editor.focusout(function (e) {
                        var newDisplayValue = $(this).val();
                        if (newDisplayValue === (oldDisplayValue || "")) {
                            return;
                        }
                        if (!newDisplayValue) {
                            abp.message.error('Value is required');
                            $(this).val(oldDisplayValue);
                            return;
                        }
                        if (newDisplayValue && !abp.helper.isTimeString(newDisplayValue)) {
                            abp.message.error('Enter a valid time in a format like 11:24 PM');
                            $(this).val(oldDisplayValue);
                            return;
                        }
                        rowData[fieldName] = convertDisplayValueToData(newDisplayValue, rowData);
                        saveCallback(rowData, cell).then(function (editResult) {
                            setRowValue(newDisplayValue);
                            if (editCompleteCallback) {
                                editCompleteCallback(editResult, rowData, $(cell));
                            }
                            return editResult;
                        });
                    });
                };
            }

            function datetime(editorOptions, gridOptions, columnIndex) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = gridOptions.editable.saveCallback;
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let convertDisplayValueToData = editorOptions.convertDisplayValueToData || function (displayValue, rowData) { return moment(displayValue, ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A", 'hh:mm A']).format('YYYY-MM-DDTHH:mm:ss') + "Z"; };
                let convertDataToDisplayValue = editorOptions.convertDataToDisplayValue || function (data) { return moment(data, ['YYYY-MM-DDTHH:mm:ss', "M/D/YYYY h:mm:ss A", 'hh:mm A']).format("L LT"); };
                let isRequired = editorOptions.required;
                let columnOptions = gridOptions.columns[columnIndex];
                columnOptions.width = columnOptions.width || "150px";
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    var oldDisplayValue = convertDataToDisplayValue(rowData[fieldName]);
                    var setRowValue = function (value) {
                        //rowData[fieldName] = value;
                        oldDisplayValue = value;
                    };
                    var editor = $('<input type="text" class="form-control">');
                    $('<div>').css('position', 'relative').append(editor).appendTo($(cell));
                    editor.val(oldDisplayValue);
                    editor.datetimepickerInit();
                    var delayedSave: number | null = null;
                    editor.focusout(function () {
                        if (delayedSave) {
                            clearTimeout(delayedSave);
                        }
                        delayedSave = setTimeout(save, 25);
                    });
                    var save = function () {
                        delayedSave = null;
                        var newDisplayValue = editor.val();
                        if (newDisplayValue === (oldDisplayValue || "")) {
                            return;
                        }
                        if (!newDisplayValue && isRequired) {
                            abp.message.error('Value is required');
                            editor.val(oldDisplayValue);
                            return;
                        }
                        rowData[fieldName] = convertDisplayValueToData(newDisplayValue, rowData);
                        saveCallback(rowData, cell).then(function (editResult) {
                            if (editCompleteCallback) {
                                editCompleteCallback(editResult, rowData, $(cell));
                            }
                            setRowValue(newDisplayValue);
                        });
                    };
                };
            }

            function dropdown(editorOptions, gridOptions) {
                let idField = editorOptions.idField;
                let nameField = editorOptions.nameField;
                let dropdownOptions = editorOptions.dropdownOptions;
                let saveCallback = gridOptions.editable.saveCallback;
                let editStartingCallback = editorOptions.editStartingCallback || function (rowData, cell, selectedOption) { };
                let editCompleteCallback = editorOptions.editCompleteCallback;
                let validate = editorOptions.validate || function () { return Promise.resolve(true); };
                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    var editorId = abp.helper.getUniqueElementId();
                    var editor = $('<select></select>').attr('id', editorId).appendTo($(cell));
                    editor.append($('<option value="">&nbsp;</option>'));
                    if (rowData[idField]) {
                        editor.append($('<option selected></option>').text(rowData[nameField]).attr("value", rowData[idField]));
                    }
                    let originalAbpServiceParamsGetter = dropdownOptions.abpServiceParamsGetter;
                    let dropdownExtendedOptions = $.extend({}, dropdownOptions, {
                        abpServiceParamsGetter: function (params) {
                            let result = {};
                            if (originalAbpServiceParamsGetter) {
                                $.extend(result, originalAbpServiceParamsGetter(params, rowData));
                            }
                            return result;
                        }
                    });
                    editor.select2Init(dropdownExtendedOptions);

                    editor.closest('td').on('focus', '.select2-selection.select2-selection--single', function (e) {
                        //$(this).closest(".select2-container").siblings('select:enabled').select2('open');
                        editor.select2('open');
                    });

                    // steal focus during close - only capture once and stop propagation
                    editor.on('select2:closing', function (e) {
                        $(e.target).data("select2").$selection.one('focus focusin', function (e) {
                            e.stopPropagation();
                        });
                    });

                    var editorData = {
                        tabCallback: null as (null | (() => void))
                    };

                    //var tabCallback = null;
                    editor.closest('td').on('keyup', /*'.select2-search__field',*/ function (e) {
                        if (e.which === 9) { //Tab
                            //e.preventDefault();
                            //editor.select2("close");
                            editorData.tabCallback = function () {
                                setTimeout(function () {
                                    var inputs = editor.closest('table').find(':input:visible');
                                    inputs.eq(inputs.index(editor) + 1).focus();
                                    editorData.tabCallback = null;
                                }, 0);
                            };
                        }
                    });

                    editor.on('select2:opening', function (e) {
                        //console.log('opening, unselectingIgnoreFocus: ' + unselectingIgnoreFocus);
                        if (unselectingIgnoreFocus) {
                            unselectingIgnoreFocus = false;
                            e.preventDefault();
                        }
                    });

                    var unselectingIgnoreFocus = false;
                    var unselecting = false;
                    editor.on('select2:unselecting', function () {
                        //console.log('unselecting');
                        unselecting = true;
                        unselectingIgnoreFocus = true;
                        setTimeout(() => { unselectingIgnoreFocus = false }, 200);
                    });
                    editor.on('select2:unselect', function () {
                        //console.log('unselect');
                        unselecting = false;
                        unselectingIgnoreFocus = true;
                        setTimeout(() => { unselectingIgnoreFocus = false }, 200);
                    });
                    editor.on('select2:closing', function () {
                        //console.log('closing');
                        setTimeout(function () {
                            //'closing' is called twice in a row sometimes
                            let callback = editorData.tabCallback;
                            editorData.tabCallback = null;
                            callback && callback();
                        }, 200);
                    });

                    editor.on('select2:close select2:clear', async function () {
                        var newValue = $(this).val();
                        let newText = editor.find('option:selected').text();
                        if (unselecting) {
                            editorData.tabCallback && editorData.tabCallback();
                            return;
                        }
                        if (newValue === (rowData[idField] || "").toString()) {
                            editorData.tabCallback && editorData.tabCallback();
                            return;
                        }

                        let dropdownData = editor.select2('data');
                        let selectedOption = dropdownData && dropdownData.length && dropdownData[0];

                        try {
                            abp.ui.setBusy($(cell));
                            var isValid = await Promise.resolve(validate(rowData, newValue, $(cell)));
                            if (!isValid) {
                                editor.val(rowData[idField]).trigger('change.select2');
                                return;
                            }
                            rowData[idField] = newValue;
                            rowData[nameField] = newText;
                            await Promise.resolve(editStartingCallback(rowData, $(cell), selectedOption));
                            var editResult = await saveCallback(rowData, cell);
                            if (editCompleteCallback) {
                                editCompleteCallback(editResult, rowData, $(cell), selectedOption);
                            }
                            editorData.tabCallback && editorData.tabCallback();
                            if (dropdownOptions.abpServiceMethod) {
                                editor.removeUnselectedOptions();
                            }
                        }
                        finally {
                            abp.ui.clearBusy($(cell));
                        }
                    });
                };
            }


            //old dropdown style when dropdown is only created on click and destoyed on losing the focus
            //function handleDropdownCellCreation(idField, nameField, dropdownOptions) {
            //    return function (cell, cellData, rowData, rowIndex, colIndex) {
            //        var getCellText = function () {
            //            return rowData[nameField];
            //        };
            //        $(cell).text(getCellText());
            //        var cellIsActive = false;
            //        $(cell).click(function () {
            //            if (cellIsActive) {
            //                return;
            //            }
            //            cellIsActive = true;
            //            $(cell).text('');
            //            $(cell).addClass('cell-editable');
            //            var editor = $('<select></select>').appendTo($(cell));
            //            editor.append($('<option selected></option>').text(rowData[nameField]).attr("value", rowData[idField]));
            //            editor.focus();
            //            editor.select2Init(dropdownOptions);
            //            editor.select2("open");
            //
            //            var closeEditor = function () {
            //                setTimeout(function () {
            //                    editor.remove();
            //                    $(cell).text(getCellText());
            //                    $(cell).removeClass('cell-editable');
            //                    cellIsActive = false;
            //                }, 100);
            //            };
            //            var unselecting = false;
            //            editor.on('select2:unselecting', function () {
            //                unselecting = true;
            //            });
            //            editor.on('select2:unselect', function () {
            //                unselecting = false;
            //            });
            //            editor.on('select2:close', function () {
            //                var newValue = $(this).val();
            //                if (unselecting) {
            //                    return;
            //                }
            //                if (newValue === (rowData[idField] || "").toString()) {
            //                    closeEditor();
            //                    return;
            //                }
            //                rowData[idField] = newValue;
            //                rowData[nameField] = editor.find('option:selected').text();
            //                var ticket = getTicketFromRowData(rowData);
            //                editTicket(ticket, cell).then(function (editResult) {
            //                    //console.log({ editResult.ticket });
            //                }).always(function () {
            //                    closeEditor();
            //                });
            //            });
            //        });
            //    };
            //}

            class DelayedSaveQueue {
                //todo check the compilation result
                options: {
                    delay?: number;
                    saveCallback: (updatedRows: any[], cells: any[]) => Promise<any> | any;
                    setBusyOnSave: boolean;
                };
                queue: any[];
                queuedEditors: any[];
                timer: number | null;

                constructor(options) {
                    this.options = options;
                    this.options.delay = (this.options.delay === undefined || this.options.delay === null) ? 500 : this.options.delay;
                    this.options.saveCallback = this.options.saveCallback || function (updatedRows, cells) { console.error('saveCallback is missing in DelayedSaveQueue options'); };
                    this.options.setBusyOnSave = this.options.setBusyOnSave === false ? false : true; //defaut is true
                    this.queue = [];
                    this.queuedEditors = [];
                    this.timer = null;
                }

                add(updatedRow, updatedEditor) {
                    if (!this.queue.includes(updatedRow)) {
                        this.queue.push(updatedRow);
                    }
                    if (!this.queuedEditors.includes(updatedEditor)) {
                        this.queuedEditors.push(updatedEditor);
                    }
                    if (this.timer) {
                        clearTimeout(this.timer);
                        this.timer = null;
                    }
                    this.timer = setTimeout(() => {
                        let queue = this.queue;
                        this.queue = [];
                        let queuedEditors = this.queuedEditors;
                        this.queuedEditors = [];
                        this.timer = null;

                        let setBusyOnSave = this.options.setBusyOnSave;
                        if (setBusyOnSave) {
                            queuedEditors.forEach(x => abp.ui.setBusy(x));
                        }

                        Promise.resolve(
                            this.options.saveCallback(queue, queuedEditors)
                        ).then(function () {
                            if (setBusyOnSave) {
                                queuedEditors.forEach(x => abp.ui.clearBusy(x));
                            }
                        }, function (e) {
                            if (setBusyOnSave) {
                                queuedEditors.forEach(x => abp.ui.clearBusy(x));
                            }
                            throw e;
                        });
                    }, this.options.delay);
                }
            }

            function checkbox(editorOptions, gridOptions, columnIndex) {
                let fieldName = editorOptions.fieldName;
                let saveCallback = editorOptions.saveCallback || gridOptions.editable.saveCallback;

                let columnOptions = gridOptions.columns[columnIndex];

                var className = (columnOptions.className || '').split(' ');
                if (!className.includes('checkmark')) {
                    className.push('checkmark');
                }
                if (!className.includes('text-center')) {
                    className.push('text-center');
                }
                columnOptions.className = className.join(' ');

                columnOptions.responsiveDispalyInHeaderOnly = true;
                columnOptions.width = columnOptions.width || "30px";
                //renderer will only be used for readonly cells/rows
                columnOptions.render = columnOptions.render || function (data, type, full, meta) {
                    //Should we have a distinct style for readonly checkboxes?
                    return abp.helper.dataTables.renderCheckbox(full[fieldName]);
                };

                let columnTitle = columnOptions.title || "";

                let uniqueCheckboxClass = abp.helper.getUniqueElementId();

                if (editorOptions.addHeaderCheckbox) {
                    let oldHeaderCallback = gridOptions.headerCallback;
                    gridOptions.headerCallback = function (thead, data, start, end, display) {
                        let headerCell = $(thead).find('th').eq(columnIndex).html('');
                        let headerCheckboxContainer = abp.helperConfiguration.dataTables.getNewCheckboxContainer();
                        let headerCheckbox = headerCheckboxContainer.input as JQuery<HTMLElement>;
                        headerCheckboxContainer.container.addClass('checkbox-only-header-label');
                        headerCell.append(headerCheckboxContainer.container);
                        headerCheckbox.change(function () {
                            var newValue = headerCheckbox.is(":checked");
                            let selector = newValue ? ':not(:checked)' : ':checked';
                            let targetCheckboxes = headerCheckbox.closest('table').find('input.' + uniqueCheckboxClass + selector);
                            targetCheckboxes.prop('checked', newValue).change();
                        });
                        if (oldHeaderCallback) {
                            oldHeaderCallback.apply(this, arguments);
                        }
                    };
                }

                return function (cell, cellData, rowData, rowIndex, colIndex) {
                    $(cell).empty();
                    let checkboxContainer = abp.helperConfiguration.dataTables.getNewCheckboxContainer({
                        checked: rowData[fieldName]
                    });
                    let editor = checkboxContainer.input as JQuery<HTMLElement>;
                    editor.addClass(uniqueCheckboxClass);
                    $(cell).append(checkboxContainer.container);

                    //editor.data('uniqueCheckboxClass', uniqueCheckboxClass);
                    //editor.data('rowData', rowData);
                    editor.change(function () {
                        var newValue = editor.is(":checked");
                        if (newValue === rowData[fieldName]) {
                            return;
                        }
                        rowData[fieldName] = newValue;
                        saveCallback(rowData, cell);
                    });
                };
            }

            return {
                quantity: quantity,
                decimal: decimal,
                text: text,
                textarea: textarea,
                time: time,
                datetime: datetime,
                dropdown: dropdown,
                checkbox: checkbox,
                DelayedSaveQueue: DelayedSaveQueue
            };
        };

        return {
            toAbpData: toAbpData,
            fromAbpResult: fromAbpResult,
            getFilterData: getFilterData,
            getRowData: getRowData,
            getResponsiveColumn: getResponsiveColumn,
            isValidRowClick: isValidRowClick,
            renderCheckbox: renderCheckbox,
            renderDate: renderDate,
            renderUtcDate: renderUtcDate,
            renderDateTime: renderDateTime,
            renderDateShortTime: renderDateShortTime,
            renderUtcDateTime: renderUtcDateTime,
            renderActualUtcDateTime: renderActualUtcDateTime,
            renderActualUtcDateShortTime: renderActualUtcDateShortTime,
            parseUtcDateTime: parseUtcDateTime, //parse utc datetime and return local datetime
            parseDateTimeAsUtc: parseDateTimeAsUtc, //parse any datetime value as it was in utc and keep it in utc/
            renderTime: renderTime,
            renderMoney: renderMoney,
            formatNumber: formatNumber,
            renderRate: renderRate,
            renderText: renderText,
            renderPhone: renderPhone,
            getEmptyResult: getEmptyResult,
            getDateRangeObject: getDateRangeObject,
            editors: getGridEditors(),
        };
    }();

    function getSelectionColumnObject(selectionColumnOptions) {
        var rowSelectionClassName = "selection-column-row-selection";
        var rowSelectAllClassName = "selection-column-row-select-all";
        var rowSelectionClass = '.' + rowSelectionClassName;
        var rowSelectAllClass = '.' + rowSelectAllClassName;
        var _table: JQuery<any> | null = null;
        var _grid: DataTables.Api | null = null;
        var _options = null;
        var selectionChangedCallbacks: ((selectedRowsCount: number) => void)[] = []; //expected an array of "function (selectedRowsCount)" handlers

        function addColumn(options) {
            var columnPosition = options.responsive ? 1 : 0;
            options.columns.splice(columnPosition, 0, {
                data: null,
                orderable: false,
                render: function (data, type, full, meta) {
                    let checkboxContainer = abp.helperConfiguration.dataTables.getNewCheckboxContainer();
                    checkboxContainer.container.addClass('checkbox-only-cell-label');
                    checkboxContainer.input.addClass(rowSelectionClassName);
                    return $('<div>').append(checkboxContainer.container).html();
                },
                className: "checkmark checkbox-only-cell",
                width: "25px",
                title: " "
            });
            var oldHeaderCallback = options.headerCallback;
            options.headerCallback = function (thead, data, start, end, display) {
                var headerCell = $(thead).find('th').eq(columnPosition).html('');
                var headerCheckboxContainer = abp.helperConfiguration.dataTables.getNewCheckboxContainer();
                headerCheckboxContainer.input.addClass(rowSelectAllClassName);
                headerCheckboxContainer.container.addClass('checkbox-only-header-label');
                headerCell.append(headerCheckboxContainer.container);
                if (oldHeaderCallback)
                    oldHeaderCallback(thead, data, start, end, display);
            };

            let originalDrawCallback = options.drawCallback;
            options.drawCallback = (settings) => {
                if (originalDrawCallback) {
                    originalDrawCallback(settings);
                }
                runSelectedChangedCallbacks();
            };
        }

        function getSelectedRowsCount() {
            if (_table) {
                return _table.find(rowSelectionClass + ':checked').length;
            }
            return 0;
        }

        function runSelectedChangedCallbacks() {
            var selectedRowsCount = getSelectedRowsCount();
            $.each(selectionChangedCallbacks, function (ind, callback) {
                callback(selectedRowsCount);
            });
        }

        function handleColumn(table: JQuery<any>, grid: DataTables.Api, options) {
            _table = table;
            _grid = grid;
            _options = options;

            table.on('change', rowSelectAllClass, function () {
                if ($(this).is(":checked")) {
                    table.find(rowSelectionClass).not(':checked').prop('checked', true).change();
                } else {
                    table.find(rowSelectionClass + ':checked').prop('checked', false).change();
                }
            });

            table.on('change', rowSelectionClass, function () {
                if ($(this).is(":checked")) {
                    if (table.find(rowSelectionClass).not(':checked').length === 0) {
                        table.find(rowSelectAllClass).not(':checked').prop('checked', true).change();
                    }
                } else {
                    if (table.find(rowSelectionClass + ':checked').length === 0) {

                        table.find(rowSelectAllClass + ':checked').prop('checked', false).change();
                    }
                }
                runSelectedChangedCallbacks();
            });
        }

        function getSelectedRowsIds() {
            return getSelectedRows().map(function (x) { return x.id; });
        }

        function getSelectedRows() {
            if (!_table) {
                console.error("_table is not set");
                throw Error('_table is not set');
            }
            var rows: any[] = [];
            _table.find(rowSelectionClass + ':checked').each(function () {
                rows.push(abp.helper.dataTables.getRowData(this));
            });
            return rows;
        }

        return $.extend(selectionColumnOptions, {
            addColumn: addColumn,
            handleColumn: handleColumn,
            selectionChangedCallbacks: selectionChangedCallbacks,
            getSelectedRowsIds: getSelectedRowsIds,
            getSelectedRows: getSelectedRows
        });
    }

    function getColumnOptions(options, index) {
        if (!options) {
            return null;
        }
        if (options.columns && options.columns.length > index) {
            return options.columns[index];
        }
        if (options.columnDefs) {
            for (var i = 0; i < options.columnDefs; i++) {
                var colDef = options.columnDefs[i];
                if (colDef.targets === index) {
                    return colDef;
                }
            }
        }
        return null;
    }

    abp.helperConfiguration.dataTables.defaultOptions = {
        responsive: true,
        serverSide: true,
        searching: false,
        processing: true,

        lengthMenu: [10, 20, 50, 100, 1000],
        pageLength: 10,

        dom: `<'row bottom'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 dataTables_pager'p>>
                  <'row'<'col-sm-12'tr>>
                  <'row bottom'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 dataTables_pager'p>>`
    };

    jQuery.fn.DataTableInit = function (userOptions) {
        var $element = $(this);
        var defaultOptions = abp.helperConfiguration.dataTables.defaultOptions;
        var options = $.extend(true, {}, defaultOptions, userOptions);
        $element.data('options', options);

        let beforeInitCallbacks = abp.helperConfiguration.dataTables.beforeInit as ((options: any) => void)[];
        if (beforeInitCallbacks) {
            beforeInitCallbacks.forEach(x => x(options));
        }

        if (options.selectionColumnOptions) {
            options.selectionColumnOptions = userOptions.selectionColumnOptions = getSelectionColumnObject(userOptions.selectionColumnOptions);
            options.selectionColumnOptions.addColumn(options);
        }

        if (options.responsive) {
            options.responsive = options.responsive === true ? {} : options.responsive;
            options.responsive.details = options.responsive.details || {};
            options.responsive.details.renderer = function (api, rowIdx, columns) {
                var data = $.map(columns, function (col) {
                    var colOptions = getColumnOptions(options, col.columnIndex) || {};
                    return col.hidden && !colOptions.responsiveDispalyInHeaderOnly ?
                        '<li data-dtr-index="' + col.columnIndex + '" data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                        '<span class="dtr-title">' +
                        //abp.helper.dataTables.renderText(col.title) +
                        col.title +
                        '</span> ' +
                        '<span class="dtr-data">' +
                        //abp.helper.dataTables.renderText(col.data) +
                        //col.data seems to be already passed through the column's own renderer, so potential XSS should already be removed
                        col.data +
                        '</span>' +
                        '</li>' :
                        '';
                }).join('');

                return data ?
                    $('<ul data-dtr-index="' + rowIdx + '" class="dtr-details"/>').append(data) :
                    false;
            };

            //set "data: null" for all 'responsive control' columns to see if it fixes #11077
            let responsiveControlColumns = options.columns.filter(c => c.className && c.className.split(' ').includes('control') && c.className.split(' ').includes('responsive'));
            //console.log('responsive control columns: ', responsiveControlColumns);
            responsiveControlColumns.filter(c => !c.data).forEach(c => c.data = null);
        }

        if (options.footerCallback && $element.find('tfoot').length === 0) {
            var footer = $("<tfoot></tfoot>");
            var footerRowCount = options.footerRowCount || 1;
            for (var j = 0; j < footerRowCount; j++) {
                var footerRow = $("<tr></tr>").appendTo(footer);
                for (var i = 0; i < options.columns.length; i++) {
                    let th = $("<th></th>");
                    footerRow.append(th);
                    let className = options.columns[i].className;
                    if (className && className.includes('control') && className.includes('responsive')) {
                        th.addClass('control responsive').hide();
                    }
                }
            }
            $element.append(footer);

            //if (options.footerRowCount > 1) {
            //    let originalFooterCallback = options.footerCallback;
            //    options.footerCallback = function (tfoot, data, start, end, display) {
            //        var api = this.api();
            //        var footerRows = $('tr', api.table().footer());
            //        var responsiveCell = footerRows.eq(0).find('th.control.responsive');
            //        if (responsiveCell.length) {
            //            for (var j = 1; j < options.footerRowCount; j++) {
            //                if (footerRows.eq(j).find('th.control.responsive').length === 0) {
            //                    footerRows.eq(j).find('th:eq(0)').addClass('control responsive').toggle(responsiveCell.is(':visible'));
            //                }
            //            }
            //        }
            //        originalFooterCallback.apply(this, arguments);
            //    };
            //}
        }

        let oldHeaderCallback = options.headerCallback;
        options.headerCallback = function (thead, data, start, end, display) {
            options.columns.filter((c) => c.visible === undefined ? true : c.visible).forEach((columnOptions, i) => {
                if (columnOptions.titleHoverText) {
                    var headerCell = $(thead).find('th').eq(i);
                    headerCell.attr('title', abp.helper.dataTables.renderText(columnOptions.titleHoverText));
                }
            });
            if (oldHeaderCallback) {
                oldHeaderCallback.apply(this, arguments);
            }
        };

        if (options.stateSave) {
            let oldStateSaveParamsFunc = options.stateSaveParams;
            options.stateSaveParams = function (settings, data) {
                for (var i = 0; i < data.columns.length; i++) {
                    delete data.columns[i].visible;
                }
                if (oldStateSaveParamsFunc) {
                    oldStateSaveParamsFunc(settings, data);
                }
            };
        }

        if (options.editable) {
            let saveCallback = options.editable.saveCallback;
            let isRowReadOnly = options.editable.isReadOnly || function () { return false };
            options.editable.saveCallback = function (rowData, cell) {
                abp.ui.setBusy(cell);
                return Promise.resolve(saveCallback && saveCallback(rowData, cell)).then(function (result) {
                    abp.ui.clearBusy(cell);
                    return result;
                }, function () {
                    abp.ui.clearBusy(cell);
                });
            };
            options.columns.forEach((columnOptions, columnIndex) => {
                if (columnOptions.editable && columnOptions.editable.editor) {
                    columnOptions.editable.fieldName = columnOptions.editable.fieldName || columnOptions.data;
                    var className = (columnOptions.className || '').split(' ');
                    if (!className.includes('cell-editable')) {
                        className.push('cell-editable');
                        columnOptions.className = className.join(' ');
                    }
                    let createdCellHandler = columnOptions.editable.editor(columnOptions.editable, options, columnIndex);
                    let previousHandler = columnOptions.createdCell;
                    let isCellReadOnly = columnOptions.editable.isReadOnly || function (rowData, rowIsReadOnly) { return rowIsReadOnly; };
                    columnOptions.createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                        let rowIsReadOnly = isRowReadOnly(rowData);
                        let cellIsReadOnly = isCellReadOnly(rowData, rowIsReadOnly);
                        if (cellIsReadOnly) {
                            $(cell).removeClass('cell-editable');
                            return;
                        }
                        createdCellHandler.apply(this, arguments);
                        if (previousHandler) {
                            previousHandler.apply(this, arguments);
                        }
                    };
                }
            });
        }

        $.each(options.columns, function (ind, column) {
            if (column.render === undefined) {
                column.render = $.fn.dataTable.render.text();
            }
        });

        if (options.columnDefs) {
            console.error('Please use columns instead of columnDefs');
            $.each(options.columnDefs, function (ind, column) {
                if (column.render === undefined) {
                    column.render = $.fn.dataTable.render.text();
                }
            });
        }

        $element.attr('width', $element.attr('width') || '100%');

        var api = $element.DataTable(options);

        if (options.selectionColumnOptions) {
            options.selectionColumnOptions.handleColumn($element, api, options);
        }

        let afterInitCallbacks = abp.helperConfiguration.dataTables.afterInit as ((table, grid, options) => void)[];
        if (afterInitCallbacks) {
            afterInitCallbacks.forEach(x => x($element, api, options));
        }

        return api;
    };

    
    // Source: https://datatables.net/blog/2016-02-26
    jQuery.fn.dataTable.render.ellipsis = function (cutoff: number, wordbreak: boolean, escapeHtml: boolean) {
        var esc = function (t) {
            return t
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;');
        };

        //type FunctionColumnRender = (data: any, type: any, row: any, meta: CellMetaSettings) => any;
        return function (d, type, row) {
            // Order, search and type get the original data
            if (type !== 'display') {
                return d;
            }

            if (typeof d !== 'number' && typeof d !== 'string') {
                return d;
            }

            d = d.toString(); // cast numbers

            if (d.length <= cutoff) {
                return d;
            }

            var shortened = d.substr(0, cutoff - 1);

            // Find the last white space character in the string
            if (wordbreak) {
                shortened = shortened.replace(/\s([^\s]*)$/, '');
            }

            // Protect against uncontrolled HTML input
            if (escapeHtml) {
                shortened = esc(shortened);
            }

            return '<span class="ellipsis" title="' + esc(d) + '">' + shortened + '&#8230;</span>';
        };
    };

    

})(jQuery);


/// <reference types="jquery" />
interface JQuery {
    DataTableInit: (userOptions: any) => DataTables.Api; //todo: define the userOptions type instead of using any
}

/// <reference types="datatables.net" />
declare namespace DataTables {
    interface StaticRenderFunctions {
        ellipsis(cutoff: number, wordbreak: boolean, escapeHtml: boolean): FunctionColumnRender; //ObjectColumnRender;
    }
}