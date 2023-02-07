(function () {
	$(function () {

		var _driverMessageService = abp.services.app.driverMessage;
		var _dtHelper = abp.helper.dataTables;

		var _sendMessageModal = new app.ModalManager({
			viewUrl: abp.appPath + 'app/DriverMessages/SendMessageModal',
			scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverMessages/_SendMessageModal.js',
			modalClass: 'SendMessageModal'
		});

		var _viewMessageModal = new app.ModalManager({
			viewUrl: abp.appPath + 'app/DriverMessages/ViewMessageModal',
			scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/DriverMessages/_ViewMessageModal.js',
			modalClass: 'ViewMessageModal'
		});

		initFilterControls();

		var driverMessagesTable = $('#DriverMessagesTable');
        var driverMessagesGrid = driverMessagesTable.DataTableInit({
			stateSave: true,
			stateDuration: 0,
			stateLoadCallback: function (settings, callback) {
				app.localStorage.getItem('drivermessages_filter', function (result) {
					var filter = result || {};

					if (filter.date) {
						$('#DateFilter').val(filter.date);
                    } else {
                        $('#DateFilter').val('');
                    }
					if (filter.officeId) {
						abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), filter.officeId, filter.officeName);
					}
					if (filter.driverId) {
						abp.helper.ui.addAndSetDropdownValue($("#DriverIdFilter"), filter.driverId, filter.driverName);
					}
					if (filter.userId) {
						abp.helper.ui.addAndSetDropdownValue($("#UserIdFilter"), filter.userId, filter.userName);
					}

					app.localStorage.getItem('drivermessages_grid', function (result) {
						callback(JSON.parse(result));
					});
				});
			},
			stateSaveCallback: function (settings, data) {
				delete data.columns;
				delete data.search;
				app.localStorage.setItem('drivermessages_grid', JSON.stringify(data));
				app.localStorage.setItem('drivermessages_filter', _dtHelper.getFilterData());
			},
			ajax: function (data, callback, settings) {
				var abpData = _dtHelper.toAbpData(data);
				$.extend(abpData, _dtHelper.getFilterData());
				$.extend(abpData, _dtHelper.getDateRangeObject(abpData.date, 'dateBegin', 'dateEnd'));
				console.log(abpData);
				_driverMessageService.getDriverMessagePagedList(abpData).done(function (abpResult) {
					callback(_dtHelper.fromAbpResult(abpResult));
					abp.helper.ui.initControls();
				});
			},
			order: [[1, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0
				},
                {
                    targets: 1,
					data: "timeSent",
					title: "Sent",
					render: function (data, type, full, meta) { return _dtHelper.renderDateTime(data); }
				},
				{
					targets: 2,
					data: 'id',
					width: '25px',
					orderable: false,
					render: function (data, type, full, meta) {
						return getStatusIcon(full);
						//var icon = abp.helper.ui.getEmailDeliveryStatusIcon(full.emailStatus);
						//console.log(icon);
						//if (!icon) {
						//	return '';
						//}
						//icon.addClass('clickable').addClass('email-delivery-status');
						//return $("<div>").append(icon).html();
					},
					title: ""
				},
                {
                    targets: 3,
                    responsivePriority: 1,
                	data: "driver",
                	title: "Driver"
                },
                {
                    targets: 4,
                	data: "sentBy",
                	title: "Sent by"
                },
                {
                    targets: 5,
                	data: "subject",
                	title: "Subject"
                },
                {
                    targets: 6,
                	data: "body",
                	title: "Body"
                },
                {
                    targets: 7,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2, 
                    defaultContent: '<div class="dropdown action-button">'
						+ '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnViewRow" title="View"><i class="fa fa-eye"></i> View</a></li>'
                        + '</ul>'
						+ '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>' 
                	
                }
			]
		});

		function getStatusIcon(full) {
			if (full.messageType === abp.enums.driverMessageType.email) {
				return getEmailStatusIcon(full.emailStatus);
			} 
			if (full.messageType === abp.enums.driverMessageType.sms) {
				return getSmsStatusIcon(full.smsStatus);
			} 
			return '';
		}
		function getEmailStatusIcon(emailStatus) {
			var icon = '';
			switch (emailStatus) {
				case abp.enums.emailDeliveryStatus.opened:
					icon = 'envelope-open';
					break;
				case abp.enums.emailDeliveryStatus.dropped:
				case abp.enums.emailDeliveryStatus.deferred:
				case abp.enums.emailDeliveryStatus.bounced:
					icon = 'exclamation-triangle';
					break;
				default:
					icon = 'envelope';
			}
			return '<i class="fas fa-' + icon + '"></i>';
		}
		function getSmsStatusIcon(smsStatus) {
			var icon = '';
			switch (smsStatus) {
				case abp.enums.smsStatus.delivered:
				case abp.enums.smsStatus.received:
				case abp.enums.smsStatus.receiving:
				case abp.enums.smsStatus.sending:
				case abp.enums.smsStatus.sent:
					icon = 'comment';
					break;
				case abp.enums.emailDeliveryStatus.failed:
				case abp.enums.emailDeliveryStatus.undelivered:
					icon = 'exclamation-triangle';
					break;
				default:
					icon = 'comment';
			}
			return '<i class="fas fa-' + icon + '"></i>';
		}

		function initFilterControls() {
			$("#OfficeIdFilter").select2Init({
				abpServiceMethod: abp.services.app.office.getOfficesSelectList,
				showAll: true,
				allowClear: true
			});
			if (abp.session.officeId) {
				abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
			}
			//$('#OfficeIdFilter').change(function () {
			//	$('#DriverIdFilter').val('');
			//});

			$("#DateFilter").daterangepicker(
				{
					locale: {
						cancelLabel: 'Clear'
					}
				})
				.on('apply.daterangepicker', function (ev, picker) {
					$(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
				})
				.on('cancel.daterangepicker', function (ev, picker) {
					$(this).val('');
                });
			$("#DriverIdFilter").select2Init({
				abpServiceMethod: abp.services.app.driver.getDriversSelectList,
				showAll: false,
				allowClear: true
			});
			$("#UserIdFilter").select2Init({
				abpServiceMethod: abp.services.app.user.getUsersSelectList,
				showAll: false,
				allowClear: true
			});
		}


		var reloadMainGrid = function () {
			driverMessagesGrid.ajax.reload();
		};

		abp.event.on('app.sendMessageModalSaved', function () {
			reloadMainGrid();
		});

		driverMessagesTable.on('click', '.btnViewRow', function () {
			var driverMessageId = _dtHelper.getRowData(this).id;
			_viewMessageModal.open({ id: driverMessageId });
		});


		$("#SendNewMessageButton").click(function (e) {
			e.preventDefault();
			_sendMessageModal.open();
		});

		$("#SearchButton").closest('form').submit(function (e) {
			e.preventDefault();
			reloadMainGrid();
		});

		$("#ClearSearchButton").click(function () {
			$(this).closest('form')[0].reset();
			$(".filter").change();
			reloadMainGrid();
		});

	});
})();