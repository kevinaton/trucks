(function ($) {
    app.modals.CreateOrEditEmployeeTimeModal = function () {

        var _modalManager;
        var _employeeTimeService = abp.services.app.employeeTime;
        var _timeClassificationService = abp.services.app.timeClassification;
        var _driverService = abp.services.app.driver;
        var _modal;
        var _$form = null;
        var _employeeIdDropdown = null;
        var _timeClassificationDropdown = null;
        var _isManualTimeCheckbox = null;
        var _manualHourAmountElement;
        var _endDateTimeElement;

        var getIsManualTime = () => _isManualTimeCheckbox.is(':checked');
        var setIsManualTime = (val) => _isManualTimeCheckbox.prop('checked', val).change();
        var getTimeClassificationAllowsManualTime = () => _modal.find("#TimeClassificationAllowsManualTime").val().toLowerCase() === "true";
        var setTimeClassificationAllowsManualTime = (val) => _modal.find("#TimeClassificationAllowsManualTime").val(val);

        function updateUIForTimeClassificationAllowsManualTime(onChange) {
            let timeClassificationAllowsManualTime = getTimeClassificationAllowsManualTime();
            if (onChange && !timeClassificationAllowsManualTime) {
                setIsManualTime(false);
            }
            let isManualTime = getIsManualTime();
            _isManualTimeCheckbox.parent().toggle(timeClassificationAllowsManualTime || isManualTime);
            _endDateTimeElement.parent().toggle(!isManualTime);
            _manualHourAmountElement.parent().toggle(isManualTime);
        };

        async function checkIfManualTimeIsAllowed() {
            let employeeId = _employeeIdDropdown.val();
            let timeClassificationId = _timeClassificationDropdown.val();
            if (!employeeId || !timeClassificationId) {
                return;
            }
            _modalManager.setBusy(true);
            abp.ui.setBusy(_modalManager.getModalContent());
            try {
                var employeeTimeClassification = await _driverService.getEmployeeTimeClassificationOrNull({
                    employeeId,
                    timeClassificationId
                });
                if (employeeTimeClassification) {
                    setTimeClassificationAllowsManualTime(employeeTimeClassification.allowForManualTime);
                } else {
                    setTimeClassificationAllowsManualTime(true);
                }
                updateUIForTimeClassificationAllowsManualTime(true);
            } finally {
                _modalManager.setBusy(false);
                abp.ui.clearBusy(_modalManager.getModalContent());
            }
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _modal = _modalManager.getModal();

            _modal.find("#StartDateTime").datetimepickerInit();
            _endDateTimeElement = _modal.find("#EndDateTime");
            _endDateTimeElement.datetimepickerInit();
            _manualHourAmountElement = _modal.find("#ManualHourAmount");

            _employeeIdDropdown = _modal.find("#EmployeeId");
            _employeeIdDropdown.select2Init({
                abpServiceMethod: _employeeTimeService.getUsersSelectList,
                minimumInputLength: 0,
                allowClear: false
            }).on("change", function (e) {
                checkIfManualTimeIsAllowed();
            });

            _timeClassificationDropdown = _modal.find("#TimeClassificationId");
            _timeClassificationDropdown.select2Init({
                abpServiceMethod: _timeClassificationService.getTimeClassificationsSelectList,
                abpServiceParamsGetter: (params) => ({
                    excludeProductionPay: false,
                    allowForManualTime: getIsManualTime() ? true : null,
                    employeeId: _employeeIdDropdown.val()
                }),
                minimumInputLength: 0,
                allowClear: false
            }).on("change", function (e) {
                checkIfManualTimeIsAllowed();
            });

            _isManualTimeCheckbox = _modal.find("#IsManualTime");
            _isManualTimeCheckbox.on("change", function () {
                let isManualTime = getIsManualTime();
                _endDateTimeElement.parent().toggle(!isManualTime);
                _manualHourAmountElement.parent().toggle(isManualTime);
            });

            updateUIForTimeClassificationAllowsManualTime(false);
        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var employeeTime = _$form.serializeFormToObject();
            if (getIsManualTime()) {
                employeeTime.IsManualTime = true;
                employeeTime.EndDateTime = employeeTime.StartDateTime;
            } else {
                employeeTime.manualHourAmount = null;
            }

            if (employeeTime.EndDateTime && !abp.helper.validateStartEndDates(
                { value: employeeTime.StartDateTime, title: _$form.find('label[for="StartDateTime"]').text() },
                { value: employeeTime.EndDateTime, title: _$form.find('label[for="EndDateTime"]').text() }
            )) {
                return;
            }

            try {
                _modalManager.setBusy(true);
                await _employeeTimeService.editEmployeeTime(employeeTime);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditEmployeeTimeModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);