(function () {
    $(function () {

        var _$textsTable = $('#TextsTable');
        var _languageService = abp.services.app.language;

        var _editTextModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Languages/EditTextModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Languages/_EditTextModal.js',
            modalClass: 'EditLanguageTextModal'
        });

        var getFilter = function () {
            return {
                targetLanguageName: $('#TextTargetLanguageSelectionCombobox').val(),
                sourceName: $('#TextSourceSelectionCombobox').val(),
                baseLanguageName: $('#TextBaseLanguageSelectionCombobox').val(),
                targetValueFilter: $('#TargetValueFilterSelectionCombobox').val(),
                filterText: $('#TextFilter').val()
            };
        }

        var dataTable = _$textsTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _languageService.getLanguageTexts,
                inputFilter: getFilter
            },
            columns: [
                {
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0
                },
                {
                    targets: 1,
                    data: "key",
                    render: function (key) {
                        return '<span title="' + key + '">' + app.utils.string.truncate(key, 32) + '</span>';
                    }
                },
                {
                    targets: 2,
                    data: "baseValue",
                    render: function (baseValue) {
                        return $("<span/>").attr("title", (baseValue || '')).text((app.utils.string.truncate(baseValue, 32) || ''))[0].outerHTML;
                    }
                },
                {
                    targets: 3,
                    data: "targetValue",
                    render: function (targetValue) {
                        return $("<span/>").attr("title", (targetValue || '')).text((app.utils.string.truncate(targetValue, 32) || ''))[0].outerHTML;
                    }
                },               
                {
                targets: 4,
                data: null,
                orderable: false,
                autoWidth: false,
				width: "10px",
				defaultContent: '',
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                            className: "btn btn-sm btn-default",
                            action: function (data) {                              
                                _editTextModal.open({
                                    sourceName: $('#TextSourceSelectionCombobox').val(),
                                    baseLanguageName: $('#TextBaseLanguageSelectionCombobox').val(),
                                    languageName: $('#TextTargetLanguageSelectionCombobox').val(),
                                    key: data.record.key
                                });
                            }
                        }]
                    }
                }

            ]
        });

        $('#TextBaseLanguageSelectionCombobox').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#TextTargetLanguageSelectionCombobox').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#TextSourceSelectionCombobox').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#TargetValueFilterSelectionCombobox').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#RefreshTextsButton').click(function (e) {
            e.preventDefault();
            loadTable();
        });

        $('#TextsFilterForm select').change(function () {
            loadTable();
        });

        $('#TextFilter').focus();

        var loadTable = function () {
            dataTable.ajax.reload();
        }

        abp.event.on('app.editLanguageTextModalSaved', function () {
            dataTable.ajax.reloadPage();
        });

    });
})();