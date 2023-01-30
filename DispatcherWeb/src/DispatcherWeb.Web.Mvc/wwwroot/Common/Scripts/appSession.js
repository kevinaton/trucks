var app = app || {};
(function () {
  abp.services.app.session.getCurrentLoginInformations({ async: false }).done(function (result) {
      app.session = result;

      if (abp && abp.event) {
          abp.event.trigger('app.appSessionLoaded');
      }
  });
})();
