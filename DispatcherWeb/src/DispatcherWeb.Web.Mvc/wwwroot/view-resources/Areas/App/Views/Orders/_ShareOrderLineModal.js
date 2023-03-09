(function ($) {
    app.modals.ShareOrderLineModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            var checkedOffices = _$form.find('input[type="checkbox"]:checked');
            var checkedOfficeIds = [];

            checkedOffices.each(function () { checkedOfficeIds.push($(this).data('id')); });

            _modalManager.setBusy(true);
            _orderService.setSharedOrderLines({
                orderLineId: formData.OrderLineId,
                checkedOfficeIds: checkedOfficeIds
            }).done(function () {
                abp.notify.info('Shared successfully.');
                _modalManager.close();
                abp.event.trigger('app.orderModalShared');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);