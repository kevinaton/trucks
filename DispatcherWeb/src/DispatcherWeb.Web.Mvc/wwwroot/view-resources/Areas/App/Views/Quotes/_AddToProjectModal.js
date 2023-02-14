(function($) {
    app.modals.AddToProjectModal = function () {

        var _modalManager;
        var _$form = null;
        var _projectDropdown = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _projectDropdown = _$form.find("#AddToProjectId");
            
            _projectDropdown.select2Init({
                abpServiceMethod: abp.services.app.project.getActiveOrPendingProjectsSelectList,
                showAll: false,
                allowClear: false,
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            _modalManager.close();
            abp.event.trigger('app.addToProjectModalSaved', {
                id: _projectDropdown.val(),
                name: _projectDropdown.getSelectedDropdownOption().text()
            });
        };

    };
})(jQuery);