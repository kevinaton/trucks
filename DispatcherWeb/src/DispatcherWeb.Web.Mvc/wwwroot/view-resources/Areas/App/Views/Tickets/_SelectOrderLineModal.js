(function ($) {
    app.modals.SelectOrderLineModal = function () {

        var _modalManager;
        var _ticketService = abp.services.app.ticket;
        var _$form = null;
        //var _$orderLineSelect;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find('#OrderLineId').select2Init({
                allowClear: false,
                width: "100%"
            });

        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var orderLine = _$form.serializeFormToObject();
            console.log(orderLine);
            abp.event.trigger('app.selectOrderLineModalSaved', orderLine.OrderLineId);

            _modalManager.close();
        };
    };
})(jQuery);