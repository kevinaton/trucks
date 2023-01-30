(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    abp.helper.reports.setReportService(abp.services.app.revenueBreakdownReport);

	abp.helper.reports.setFormDataHandler(function (formData) {
        $.extend(formData, _dtHelper.getDateRangeObject(formData.DeliveryDate, 'DeliveryDateBegin', 'DeliveryDateEnd'));
        delete formData.DeliveryDateFilter;

		if (formData.Shifts && !$.isArray(formData.Shifts)) {
			formData.Shifts = [formData.Shifts];
		}

    });

    $('#DeliveryDateFilter').val(moment().format('MM/DD/YYYY - MM/DD/YYYY'));
    var drpOptions = {
        //autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        },
        showDropDown: true
    };
    $("#DeliveryDateFilter").daterangepicker(drpOptions)
        .on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });

    $("#CustomerIdFilter").select2Init({
        abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
        showAll: true
    });

    $("#OfficeIdFilter").select2Init({
        abpServiceMethod: abp.services.app.office.getOfficesSelectList,
        minimumInputLength: 0,
        minimumResultsForSearch: Infinity,
        allowClear: false
    });
    if (abp.session.officeId) {
        abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
    }

    $("#LoadAtIdFilter").select2Init({
        abpServiceMethod: abp.services.app.location.getAllLocationsSelectList
    });
    $("#DeliverToIdFilter").select2Init({
        abpServiceMethod: abp.services.app.location.getAllLocationsSelectList
    });
    $("#ServiceIdFilter").select2Init({
        abpServiceMethod: abp.services.app.service.getAllServicesSelectList,
        minimumInputLength: 0
	});

    $('#Shifts').select2Init({ allowClear: false });



})();