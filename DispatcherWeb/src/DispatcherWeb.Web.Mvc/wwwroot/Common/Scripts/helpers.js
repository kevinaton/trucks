var app = app || {};
(function () {

    app.colors = {
        danger: '#dd4b39',
        warning: '#f39c12',
        success: '#00a65a',
        unavailable: '#999999',
        freight: '#00008b',
        material: '#4169e1',
        fuel: '#6aa0ca',
    };

    abp.helperConfiguration = abp.helperConfiguration || {};
    abp.helperConfiguration.getCurrentLanguageLocale = function () {
        return abp.localization.currentLanguage.name
    };

    abp.helperConfiguration.getIanaTimezoneId = function () {
        return abp.timing.timeZoneInfo.iana.timeZoneId;
    };

    abp.helperConfiguration.getDefaultCurrencySymbol = function () {
        return abp.setting.get('App.General.CurrencySymbol');
    };

    abp.helperConfiguration.dataTables = abp.helperConfiguration.dataTables || {};
    abp.helperConfiguration.dataTables.beforeInit = abp.helperConfiguration.dataTables.beforeInit || [];
    abp.helperConfiguration.dataTables.afterInit = abp.helperConfiguration.dataTables.afterInit || [];

    if (abp.helperConfiguration.dataTables.defaultOptions) {
        abp.helperConfiguration.dataTables.defaultOptions.dom =
            `<'row bottom'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 dataTables_pager'p>>
             <'row'<'col-sm-12'tr>>
             <'row bottom'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 dataTables_pager'p>>`;

        abp.helperConfiguration.dataTables.defaultOptions.lengthMenu = app.consts.grid.defaultPageSizes;
        abp.helperConfiguration.dataTables.defaultOptions.pageLength = app.consts.grid.defaultPageSize;
    }

    var appLocalizationSource = abp.localization.getSource('DispatcherWeb');
    app.localize = function () {
        return appLocalizationSource.apply(this, arguments);
    };

    app.downloadTempFile = function (file) {
        location.href = abp.appPath + 'File/DownloadTempFile?fileType=' + encodeURIComponent(file.fileType) + '&fileToken=' + encodeURIComponent(file.fileToken) + '&fileName=' + encodeURIComponent(file.fileName);
    };
    app.downloadReportFile = function (file) {
        var url = abp.appPath + 'DownloadReportFile?fileType=' + encodeURIComponent(file.fileType) + '&fileToken=' + encodeURIComponent(file.fileToken) + '&fileName=' + encodeURIComponent(file.fileName);
        if (file.fileType === "application/pdf") {
            var win = window.open(url, '_blank');
            if (win) {
                win.focus();
            } else {
                alert('Please allow popups for this website');
            }
        } else {
            location.href = url;
        }
    };

    app.createDateRangePickerOptions = function (extraOptions) {
        extraOptions = extraOptions ||
        {
            allowFutureDate: false
        };

        var options = {
            locale: {
                format: 'L',
                applyLabel: app.localize('Apply'),
                cancelLabel: app.localize('Cancel'),
                customRangeLabel: app.localize('CustomRange')
            },
            min: moment('2015-05-01'),
            minDate: moment('2015-05-01'),
            opens: 'left',
            ranges: {}
        };

        if (!extraOptions.allowFutureDate) {
            options.max = moment();
            options.maxDate = moment();
        }

        options.ranges[app.localize('Today')] = [moment().startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('Yesterday')] = [moment().subtract(1, 'days').startOf('day'), moment().subtract(1, 'days').endOf('day')];
        options.ranges[app.localize('Last7Days')] = [moment().subtract(6, 'days').startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('Last30Days')] = [moment().subtract(29, 'days').startOf('day'), moment().endOf('day')];
        options.ranges[app.localize('ThisMonth')] = [moment().startOf('month'), moment().endOf('month')];
        options.ranges[app.localize('LastMonth')] = [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')];

        return options;
    };

    app.getUserProfilePicturePath = function (profilePictureId) {
        return profilePictureId
            ? abp.appPath + 'Profile/GetProfilePictureById?id=' + profilePictureId
            : abp.appPath + 'Common/Images/default-profile-picture.png';
    };

    app.getUserProfilePicturePath = function () {
        return abp.appPath + 'Profile/GetProfilePicture?v=' + new Date().valueOf();
    };

    app.getShownLinkedUserName = function (linkedUser) {
        if (!abp.multiTenancy.isEnabled) {
            return linkedUser.username;
        } else {
            if (linkedUser.tenancyName) {
                return linkedUser.tenancyName + '\\' + linkedUser.username;
            } else {
                return '.\\' + linkedUser.username;
            }
        }
    };

    app.notification = app.notification || {};

    app.notification.getUiIconBySeverity = function (severity) {
        switch (severity) {
            case abp.notifications.severity.SUCCESS:
                return 'fa fa-check';
            case abp.notifications.severity.WARN:
                return 'fa fa-exclamation-triangle';
            case abp.notifications.severity.ERROR:
                return 'fa fa-bolt';
            case abp.notifications.severity.FATAL:
                return 'fa fa-bomb';
            case abp.notifications.severity.INFO:
            default:
                return 'fa fa-info';
        }
    };


    app.notification.getIconFontClassBySeverity = function (severity) {
        switch (severity) {
            case abp.notifications.severity.SUCCESS:
                return ' text-success';
            case abp.notifications.severity.WARN:
                return ' text-warning';
            case abp.notifications.severity.ERROR:
                return ' text-danger';
            case abp.notifications.severity.FATAL:
                return ' text-danger';
            case abp.notifications.severity.INFO:
            default:
                return ' text-info';
        }
    };

    app.changeNotifyPosition = function (positionClass) {
        if (!toastr) {
            return;
        }

        //commented out because it was clearing priority notifications
        //toastr.clear();
        toastr.options.positionClass = positionClass;
    };

    app.waitUntilElementIsReady = function (selector, callback, checkPeriod) {
        if (!$) {
            return;
        }

        var elementCount = selector.split(',').length;

        if (!checkPeriod) {
            checkPeriod = 100;
        }

        var checkExist = setInterval(function () {
            if ($(selector).length >= elementCount) {
                clearInterval(checkExist);
                callback();
            }
        }, checkPeriod);
    };

    app.calculateTimeDifference = function (fromTime, toTime, period) {
        if (!moment) {
            return null;
        }

        var from = moment(fromTime);
        var to = moment(toTime);
        return to.diff(from, period);
    };

    app.htmlUtils = {
        htmlEncodeText: function (value) {
            return $("<div/>").text(value).html();
        },

        htmlDecodeText: function (value) {
            return $("<div/>").html(value).text();
        },

        htmlEncodeJson: function (jsonObject) {
            return JSON.parse(app.htmlUtils.htmlEncodeText(JSON.stringify(jsonObject)));
        },

        htmlDecodeJson: function (jsonObject) {
            return JSON.parse(app.htmlUtils.htmlDecodeText(JSON.stringify(jsonObject)));
        }
    };

    app.guid = function () {
        //function s4() {
        //    return Math.floor((1 + Math.random()) * 0x10000)
        //        .toString(16)
        //        .substring(1);
        //}
        //return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
        //uuidv4:
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    };

    app.showWarningIfFreeVersion = function () {
        if (abp.features.isEnabled('App.PaidFunctionalityFeature')) {
            return false;
        }

        abp.message.warn(app.localize('UpgradeToAccessThisFunctionality'));

        return true;
    };

    app.isRTL = function () {
        return (
            document.documentElement.getAttribute('dir') === 'rtl' ||
            document.documentElement.getAttribute('direction') === 'rtl'
        );
    };

})();
(function ($) {

    abp.utils.diffDays = function (date1, date2) {
        var oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
        return Math.round(Math.abs((date1.getTime() - date2.getTime()) / oneDay));
    };

    abp.enums = abp.enums || {};
    abp.enums.projectStatus = {
        pending: 0,
        active: 1,
        inactive: 2
    };
    abp.enums.workOrderStatus = {
        pending: 0,
        inProgress: 1,
        complete: 2
    };
    abp.enums.truckCategory = {
        dumpTrucks: 1,
        trailers: 2,
        tractors: 3,
        leasedDumpTrucks: 4,
        leasedTractors: 5,
        other: 6,
        triaxleDumpTruck: 7,
        quadDumpTruck: 8,
        quintDumpTruck: 9,
        waterTruck: 10,
        cementTruck: 11,
        concreteMixer: 12,
        bellyDumpTrailer: 13,
        endDumpTrailer: 14,
        walkingBedTrailer: 15,
        lowBoyTrailer: 16,
        flatBedTrailer: 17,
        stoneSlinger: 18,
        flowboy: 19,
        liveBottomTruck: 20,
        centipedeDumpTruck: 21,
        tandemDumpTruck: 22
    };
    abp.enums.truckCategoryGroup = {
        trailers: [
            abp.enums.truckCategory.trailers,
            abp.enums.truckCategory.bellyDumpTrailer,
            abp.enums.truckCategory.endDumpTrailer,
            abp.enums.truckCategory.walkingBedTrailer,
            abp.enums.truckCategory.lowBoyTrailer,
            abp.enums.truckCategory.flatBedTrailer
        ],
        dumpTrucks: [
            abp.enums.truckCategory.dumpTrucks,
            abp.enums.truckCategory.triaxleDumpTruck,
            abp.enums.truckCategory.quadDumpTruck,
            abp.enums.truckCategory.quintDumpTruck,
            abp.enums.truckCategory.waterTruck,
            abp.enums.truckCategory.cementTruck,
            abp.enums.truckCategory.concreteMixer,
            abp.enums.truckCategory.stoneSlinger,
            abp.enums.truckCategory.flowboy,
            abp.enums.truckCategory.liveBottomTruck,
            abp.enums.truckCategory.centipedeDumpTruck,
            abp.enums.truckCategory.tandemDumpTruck
        ],
        leaseHaulers: [
            abp.enums.truckCategory.leasedDumpTrucks,
            abp.enums.truckCategory.leasedTractors
        ]
    };
    abp.enums.assetType = {
        dumpTruck: 1,
        tractor: 2,
        trailer: 3,
        other: 4
    };
    abp.enums.designation = {
        freightOnly: 1,
        materialOnly: 2,
        freightAndMaterial: 3,
        //rental: 4,
        backhaulFreightOnly: 5,
        backhaulFreightAndMaterial: 9,
        disposal: 6,
        backHaulFreightAndDisposal: 7,
        straightHaulFreightAndDisposal: 8
    };
    abp.enums.designations = {
        hasMaterial: [
            abp.enums.designation.materialOnly,
            abp.enums.designation.freightAndMaterial,
            abp.enums.designation.backhaulFreightAndMaterial,
            abp.enums.designation.disposal,
            abp.enums.designation.backHaulFreightAndDisposal,
            abp.enums.designation.straightHaulFreightAndDisposal
        ],
        materialOnly: [
            abp.enums.designation.materialOnly
        ],
        freightOnly: [
            abp.enums.designation.freightOnly,
            abp.enums.designation.backhaulFreightOnly
        ],
        freightAndMaterial: [
            abp.enums.designation.freightAndMaterial,
            abp.enums.designation.backhaulFreightAndMaterial,
            abp.enums.designation.disposal,
            abp.enums.designation.backHaulFreightAndDisposal,
            abp.enums.designation.straightHaulFreightAndDisposal
        ]
    };

    abp.enums.emailDeliveryStatus = {
        notProcessed: 0,
        processed: 1,
        dropped: 2,
        deferred: 3,
        bounced: 4,
        delivered: 5,
        opened: 6
    };

    abp.enums.smsStatus = {
        unknown: 0,
        accepted: 1,
        delivered: 2,
        failed: 3,
        queued: 4,
        received: 5,
        receiving: 6,
        sending: 7,
        sent: 8,
        undelivered: 9
    };

    abp.enums.driverMessageType = {
        email: 1,
        sms: 2
    };

    abp.enums.orderPriority = {
        high: 1,
        medium: 2,
        low: 3
    };

    abp.enums.paymentProcessor = {
        none: 0,
        heartlandConnect: 1
    };

    abp.enums.taxCalculationType = {
        freightAndMaterialTotal: 1,
        materialLineItemsTotal: 2,
        materialTotal: 3,
        noCalculation: 4
    };

    abp.enums.dispatchStatus = {
        created: 0,
        sent: 1,
        acknowledged: 3,
        loaded: 4,
        completed: 5,
        error: 6,
        canceled: 7
    };

    abp.enums.shifts = {
        shift1: 0,
        shift2: 1,
        shift3: 2,
        noShift: 255
    };

    abp.enums.dispatchVia = {
        none: 0,
        //sms: 1,
        simplifiedSms: 2,
        driverApplication: 3
    };

    abp.enums.sendSmsOnDispatching = {
        dontSend: 1,
        sendWhenUserNotClockedIn: 2,
        sendForAllDispatches: 3
    };

    abp.enums.invoiceStatus = {
        draft: 0,
        sent: 1,
        viewed: 2,
        readyForQuickbooks: 3
    };

    abp.enums.quickbooksIntegrationKind = {
        none: 0,
        desktop: 1,
        online: 2,
        qboExport: 3,
        transactionProExport: 4
    };
    abp.enums.staggeredTimeKind = {
        none: 0,
        setInterval: 1
        //specificStartTimes: 2
    };
    abp.enums.driverDateConflictKind = {
        bothProductionAndHourlyPay: 1,
        productionPayTimeButNoTickets: 2
    };
    abp.enums.invoicingMethod = {
        aggregateAllTickets: 0,
        separateTicketsByJobNumber: 1,
        separateInvoicePerTicket: 2
    };
    abp.enums.gpsPlatform = {
        dtdTracker: 1,
        geotab: 2,
        samsara: 3,
        intelliShift: 4
    };
    abp.enums.predefinedLocationCategoryKind = {
        asphaltPlant: 1,
        concretePlant: 2,
        landfillOrRecycling: 3,
        miscellaneous: 4,
        yard: 5,
        quarry: 6,
        sandPit: 7,
        temporary: 8,
        projectSite: 10,
        unknownLoadSite: 11,
        unknownDeliverySite: 12
    };
    abp.enums.entityEnum = {
        dispatch: 1,
        employeeTime: 2,
        driverAssignment: 3
    };
    abp.enums.changeType = {
        removed: 0,
        modified: 1
    };
    abp.enums.payStatementItemKind = {
        time: 1,
        ticket: 2
    };
    abp.enums.showFuelSurchargeOnInvoiceEnum = {
        none: 0, //default for historical invoices
        lineItemPerTicket: 2,
        singleLineItemAtTheBottom: 3,
    };
    abp.enums.childInvoiceLineKind = {
        none: 0,
        fuelSurchargeLinePerTicket: 1,
        bottomFuelSurchargeLine: 2
    };
    abp.enums.analyzeRevenueBy = {
        driver: 0,
        truck: 1,
        customer: 2,
        date: 3
    };
    abp.enums.revenueGraphDatePeriod = {
        daily: 1,
        weekly: 2,
        monthly: 3,
        total: 4
    };
    abp.enums.orderNotifyPreferredFormat = {
        neither: 0x0,
        email: 0x1,
        sms: 0x2,
        both: 0x1 | 0x2,
    };
    abp.enums.filterActiveStatus = {
        all: 0,
        active: 1,
        inactive: 2
    };
    abp.enums.multiTenancySides = {
        tenant: 0x1,
        host: 0x2
    };

    window.app = app || {};
    app.order = app.order || {};
    app.order.getBackOfficeReportOptions = function (userOptions) {
        return $.extend({
            splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            useActualAmount: true
        }, userOptions);
    };

    app.order.getOrderWithSeparatePricesReportOptions = function (userOptions) {
        return $.extend({
            splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true
        }, userOptions);
    };

    app.order.getOrdersWithDeliveryInfoReportOptions = function (userOptions) {
        return $.extend({
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            useActualAmount: true,
            showDeliveryInfo: true
        }, userOptions);
    };

    app.order.getReceiptReportOptions = function (userOptions) {
        return $.extend({
            //splitRateColumn: true,
            showPaymentStatus: true,
            showSpectrumNumber: true,
            showOfficeName: true,
            //useActualAmount: true,
            useReceipts: true
        }, userOptions);
    };

    app.sessionStorage = app.sessionStorage || {};
    app.sessionStorage.getItem = function (key, callback) {
        app.localStorage.getItem('sessionStorageObject',
            function (sessionObject) {
                sessionObject = sessionObject || getDefaultSessionStorageObject();
                callback && callback(sessionObject[key]);
            });
    };

    app.sessionStorage.setItem = function (key, value) {
        app.localStorage.getItem('sessionStorageObject',
            function (sessionObject) {
                sessionObject = sessionObject || getDefaultSessionStorageObject();
                sessionObject[key] = value;
                app.localStorage.setItem('sessionStorageObject', sessionObject);
            });
    };

    function getDefaultSessionStorageObject() {
        return {
            //quoteBaseFuelCost: ''

        };
    }

    app.sessionStorage.clear = function () {
        app.localStorage.setItem('sessionStorageObject', getDefaultSessionStorageObject());
    };

    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (search, pos) {
            return this.substr(!pos || pos < 0 ? 0 : +pos, search.length) === search;
        };
    }

    abp.dispatches = abp.dispatches || {};

    abp.dispatches.cancel = function (acknowledged, dispatchId, doneCallback) {
        var confirmMessage = acknowledged ?
            'This dispatch is already being processed. Are you sure you want to cancel it?' :
            'Are you sure you want to cancel this dispatch?';
        swal(
            confirmMessage,
            {
                buttons: {
                    no: "No",
                    yes: "Yes"
                }
            }
        ).then(function (answer) {
            if (answer === 'yes') {
                abp.services.app.dispatching.cancelDispatch({ dispatchId: dispatchId, cancelAllDispatchesForDriver: false })
                    .done(function () {
                        abp.notify.info('Canceled successfully.');
                        if (acknowledged) {
                            abp.message.info('You should call or radio the driver to be sure they know the dispatch has been cancelled.');
                        }
                        if (doneCallback) {
                            doneCallback();
                        }
                    });

            }
        });

    };

    abp.scheduling = abp.scheduling || {};

    abp.scheduling.checkExistingDispatchesBeforeRemovingTruck = function (orderLineTruckId, truckCode, removeCallback, cancelCallback, doneCallback) {
        var removeMarkAsDoneOrCancel = function (handleMultiple) {
            switch (handleMultiple) {
                case "done":
                    doneCallback();
                    break;
                case "remove":
                    removeCallback();
                    break;
                default:
                    cancelCallback && cancelCallback();
                    return;
            }
        };

        var buttons = {
            cancel: "Cancel",
            remove: "Remove",
            done: "Mark as Done"
        };

        abp.services.app.scheduling.hasDispatches({
            orderLineTruckId: orderLineTruckId
        }).done(function (result) {
            if (result.acknowledgedOrLoaded) {
                abp.message.error(app.localize('TruckHasDispatch_YouMustCancelItFirstToRemoveTruck', truckCode));
                cancelCallback && cancelCallback();
            } else if (result.unacknowledged) {
                swal('There are open dispatches associated with this order line and truck. Removing this truck from the order will remove the dispatches this truck is assigned for this order line. Are you sure you want to do this?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            } else {
                swal('Do you want to remove this truck or mark its work as done on this order?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            }
        }).fail(function () {
            cancelCallback && cancelCallback();
        });

    };


    abp.scheduling.checkExistingDispatchesBeforeRemovingTrucks = function (orderLineId, removeCallback, cancelCallback, doneCallback) {
        var removeMarkAsDoneOrCancel = function (handleMultiple) {
            switch (handleMultiple) {
                case "done":
                    doneCallback();
                    break;
                case "remove":
                    removeCallback();
                    break;
                default:
                    cancelCallback && cancelCallback();
                    return;
            }
        };

        var buttons = {
            cancel: "Cancel",
            remove: "Remove",
            done: "Mark as Done"
        };

        abp.services.app.scheduling.orderLineHasDispatches({
            orderLineId: orderLineId
        }).done(function (result) {
            if (result.some(r => r.acknowledgedOrLoaded)) {
                var truckCode = result.find(r => r.acknowledgedOrLoaded).truckCode;
                abp.message.error(app.localize('TruckHasDispatch_YouMustCancelItFirstToRemoveTruck', truckCode));
                cancelCallback && cancelCallback();
            } else if (result.some(r => r.unacknowledged)) {
                swal('There are open dispatches associated with this order line. Removing these trucks from the order will remove the dispatches these trucks are assigned for this order line. Are you sure you want to do this?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            } else {
                swal('Do you want to remove these trucks or mark their work as done on this order?', { buttons: buttons })
                    .then(removeMarkAsDoneOrCancel);
            }
        }).fail(function () {
            cancelCallback && cancelCallback();
        });

    };


    abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero = async function (orderLineId, materialQuantity, freightQuantity, numberOfTrucks) {
        if (materialQuantity !== 0 && materialQuantity !== null || freightQuantity !== 0 && freightQuantity !== null
            || numberOfTrucks !== 0 && numberOfTrucks !== null) {
            return true;
        }
        try {
            let result = await abp.services.app.scheduling.orderLineHasDispatches({
                orderLineId: orderLineId
            });
            if (result.some((r) => r.acknowledgedOrLoaded)) {
                abp.message.warn('This order line has a dispatch in progress so the quantity and requested trucks can’t both be zero. You’ll need to cancel the dispatch before you can make this change.');
                return false;
            }
            if (result.some((r) => r.unacknowledged)) {
                if (!await abp.message.confirm(
                    'This order line has open dispatches. If you set both the quantity and requested trucks to zero, these dispatches will be removed. Are you sure you want to do this?'
                )) {
                    return false;
                }
            }
            return true;
        } catch {
            return false;
        }
    };


    abp.helper = abp.helper || {};

    abp.helper.showValidateMessage = function ($form) {
        if (!$form.is('form')) {
            return;
        }
        var validator = $form.validate();
        if (!validator.numberOfInvalids())
            return;

        var errorMessage = '';
        for (var i = 0; i < validator.errorList.length; i++) {
            var invalidElement = validator.errorList[i].element;
            var $formGroup = $(invalidElement).closest('div.form-group');
            var $label = $formGroup.find('label[for="' + invalidElement.id + '"]');
            if ($label.length === 0) {
                $label = $formGroup.find('label');
            }
            var labelText = $label.length === 0 ? '' : $label.text().trim();
            if (labelText.slice(-1) === ':') {
                labelText = labelText.slice(0, -1);
            }
            errorMessage += '"' + labelText + '"' + ' - ' + validator.errorList[i].message + '\n';
        }

        abp.helper.formatAndShowValidationMessage(errorMessage);
    };

    abp.helper.showTruckWarning = function (trucks, message, confirmCallback) {
        var s = trucks.length > 1 ? 's' : '';
        var is = trucks.length > 1 ? 'are' : 'is';
        var trucksString = trucks.join(', ');
        return abp.message.confirmWithOptions({
            text: 'Truck' + s + ' ' + trucksString + ' ' + is + ' ' + message,
            title: ' ',
            //cancelButtonText: 'No'
            buttons: ['No', 'Yes']
        },
            confirmCallback
        );
    };

    jQuery.fn.showValidateMessage = function () {
        abp.helper.showValidateMessage($(this));
    };

    abp.helper.formatAndShowValidationMessage = function (errorMessage) {
        abp.message.error('Please check the following: \n' + errorMessage, 'Some of the data is invalid');
    }

    abp.helper.validateTicketPhoto = function ($input) {
        let files = $input.get(0).files;

        if (!files.length) {
            abp.helper.formatAndShowValidationMessage('No file is selected.');
            return false;
        }

        let file = files[0];

        let type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
        if ('|jpg|jpeg|png|gif|pdf|'.indexOf(type) === -1) {
            abp.helper.formatAndShowValidationMessage('Invalid file type.');
            return false;
        }

        if (file.size > 8388608) //8 MB
        {
            abp.helper.formatAndShowValidationMessage('Size of the file exceeds allowed limits.');
            return false;
        }
        return true;
    }

    abp.helper.reports = function () {
        var _reportService;

        function setReportService(reportService) {
            _reportService = reportService;
        }
        function getReportService() {
            return _reportService;
        }

        var _formDataHandler;

        function setFormDataHandler(formDataHandler) {
            _formDataHandler = formDataHandler;
        }
        function executeFormDataHandler(formData) {
            if (_formDataHandler) {
                return _formDataHandler(formData);
            }
            return formData;
        }

        return {
            setReportService: setReportService,
            getReportService: getReportService,
            setFormDataHandler: setFormDataHandler,
            executeFormDataHandler: executeFormDataHandler
        };
    }();

    abp.helper.trimEndChar = function (str, char) {
        var re = new RegExp(char + "+$", "g");
        return str.replace(re, '');
    };

    abp.helper.validateStartEndDates = function () {
        if (arguments.length < 2) {
            return true;
        }
        var startDateIndex = 0;
        for (var i = 1; i < arguments.length; i++) {
            var startDate = arguments[startDateIndex].value;
            var endDate = arguments[i].value;
            if (startDate && endDate) {
                if (!(startDate instanceof Date)) {
                    startDate = new Date(startDate);
                }
                if (!(endDate instanceof Date)) {
                    endDate = new Date(endDate);
                }
                if (startDate > endDate) {
                    abp.message.error('The "' + abp.helper.trimEndChar(arguments[startDateIndex].title, ':') + '" cannot be after the "' + abp.helper.trimEndChar(arguments[i].title, ':') + '"');
                    return false;
                }
            } else if (startDate) {
                continue;
            }
            startDateIndex = i;
        }
        return true;
    };

    abp.helper.validateDatePickersIsNotInFuture = function ($datePickers) {
        var result = true;
        var today = new Date();
        var errorMessage = '';
        $datePickers.each(function (index, element) {
            var $ctrl = $(element);
            var selectedDate = new Date($ctrl.val());
            if (selectedDate > today) {
                result = false;
                var $label = $ctrl.closest('div.form-group').find('label');
                var labelText = abp.helper.trimEndChar($label.length === 0 ? '' : $label.text(), ':');
                errorMessage += 'The "' + labelText + '"' + ' cannot be later than today \n';
            }
        });
        if (errorMessage) {
            abp.helper.formatAndShowValidationMessage(errorMessage);
        }
        return result;
    };
    jQuery.fn.validateDatePickersIsNotInFuture = function () {
        return abp.helper.validateDatePickersIsNotInFuture($(this));
    };

    abp.helper.validateFutureDates = function () {
        if (arguments.length < 1) {
            return true;
        }
        var startDate = arguments[0].value;
        startDate = new Date(startDate);
        var today = new Date();
        if (startDate >= today) {
            abp.message.error('"' + abp.helper.trimEndChar(arguments[0].title, ':') + '" cannot be later than todays date. "');
            return false;
        }
        return true;
    };

    abp.helper.validateTodayDates = function () {
        if (arguments.length < 1) {
            return true;
        }
        var startDate = arguments[0].value;
        startDate = new Date(startDate);
        startDate.setHours(0, 0, 0, 0);
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        if (Date.parse(startDate) !== Date.parse(today)) {
            abp.message.error('"' + abp.helper.trimEndChar(arguments[0].title, ':') + '" cannot be other than todays date. "');
            return false;
        }
        return true;
    };

    abp.helper.checkGreaterNumber = function () {
        if (arguments.length < 2) {
            return true;
        }
        var startDateIndex = 0;
        for (var i = 1; i < arguments.length; i++) {
            var firstNumber = arguments[startDateIndex].value;
            var secondNumber = arguments[i].value;

            if (parseInt(firstNumber) && parseInt(secondNumber)) {
                if (parseInt(firstNumber) > parseInt(secondNumber)) {
                    abp.message.error('The "' + abp.helper.trimEndChar(arguments[startDateIndex].title, ':') + '" should not be greater than "' + abp.helper.trimEndChar(arguments[i].title, ':') + '"');
                    return false;
                }
            } else if (parseInt(firstNumber)) {
                continue;
            }
            startDateIndex = i;
        }
        return true;
    };

    abp.helper.isPositiveIntegerString = function (str, maxLength) {
        var times = '';
        if (maxLength) {
            times = '{0,' + maxLength + '}';
        } else {
            times = '*';
        }
        var r = new RegExp('^([1-9]\\d' + times + ')$');
        return r.test(str);
    };

    abp.helper.getLocation = function (callback) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(callback, onError);
        } else {
            callback(null);
        }

        function onError(error) {
            abp.notify.error('Location error: ' + error.message);
            callback(null);
        }
    };

    abp.helper.getMMddyyyyDate = function (date) {
        var year = date.getFullYear();

        var month = (1 + date.getMonth()).toString();
        month = month.length > 1 ? month : '0' + month;

        var day = date.getDate().toString();
        day = day.length > 1 ? day : '0' + day;

        return month + '/' + day + '/' + year;
    };

    abp.helper.createModal = function (modalName, folderName, modalSize) {
        return new app.ModalManager({
            viewUrl: abp.appPath + 'app/' + folderName + '/' + modalName + 'Modal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/' + folderName + '/_' + modalName + 'Modal.js',
            modalClass: modalName + 'Modal',
            modalId: modalName + 'Modal',
            modalSize: modalSize || 'md'
        });
    };

    abp.helper.calculateOrderLineTotal = function (materialAmount, freightAmount, isTaxable, salesTaxRate) {
        var taxCalculationType = abp.setting.getInt('App.Invoice.TaxCalculationType');

        materialAmount = abp.utils.round(materialAmount);
        freightAmount = abp.utils.round(freightAmount);
        var subtotal = materialAmount + freightAmount;
        var taxableTotal = 0;
        var taxRate = salesTaxRate / 100;
        var salesTax = 0;
        var orderLineTotal = 0;

        switch (taxCalculationType) {
            case abp.enums.taxCalculationType.freightAndMaterialTotal:
                taxableTotal = materialAmount + freightAmount;
                break;

            case abp.enums.taxCalculationType.materialLineItemsTotal:
                taxableTotal = materialAmount > 0 ? materialAmount + freightAmount : 0;
                break;

            case abp.enums.taxCalculationType.materialTotal:
                taxableTotal = materialAmount;
                break;

            case abp.enums.taxCalculationType.noCalculation:
                taxRate = 0;
                salesTax = 0;
                break;
        }

        if (!isTaxable || taxableTotal < 0) {
            taxableTotal = 0;
        }

        switch (taxCalculationType) {
            case abp.enums.taxCalculationType.freightAndMaterialTotal:
            case abp.enums.taxCalculationType.materialLineItemsTotal:
            case abp.enums.taxCalculationType.materialTotal:
                salesTax = abp.utils.round(taxableTotal * taxRate);
                orderLineTotal = abp.utils.round(subtotal + taxableTotal * taxRate, 2);
                break;

            case abp.enums.taxCalculationType.noCalculation:
                //salesTax = abp.utils.round(salesTax);
                //orderLineTotal = abp.utils.round(subtotal + salesTax);
                orderLineTotal = subtotal;
                break;
        }

        //var totalsToCheck = new[] { order.FreightTotal, order.MaterialTotal, order.SalesTax, order.CODTotal };
        //var maxValue = AppConsts.MaxDecimalDatabaseLength;
        //if (totalsToCheck.Any(x => x > maxValue))
        //{
        //    throw new UserFriendlyException("The value is too big", "One or more totals exceeded the maximum allowed value. Please decrease some of the values so that the total doesn't exceed " + maxValue);
        //}

        return {
            subtotal: subtotal,
            total: orderLineTotal,
            tax: salesTax
        };
    };

    abp.helper.getShiftName = function (shift) {
        switch (shift) {
            case abp.enums.shifts.shift1:
                return abp.setting.get('App.General.ShiftName1');
            case abp.enums.shifts.shift2:
                return abp.setting.get('App.General.ShiftName2');
            case abp.enums.shifts.shift3:
                return abp.setting.get('App.General.ShiftName3');
            case abp.enums.shifts.noShift:
                return "[No Shift]";
        }
        return '';
    };

    abp.helper.getDefaultStartTime = function (date) {
        date = date ? moment(date).clone() : moment();
        var time = moment(abp.setting.get('App.DispatchingAndMessaging.DefaultStartTime'));
        return date.set({
            hour: time.hour(),
            minute: time.minute(),
            second: time.second()
        });
    };

    abp.helper.promptForHideLoadAtOnQuote = function () {
        return new Promise((resolve) => {
            if (!abp.setting.getBoolean("App.General.PromptForDisplayingQuarryInfoOnQuotes")) {
                resolve(false);
                return;
            }
            abp.message.confirmWithOptions({
                text: app.localize('DoYouWantToHideLoadAtColumn'),
                title: ' ',
                buttons: ['No', 'Yes']
            },
                function (isConfirmed) {
                    resolve(isConfirmed || false);
                }
            );
        });
    };

})(jQuery);
(function ($) {



    var dataMerge = function () {
        var mergeButtonClassName = "data-merge-button";
        var rowSelectionClassName = "data-merge-row-selection";
        var mergeButtonClass = '.' + mergeButtonClassName;
        var rowSelectionClass = '.' + rowSelectionClassName;
        var mergeRecordCountLimit = 5;

        function addColumn(options) {
            options.columns.push({
                data: null,
                orderable: false,
                render: function (data, type, full, meta) {
                    if (full.disallowDataMerge === true) {
                        return "";
                    }
                    return '<label class="m-checkbox"><input type="checkbox" class="minimal ' + rowSelectionClassName + '"><span></span></label>';
                },
                className: "checkmark data-merge-header-cell text-center",
                width: "50px",
                title: " ",
                responsivePriority: 3,
                responsiveDispalyInHeaderOnly: true
            });
            var oldHeaderCallback = options.headerCallback;
            options.headerCallback = function (thead, data, start, end, display) {
                var headerCell = $(thead).find('th').eq(options.columns.length - 1).html('');
                headerCell.append($('<button type="button" class="btn btn-default btn-sm" disabled>Merge</button>').addClass(mergeButtonClassName));
                if (oldHeaderCallback)
                    oldHeaderCallback(thead, data, start, end, display);
            };
        }

        function handleColumn(table, grid, options) {

            table.on('change', rowSelectionClass, function () {
                if ($(this).is(":checked")) {
                    if (table.find(rowSelectionClass + ':checked').length > mergeRecordCountLimit) {
                        $(this).prop('checked', false).change();
                        abp.notify.warn(`You can only merge ${mergeRecordCountLimit} ${(options.dataMergeOptions.entitiesName || 'records')} at a time. Please change your selection.`);
                    }
                }
                table.find(mergeButtonClass).prop('disabled', table.find(rowSelectionClass + ':checked').length < 2);
            });

            var mergeModal = app.modals.MergeModal.create({
                dropdownServiceMethod: options.dataMergeOptions.dropdownServiceMethod,
                mergeServiceMethod: options.dataMergeOptions.mergeServiceMethod
            });

            table.on('click', mergeButtonClass, function (e) {
                e.preventDefault();
                var ids = [];
                table.find(rowSelectionClass + ':checked').each(function () {
                    ids.push(abp.helper.dataTables.getRowData(this).id);
                });
                mergeModal.open({
                    idsToMerge: ids.join(),
                    description: options.dataMergeOptions.description
                });
            });

            abp.event.on('app.mergeModalFinished', function () {
                grid.ajax.reload();
            });
        }

        return {
            addColumn: addColumn,
            handleColumn: handleColumn
        };
    }();
    abp.helperConfiguration.dataTables.beforeInit.push((options) => {
        if (options.dataMergeOptions && options.dataMergeOptions.enabled && app.modals.MergeModal) {
            dataMerge.addColumn(options);
        }
    });
    abp.helperConfiguration.dataTables.afterInit.push((table, grid, options) => {
        if (options.dataMergeOptions && options.dataMergeOptions.enabled && app.modals.MergeModal) {
            dataMerge.handleColumn(table, grid, options);
        }
    });

    var massDelete = function () {
        function handleColumn(table, grid, options) {
            options.selectionColumnOptions.selectionChangedCallbacks.push(function (selectedRowsCount) {
                $(options.massDeleteOptions.deleteButton).prop('disabled', selectedRowsCount < 1);
            });

            $(options.massDeleteOptions.deleteButton).click(async function (e) {
                e.preventDefault();
                var ids = options.selectionColumnOptions.getSelectedRowsIds();
                if (ids.length === 0) {
                    return;
                }
                if (await abp.message.confirm(
                    'Are you sure you want to delete all selected items?'
                )) {
                    options.massDeleteOptions.deleteServiceMethod({
                        ids: ids
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        grid.ajax.reload();
                    });
                }
            });
        }

        return {
            handleColumn: handleColumn
        };
    }();
    abp.helperConfiguration.dataTables.beforeInit.push((options) => {
        if (options.massDeleteOptions && options.massDeleteOptions.enabled) {
            options.selectionColumnOptions = options.selectionColumnOptions || {};
        }
    });
    abp.helperConfiguration.dataTables.afterInit.push((table, grid, options) => {
        if (options.massDeleteOptions && options.massDeleteOptions.enabled) {
            massDelete.handleColumn(table, grid, options);
        }
    });

    abp.helperConfiguration.dataTables.beforeInit.push((options, table) => {
        var lastRequestedPageSize;
        table.on("draw.dt", () => {
            var pageSizeSelector = findPageSizeDropdownForTable(table);

            if (lastRequestedPageSize && parseInt(pageSizeSelector.val()) !== lastRequestedPageSize) {
                pageSizeSelector.val(lastRequestedPageSize).trigger('change.select2');
            }

            if (!pageSizeSelector.data('select2')) {
                pageSizeSelector.select2Init({
                    showAll: true,
                    allowClear: false
                });
            } else {
                pageSizeSelector.trigger('change.select2');
            }
        });

        table.on('preXhr.dt', (e, settings, data) => {
            if (data && data.length > 0) {
                lastRequestedPageSize = data.length;
            }
        });
    });

    abp.helperConfiguration.dataTables.beforeInit.push((options, table) => {
        table.on('preXhr.dt', () => abp.ui.setBusy(table.closest('.dataTables_wrapper')));
        table.on('xhr.dt', () => abp.ui.clearBusy(table.closest('.dataTables_wrapper')));
    });

    function findPageSizeDropdownForTable(table) {
        return $(table).closest('.dataTables_wrapper').find('div.dataTables_length select');
    }

})(jQuery);
(function ($) {

    abp.helper = abp.helper || {};
    abp.helper.graphs = abp.helper.graphs || {};

    abp.helper.graphs.initBarChart = async function initBarChart(options) {
        options.container = $(options.container);
        options.loadingContainer = $(options.loadingContainer);
        options.barOptions = options.barOptions || {};
        if (!options.getDataAsync) {
            return;
        }

        if (!options.container.length) {
            return;
        }

        options.container.hide();
        options.loadingContainer.show();

        //added maxXLabelLength option to truncate the extra long labels, but keep showing the full label in the hover text
        if (options.barOptions.maxXLabelLength && options.barOptions.xkey) {
            let originalXLabelFormat = options.barOptions.xLabelFormat;
            let originalHoverCallback = options.barOptions.hoverCallback;

            options.barOptions.xLabelFormat = function (x) {
                let text = originalXLabelFormat ? originalXLabelFormat(x) : x.label;
                return abp.utils.truncate(text, options.barOptions.maxXLabelLength, true);
            };

            options.barOptions.hoverCallback = function (index, barOptions, content, row) {
                let finalContent = $(content);
                finalContent.eq(0).text(row[options.barOptions.xkey]);

                if (originalHoverCallback) {
                    finalContent = originalHoverCallback(index, barOptions, finalContent, row);
                }

                return finalContent;
            };
        }

        var updateGraphHeight = function (data) {
            let fallbackHeight = options.height || 342;
            let height = fallbackHeight;
            if (options.barOptions.horizontal) {
                const rowHeight = options.rowHeight || 30; //px
                const footerHeight = 56;
                const topPadding = 26;
                height = data.length ? data.length * rowHeight + footerHeight + topPadding : fallbackHeight;
            }
            options.container.css('height', height + 'px');
            return height;
        }

        var BarChart = function (element) {
            var init = function (data) {
                return new Morris.Bar({
                    element: element,
                    fillOpacity: 1,
                    data: data,
                    lineColors: ['#399a8c'],
                    hideHover: 'auto',
                    resize: true,
                    ...options.barOptions
                });
            };

            var refresh = async function () {
                let result = await options.getDataAsync();
                this.draw(result);
            };

            var draw = function (data) {
                let newHeight = updateGraphHeight(data);
                if (data.length) {
                    if (!this.graph) {
                        this.graph = init(data);
                    } else {
                        this.graph.setData(data);
                        this.graph.redraw();
                        this.graph.el.find('svg').attr('height', newHeight);
                    }
                } else {
                    console.warn("An empty array was passed to Morris Bar graph");
                }
            };

            return {
                draw: draw,
                refresh: refresh
            };
        };
        var data = await options.getDataAsync();

        options.container.empty();
        options.container.show();

        var chart = new BarChart(options.container);
        chart.draw(data);

        options.loadingContainer.hide();

        return chart;
    };

    abp.helper.graphs.initDonutChart = async function initDonutChart(options) {
        options.container = $(options.container);
        options.highlightedDataIndex = options.highlightedDataIndex || 0;
        options.donutOptionsGetter = options.donutOptionsGetter || (() => { return {} });
        options.hasData = options.hasData || ((result) => true);

        if (!options.container.length || !options.getDataAsync) {
            return;
        }

        var DonutChart = function (options) {
            let donutElement = options.container.find('.dashboard-donut-chart');
            let legendContainer = options.container.find('.donut-legend');

            var init = function (data) {
                let donutOptions = {
                    element: donutElement,
                    resize: true,
                    ...options.donutOptionsGetter(data)
                };
                return new Morris.Donut(donutOptions);
            }

            var draw = function (data) {
                donutElement.show();
                if (!this.graph) {
                    this.graph = init(data);
                    this.graph.select(options.highlightedDataIndex);

                    // Add Legends
                    this.graph.data.forEach(function (item, i) {
                        var legendItem = $('<span></span>').text(item['label']).prepend('<i>&nbsp;</i>');
                        legendItem.find('i').css('backgroundColor', item.color);
                        legendContainer.append(legendItem);
                    });

                } else {
                    this.graph.setData(data);
                    this.graph.redraw();
                    this.graph.select(options.highlightedDataIndex);
                }
            };

            return {
                draw: draw
            };
        };

        try {
            updateDonutChartContainerVisibility(options.container);
            let data = await options.getDataAsync();
            let hasData = options.hasData(data);
            if (options.gotDataCallback) {
                options.gotDataCallback(data, hasData);
            }
            updateDonutChartContainerVisibility(options.container, hasData);
            if (!hasData) {
                return;
            }
            let chart = new DonutChart(options);
            chart.draw(data);
        }
        catch (e) {
            options.container.hide();
            throw e;
        }
    }

    function updateDonutChartContainerVisibility(container, hasData) {
        let loading = hasData === undefined;
        container.find('.data-loading').toggle(loading);
        //container.find('.donut-chart-header-text').toggle(!loading);
        container.find('.no-data').toggle(!loading && !hasData);
        container.find('.has-data').toggle(!loading && !!hasData);
    }

})(jQuery);
(function ($) {

    $(document).on('select2:open', () => {
        document.querySelector('.select2-container--open .select2-search__field').focus();
    });

    jQuery.fn.select2Uom = function (additionalOptions) {
        additionalOptions = additionalOptions || {};
        var options = {
            abpServiceMethod: abp.services.app.unitOfMeasure.getUnitsOfMeasureSelectList,
            showAll: true,
            allowClear: false
        };
        options = $.extend(true, {}, options, additionalOptions);
        return $(this).select2Init(options);
    };

    jQuery.fn.select2Location = function (userOptions) {
        userOptions = userOptions || {};

        if (!userOptions.predefinedLocationCategoryKind) {
            console.error('userOptions.predefinedLocationCategoryKind is not set in a call to select2Location');
            return;
        }

        var $element = $(this);
        const select2PageSize = 20;

        var googlePlacesContext = {
            sessionToken: null //new google.maps.places.AutocompleteSessionToken()
        };
        $element.data('google-places-context', googlePlacesContext);
        let mapsDiv = $('<div>').appendTo($element.parent());
        var initGooglePlaces = function () {
            return abp.maps.waitForGoogleMaps().then(function () {
                if (googlePlacesContext.autocompleteService) {
                    return;
                }
                googlePlacesContext.autocompleteService = new google.maps.places.AutocompleteService();
                googlePlacesContext.placesService = new google.maps.places.PlacesService(mapsDiv[0]);
            });
        };

        var defaultOptions = {
            abpServiceMethod: abp.services.app.location.getLocationsSelectList,
            minimumInputLength: 1,
            allowClear: true,
            ajax: {
                delay: 500,
                transport: function (params, success, failure) {
                    var additionalParams = {};
                    if (userOptions.abpServiceParamsGetter) {
                        additionalParams = userOptions.abpServiceParamsGetter(params);
                    }
                    return options.abpServiceMethod($.extend({}, params.data, userOptions.abpServiceParams, additionalParams))
                        .done(function (dbResponse) {
                            console.log({ dbResponse, params, page: params.page });
                            if (dbResponse && dbResponse.totalCount === 0 && params.data.page === 1) {
                                console.log('using google places');
                                return initGooglePlaces().then(function () {
                                    if (!googlePlacesContext.sessionToken) {
                                        googlePlacesContext.sessionToken = new google.maps.places.AutocompleteSessionToken();
                                    }
                                    googlePlacesContext.autocompleteService.getPlacePredictions({
                                        input: params.data.term,
                                        sessionToken: googlePlacesContext.sessionToken
                                    }, function (predictions, status) {
                                        if (status != google.maps.places.PlacesServiceStatus.OK || !predictions) {
                                            predictions = [];
                                        }
                                        console.log(predictions);
                                        success({
                                            results: predictions.map(p => ({
                                                id: _googlePlacesHelper.googlePlaceIdPrefix + p.place_id,
                                                name: p.description
                                            })),
                                            pagination: {
                                                more: false //todo
                                            }
                                        });
                                    });
                                });
                            } else {
                                success({
                                    results: dbResponse.items,
                                    pagination: {
                                        more: params.data.page * select2PageSize < dbResponse.totalCount
                                    }
                                });
                            }
                        })
                        .fail(failure);
                },
                cache: false
            }
        };

        var options = $.extend(true, {}, defaultOptions, userOptions);

        $element.on("select2:close", function () {
            //'close' is called after the 'change', so we can clear the session token
            googlePlacesContext.sessionToken = null;
        });
        $element.change(function () {
            var val = $element.val();
            var select2 = $element.data('select2');
            if (val && val.startsWith(_googlePlacesHelper.googlePlaceIdPrefix)) {
                var onFail = function () {
                    $element.val(null).change();
                    abp.ui.clearBusy(select2?.$container);
                };
                var placeId = val.substring(_googlePlacesHelper.googlePlaceIdPrefix.length);
                if (!placeId) {
                    console.error('select2 had value ' + val + ' that can\'t be replaced');
                    onFail();
                    return;
                }
                var sessionToken = googlePlacesContext.sessionToken;
                if (!sessionToken) {
                    console.error('select2 had value ' + val + ', but session token is missing');
                    onFail();
                    return;
                }

                abp.ui.setBusy(select2?.$container);
                googlePlacesContext.placesService.getDetails({
                    placeId,
                    sessionToken,
                    fields: ["address_components", "geometry", "place_id", "name"]
                }, function (place, status) {
                    console.log(place);
                    if (status != google.maps.places.PlacesServiceStatus.OK || !place) {
                        console.error('unexpected response code: ' + status, place);
                        onFail();
                        return;
                    }
                    googlePlacesContext.sessionToken = null;
                    let locationName = place.name;
                    let locationAddress = _googlePlacesHelper.parseAddressComponents(place.address_components);
                    if (place.name === locationAddress.streetAddress) {
                        console.log('location name matched street address, setting name to ""');
                        locationName = '';
                    }

                    let newLocation = $.extend({
                        PredefinedLocationCategoryKind: userOptions.predefinedLocationCategoryKind,
                        Name: locationName,
                        //...locationAddress, our JS bunlder might not support destructuring assignment
                        PlaceId: place.place_id,
                        Latitude: place.geometry && place.geometry.location && place.geometry.location.lat || null,
                        Longitude: place.geometry && place.geometry.location && place.geometry.location.lng || null,
                        IsActive: true
                    }, locationAddress);

                    abp.services.app.location.createOrGetExistingLocation(
                        newLocation
                    ).then(createdLocation => {
                        abp.helper.ui.addAndSetDropdownValue($element, createdLocation.id, createdLocation.displayName);
                        abp.ui.clearBusy(select2?.$container);
                    }).fail(function () {
                        onFail();
                    });
                });
            }
        });

        return $element.select2Init(options);
    };

    var _googlePlacesHelper = {
        googlePlaceIdPrefix: 'google.maps.places.id.',
        parseAddressComponents: function (address_components) {

            let result = {
                streetAddress: "",
                city: "",
                state: "",
                zipCode: "",
                countryCode: ""
            };

            if (!address_components) {
                return result;
            }

            // address_components are google.maps.GeocoderAddressComponent objects
            // which are documented at http://goo.gle/3l5i5Mr
            for (var component of address_components) {
                const componentType = component.types[0];

                switch (componentType) {
                    case "street_number": {
                        result.streetAddress = component.long_name + ' ' + result.streetAddress;
                        break;
                    }

                    case "route": {
                        result.streetAddress += component.short_name;
                        break;
                    }

                    case "postal_code": {
                        result.zipCode = component.long_name + result.zipCode;
                        break;
                    }

                    case "postal_code_suffix": {
                        result.zipCode = result.zipCode + '-' + component.long_name;
                        break;
                    }
                    case "locality":
                        result.city = component.long_name;
                        break;

                    case "administrative_area_level_1": {
                        result.state = component.short_name;
                        break;
                    }
                    case "country":
                        result.countryCode = component.short_name;
                        break;
                }
            }

            return result;
        }
    };
    abp.helper.googlePlacesHelper = _googlePlacesHelper;

    abp.helper = abp.helper || {};
    abp.helper.ui = abp.helper.ui || {};

    abp.helper.ui.initControls = function initControls() {
        //var checkboxes = $('input[type="checkbox"].minimal, input[type="radio"].minimal');
        //checkboxes.each(function () {
        //    if ($(this).data('hasICheckboxAttached') === true) {
        //        return;
        //    }
        //    $(this).data('hasICheckboxAttached', true);
        //    $(this).on('ifChanged', function (event) { $(event.target).trigger('change'); });
        //    $(this).on('change', function () { $(this).iCheck('update'); });
        //});
        //checkboxes.iCheck({
        //    checkboxClass: 'icheckbox_minimal-blue',
        //    radioClass: 'iradio_minimal-blue'
        //});
    };

    abp.helper.ui.initChildDropdown = function initChildDropdown(options) {
        var parentDropdown = options.parentDropdown;
        var childDropdown = options.childDropdown;
        var abpServiceMethod = options.abpServiceMethod;
        var abpServiceData = options.abpServiceData || {};
        var optionCreatedCallback = options.optionCreatedCallback;
        var onChildDropdownUpdatedCallbacks = [];
        var updateChildDropdown = function (callback, updateOptions) {
            updateOptions = updateOptions || {};
            var parentValue = parentDropdown.val();
            abp.ui.setBusy(childDropdown);
            abpServiceMethod($.extend({}, abpServiceData, { id: parentValue }))
                .done(function (data) {
                    var oldChildValue = childDropdown.val();
                    var oldChildOption = childDropdown.find('option[value="' + oldChildValue + '"]').clone();
                    var childPlaceholder = childDropdown.find('option[value=""]').clone();
                    var setPlaceholderText = function (placeholderText) {
                        childPlaceholder.text(placeholderText);
                        if (childDropdown.data("select2")) {
                            childDropdown.data("placeholder", placeholderText);
                            childDropdown.data("select2").selection.placeholder.text = placeholderText;
                            childDropdown.trigger('change.select2');
                        }
                    };
                    childDropdown.empty();
                    childDropdown.append(childPlaceholder);
                    if (data.items.length) {
                        $.each(data.items, function (ind, val) {
                            var option = $('<option></option>').text(val.name).attr('value', val.id);

                            if (val.item) {
                                for (var itemProperty in val.item) {
                                    if (val.item.hasOwnProperty(itemProperty)) {
                                        option.data(itemProperty, val.item[itemProperty]);
                                    }
                                }
                            }
                            if (optionCreatedCallback)
                                optionCreatedCallback(option, val);
                            option.appendTo(childDropdown);
                        });
                        if (oldChildValue !== '') {
                            if (childDropdown.find('option[value="' + oldChildValue + '"]').length === 0) {
                                childDropdown.append(oldChildOption);
                            }
                            childDropdown.val(oldChildValue); //.change();
                        }
                        setPlaceholderText(childPlaceholder.data('placeholder-default') || 'Select an option');
                    } else {
                        if (oldChildValue !== '') {
                            childDropdown.append(oldChildOption);
                        }
                        setPlaceholderText(childPlaceholder.data(parentValue ? 'placeholder-no-items' : 'placeholder-no-parent') || 'No options available');
                    }
                    if (callback) {
                        callback(data);
                    }
                    if (onChildDropdownUpdatedCallbacks.length && !updateOptions.skipCallbacks) {
                        $.each(onChildDropdownUpdatedCallbacks, function (ind, c) {
                            c && c(data);
                        });
                    }
                })
                .always(function () {
                    abp.ui.clearBusy(childDropdown);
                });
        };
        updateChildDropdown(null, { skipCallbacks: true });

        parentDropdown.change(function () {
            if (childDropdown.val() !== '') {
                childDropdown.val('').change();
            }
            updateChildDropdown();
        });

        var onChildDropdownUpdated = function (callbackToAdd) {
            onChildDropdownUpdatedCallbacks.push(callbackToAdd);
        };

        return {
            parentDropdown: parentDropdown,
            childDropdown: childDropdown,
            updateChildDropdown: updateChildDropdown,
            onChildDropdownUpdated: onChildDropdownUpdated
        };
    };

    abp.helper.ui.syncDropdownValueIfOtherIsNull = function syncDropdownValueIfOtherIsNull(source, target) {
        if (target.val() || !source.val()) {
            return;
        }
        var option = abp.helper.ui.getDropdownValueAndLabel(source);
        abp.helper.ui.addAndSetDropdownValue(target, option.value, option.label);
    };

    abp.helper.ui.syncUomDropdowns = function syncUomDropdowns(materialUomDropdown, freightUomDropdown, designationDropdown, materialQuantityInput, freightQuantityInput) {
        var designationIsFreightAndMaterial = function () {
            return designationDropdown ? abp.enums.designations.freightAndMaterial.includes(Number(designationDropdown.val())) : true;
        };
        var uomsAreSame = function () {
            return materialUomDropdown.val() === freightUomDropdown.val() && materialUomDropdown.val();
        };
        materialUomDropdown.change(function () {
            if (designationIsFreightAndMaterial()) {
                abp.helper.ui.syncDropdownValueIfOtherIsNull(materialUomDropdown, freightUomDropdown);
            }
        });
        freightUomDropdown.change(function () {
            if (designationIsFreightAndMaterial()) {
                abp.helper.ui.syncDropdownValueIfOtherIsNull(freightUomDropdown, materialUomDropdown);
            }
        });
        if (materialQuantityInput && freightQuantityInput) {
            materialQuantityInput.change(function () {
                if (designationIsFreightAndMaterial() && uomsAreSame()) {
                    freightQuantityInput.val(materialQuantityInput.val());
                }
            });
        }
    };

    abp.helper.ui.getEmailDeliveryStatusIcon = function getEmailDeliveryStatusIcon(status) {
        switch (status) {
            default:
                return null; //$('<span>');
            case abp.enums.emailDeliveryStatus.notProcessed:
            case abp.enums.emailDeliveryStatus.processed:
            case abp.enums.emailDeliveryStatus.deferred:
                return $('<span class="fa-stack" title="Sending">' +
                    '<i class="fa fa-envelope fa-stack-1x fa-lg"></i>' +
                    '<i class="fa fa-clock fa-stack-1x" style="margin-left: 10.7px; margin-top: 11.4px; color:#e69c1c;"></i>' +
                    '</span>');
            case abp.enums.emailDeliveryStatus.dropped:
            case abp.enums.emailDeliveryStatus.bounced:
                return $('<span class="fa-stack" title="Delivery failed">' +
                    '<i class="fa fa-envelope fa-stack-1x fa-lg"></i>' +
                    '<i class="fa fa-times fa-stack-1x text-danger" style="margin-left: 10.3px; margin-top: 6.6px;"></i>' +
                    '</span>');
            case abp.enums.emailDeliveryStatus.delivered:
                return $('<i class="fa fa-envelope fa-lg icon-center" title="Delivered"></i>');
            //    '<span class="fa-stack">' +
            //      '<i class="fa fa-envelope-open fa-stack-1x fa-lg"></i>' +
            //      '<i class="fa fa-check fa-stack-1x text-success" style="margin-left: 14.7px; margin-top: -10.6px;"></i>' +
            //    '</span>'
            case abp.enums.emailDeliveryStatus.opened:
                return $('<i class="fa fa-envelope-open fa-lg icon-center" title="Opened"></i>');
        }
    };

    abp.helper.ui.initCannedTextLists = function initCannedTextLists() {
        $('.insert-canned-text-list').each(function () {
            var list = $(this);
            list.find('li[data-id="loading"]').show();
            abp.services.app.cannedText.getCannedTextsSelectList({}).done(function (data) {
                if (data.items.length > 0) {
                    list.find('li[data-id="no-items"]').hide();
                    $.each(data.items, function (ind, item) {
                        var li = $('<li></li>').attr('data-id', item.id).appendTo(list);
                        li.append($('<a href="#"></a>').text(item.name));
                    });
                } else {
                    list.find('li[data-id="no-items"]').show();
                }
                list.find('li[data-id="loading"]').hide();
            });
            var target = $('#' + list.attr('data-target-id'));
            list.on('click', 'li', function (e) {
                e.preventDefault();
                var cannedTextId = $(this).attr('data-id');
                if (cannedTextId === "no-items" || cannedTextId === "loading" || cannedTextId === undefined || cannedTextId === '') {
                    return;
                }
                //abp.ui.setBusy(target);
                abp.services.app.cannedText.getCannedTextForEdit({ id: cannedTextId }).done(function (data) {
                    target.replaceSelectedText(data.text);
                }).always(function () {
                    //abp.ui.clearBusy(target);
                });
            });
        });
    };
    abp.helper.ui.initCannedTextLists();


    jQuery.fn.replaceSelectedText = function (newText) {
        var start = $(this).prop('selectionStart');
        var end = $(this).prop('selectionEnd');
        var value = $(this).val();
        var newValue = value.substring(0, start) + newText + value.substring(end);
        $(this).val(newValue);
        $(this).prop('selectionStart', start + newText.length);
        $(this).prop('selectionEnd', start + newText.length);
        $(this).focus();
    };



    abp.message = abp.message || {};
    abp.message.confirmWithOptions = function (userOpts, callback) {
        //use userOpts.text and userOpts.title
        var opts = $.extend(
            {},
            abp.libs.sweetAlert.config.default,
            abp.libs.sweetAlert.config.confirm,
            userOpts
        );

        return $.Deferred(function ($dfd) {
            //pop up
            swal(opts)
                .then((isConfirmed) => {
                    callback && callback(isConfirmed);
                    $dfd.resolve(isConfirmed);
                });
        });


        ////use userOpts.text and userOpts.title
        //var opts = $.extend(
        //	{},
        //	abp.libs.sweetAlert.config.default,
        //	abp.libs.sweetAlert.config.confirm,
        //	userOpts
        //      );
        //      console.log(opts)

        //      return $.Deferred(function ($dfd) {
        //	sweetAlert(opts, function (isConfirmed) {
        //		callback && callback(isConfirmed);
        //		$dfd.resolve(isConfirmed);
        //	});
        //});
    };

    function storeNavbarCollapsedState(isCollapsed) {
        app.localStorage.setItem('isNavbarCollapsed', isCollapsed);
    }

    $("body").on('expanded.pushMenu', function () {
        storeNavbarCollapsedState(false);
    });
    $("body").on('collapsed.pushMenu', function () {
        storeNavbarCollapsedState(true);
    });

    app.regex = app.regex || {};
    //app.regex.email = '^[A-Z0-9._%+-]+@([A-Z0-9-]+\\.)+[A-Z]{2,63}$';
    //app.regex.emails = '^([A-Z0-9._%+-]+@([A-Z0-9-]+\\.)+([A-Z]{2,63})[;, ]*)+$';
    app.regex.email = '^(?:[a-z0-9!#$%&\'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&\'*+/=?^_`{|}~-]+)*|"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])$';
    //escaped ^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$
    app.regex.emails = '^(?:(?:[a-z0-9!#$%&\'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&\'*+/=?^_`{|}~-]+)*|"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])[;, ]*)+$';
    //escaped ^(?:(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])[;, ]*)+$
    app.regex.latitude = "^[-+]?([1-8]?\\d(\\.\\d+)?|90(\\.0+)?)$";
    app.regex.longitude = "^\\s*[-+]?(180(\\.0+)?|((1[0-7]\\d)|([1-9]?\\d))(\\.\\d+)?)$";
    app.regex.cellPhoneNumber = '^\\+?[1-9]\\d{1,14}$';
    app.regex.url = '^https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.,~#?!&\\/\\/=]*)$';
    app.regex.mileage = '^\\d{0,17}(\\.\\d{0,1})?$';


    abp.signalr = abp.signalr || {};
    abp.signalr.hubs = abp.signalr.hubs || {};
    abp.signalr.hubs.dispatcher = null;
    abp.helper.signalr = abp.helper.signalr || {};
    abp.helper.signalr.startDispatcherConnection = function () {
        abp.signalr.startConnection(abp.appPath + 'signalr-dispatcher', function (connection) {
            abp.signalr.hubs.dispatcher = connection;
            connection.on('syncRequest', function (syncRequest) {
                //abp.log.debug('received syncRequest:', syncRequest);
                abp.event.trigger('abp.signalR.receivedSyncRequest', syncRequest);
            });
        }).then(function (connection) {
            abp.log.debug('Connected to dispatcherHub server!');
            abp.event.trigger('dispatcherHub.connected');
        });
    };


    abp.maps = abp.maps || {};
    abp.maps.waitForGoogleMaps = function () {
        return new Promise((resolve, reject) => {
            var isAvailable = () => typeof (window.googleMapsAreAvailable) !== 'undefined';
            if (isAvailable()) {
                resolve();
                return;
            }
            if (typeof (window.googleMapsApiKeyIsMissing) !== 'undefined') {
                let message = app.localize('GoogleMapsApiKeyIsMissing');
                console.error(message);
                abp.message.error(message);
                reject(message);
                return;
            }
            var resolveWhenAvailable = function () {
                setTimeout(function () {
                    if (isAvailable()) {
                        resolve();
                    } else {
                        resolveWhenAvailable();
                    }
                }, 300);
            };
            resolveWhenAvailable();
        });
    };


    $(function () {
        $.validator.addMethod(
            "mileage",
            function (value, element) {
                var re = new RegExp(app.regex.mileage, 'i');
                return this.optional(element) || re.test(value);
            },
            "The value must be a number with a maximum one decimal place."
        );
    });

    $(function () {
        //custom icon stacking
        $('.fa.fa-truck.fa-truck-usd').replaceWith($('<span class="fa-stack" style="font-size: 7px; margin-right: 6px; margin-top: -4px;">'
            + '<i class="fa fa-truck fa-stack-2x"></i>'
            + '<i class="fa fa-usd fa-stack-1x" style=" margin-left:2.5px; color: #2c3b41"></i>'
            + '</span>'));

        $('.fa.fa-truck.fa-truck-navicon').replaceWith('<span class="fa-stack" style="font-size: 7px; margin-right: 6px; margin-top: -4px;"><i class="fa fa-truck fa-stack-2x"></i><i class="fa fa-navicon fa-stack-1x" style="margin-left: 2.5px; color: #2c3b41;"></i></span>');
    });

    $('body').on('click', '.map-help .map-help-collapse', function () {
        let body = $(this).closest('.map-help').find('.map-help-body');
        body.toggleClass('d-none');
        isCollapsed = body.hasClass('d-none');
        $(this).find('i').removeClass('fa-angle-double-up fa-angle-double-down').addClass('fa-angle-double-' + (isCollapsed ? 'down' : 'up'));
    });
    $('body').on('click', '.map-help .map-help-close', function () {
        $(this).closest('.map-help').addClass('d-none');
    });

    //XSS in notifications
    abp.notifications.showUiNotifyForUserNotification = function (userNotification, options) {
        var message = abp.notifications.getFormattedMessageFromUserNotification(userNotification);
        message = abp.helper.dataTables.renderText(message);
        var uiNotifyFunc = abp.notifications.getUiNotifyFuncBySeverity(userNotification.notification.severity);
        uiNotifyFunc(message, undefined, options);
    };

})(jQuery);