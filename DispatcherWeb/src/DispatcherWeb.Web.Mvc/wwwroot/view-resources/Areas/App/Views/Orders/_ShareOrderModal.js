(function($) {
    app.modals.ShareOrderModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _$form = null;

        this.init = function(modalManager) {
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
            
            var checkedOffices = _$form.find(".shared-order-item:checked");
            var checkedOfficeIds = [];

            if (checkedOffices.length === 0) {
                abp.message.error('Please select at least one office');
                return;
            }

            checkedOffices.each(function () { checkedOfficeIds.push($(this).data('id')); });

            _modalManager.setBusy(true);
            _orderService.setSharedOrders({
                orderId: formData.OrderId,
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