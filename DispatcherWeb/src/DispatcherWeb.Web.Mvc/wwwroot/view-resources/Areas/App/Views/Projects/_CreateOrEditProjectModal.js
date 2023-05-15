(function ($) {
    app.modals.CreateOrEditProjectModal = function () {

        var _modalManager;
        var _projectAppService = abp.services.app.project;
        var _$form = null;
        var _$startDate = null;
        var _$endDate = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _$startDate = _$form.find("#StartDate");
            _$endDate = _$form.find("#EndDate");
            _$startDate.datepickerInit();
            _$endDate.datepickerInit();

            _$form.find("#Status").select2Init({
                showAll: true,
                allowClear: false
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            if (!abp.helper.validateStartEndDates(
                { value: _$startDate.val(), title: _$form.find('label[for="StartDate"]').text() },
                { value: _$endDate.val(), title: _$form.find('label[for="EndDate"]').text() }
            )) {
                return;
            }

            var project = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _projectAppService.editProject(project).done(function (editResult) {
                abp.notify.info('Saved successfully.');
                _modalManager.setResult(editResult);
                _modalManager.close();
                abp.event.trigger('app.createOrEditProjectModalSaved', editResult);
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);