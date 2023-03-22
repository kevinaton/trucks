(function ($) {
    app.modals.PrintOrdersModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;
        var _dateInput = null;
        var _hidePricesInput = null;
        var _includeTicketsInput = null;
        var _printDailySummary = null;
        var _printDailyDetail = null;
        var _printOrdersWithDeliveryInfo = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            var saveButton = _modalManager.getModal().find('.save-button');
            saveButton.find('span').text('Print');
            saveButton.find('i.fa-save').removeClass('fa-save').addClass('fa-print');

            _modalManager.getModal().on('shown.bs.modal', function () {
                saveButton.focus();
                //_$form.find("#DateFilter").datepicker('hide');
            });

            abp.helper.ui.initControls();

            _dateInput = _$form.find("#DateFilter");
            //_dateInput.val(moment().format("MM/DD/YYYY"));
            _dateInput.datepickerInit();

            _hidePricesInput = _$form.find("#HidePrices");
            _includeTicketsInput = _$form.find("#IncludeTickets");

            _printDailySummary = _$form.find("#PrintDailySummary");
            _printDailyDetail = _$form.find("#PrintDailyDetail");
            _printOrdersWithDeliveryInfo = _$form.find("#PrintOrdersWithDeliveryInfo");
            var printOrdersOptions = _$form.find('[name="PrintOrdersOption"]');
            printOrdersOptions.on('change', function (e) {
                if (!$(this).is(_printOrdersWithDeliveryInfo)) {
                    _$form.find("#IncludeTicketsBlock").hide();
                } else {
                    _$form.find("#IncludeTicketsBlock").show();
                }
            }).change.apply(_printDailySummary);
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var date = _dateInput.val();

            var dateValue = moment(date, "MM/DD/YYYY", true);
            if (!dateValue.isValid()) {
                abp.message.warn("Date is incorrect!");
                return;
            }

            var hidePrices = _hidePricesInput.is(":checked");
            var includeTickets = _includeTicketsInput.is(":checked");

            var noDataMessage = 'There are no orders to print for ' + date + '.';
            var reportParams = {};

            if (_printDailySummary.is(":checked")) {
                reportParams = {
                    date: date,
                    hidePrices: hidePrices
                };
                _orderService.doesOrderSummaryReportHaveData(reportParams).done(function (result) {
                    if (!result) {
                        abp.message.warn(noDataMessage);
                        return;
                    }
                    _modalManager.close();
                    window.open(abp.appPath + 'app/orders/GetOrderSummaryReport?' + $.param(reportParams));
                });
            }

            if (_printDailyDetail.is(":checked")) {
                reportParams = {
                    date: date,
                    hidePrices: hidePrices
                };
                printWorkOrderReport(reportParams);
            }

            if (_printOrdersWithDeliveryInfo.is(":checked")) {
                reportParams = app.order.getOrdersWithDeliveryInfoReportOptions({
                    date: date,
                    hidePrices: hidePrices,
                    includeTickets: includeTickets
                });
                printWorkOrderReport(reportParams);
            }

            function printWorkOrderReport(reportParams) {
                _orderService.doesWorkOrderReportHaveData(reportParams).done(function (result) {
                    if (!result) {
                        abp.message.warn(noDataMessage);
                        return;
                    }
                    _modalManager.close();
                    window.open(abp.appPath + 'app/orders/GetWorkOrderReport?' + $.param(reportParams));
                });
            }
        };
    };
})(jQuery);