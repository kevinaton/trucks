﻿var reportViewerModule = (function () {

    var _settings = {
        productsInProgressLoading: false
    };

    let __viewer;
    var _initialized = false;

    var _init = function (settings) {
        _settings = $.extend(_settings, settings);

        // Initialize to open the report in the viewer;
        __viewer = GrapeCity.ActiveReports.JSViewer.create({
            element: '#viewerContainer',
            reportParameters: [{ name: 'TenantId', values: [_settings.tenantId] }],

            action: (actionType, actionParams) => console.log('Action type: ' + actionType + '; Action parameters: ' + actionParams),
            error: (error) => {
                if (error.message) {
                    alert(`Internal error! Please ask administrator. ${error.message}`);
                    // do not show default error message.
                    //return true;
                }
            },
            documentLoaded: () => {
                if (!$(".gc-menu__panel-container--visible").length) {
                    $("button[title='Parameters']").click();
                    if (!$(".gc-btn-pin--pinned").length)
                        setTimeout(() => $("button[title='Pin']").click(), 100);
                }
                else {
                    if (!$(".gc-btn-pin--pinned").length)
                        $("button[title='Pin']").click();
                }
            },

            reportLoaded: (reportInfo) => {
                //console.log('reportLoaded : reportInfo >', reportInfo);
                if (_initialized) return;

                setTimeout(() => {
                    // work-around to toggle the panel pinned (as default)
                    if (!$(".gc-menu__panel-container--visible").length) {
                        $("button[title='Parameters']").click();
                        if (!$(".gc-btn-pin--pinned").length)
                            setTimeout(() => $("button[title='Pin']").click(), 100);
                    }
                    else {
                        if (!$(".gc-btn-pin--pinned").length)
                            $("button[title='Pin']").click();
                    }

                    _initialized = true;

                }, 100);
            },

            error: function (error) {
                console.log("error");
            }
        });

        __viewer.openReport(`${_settings.reportPathSanitized}`);
    };

    return {
        init: _init
    };

})();