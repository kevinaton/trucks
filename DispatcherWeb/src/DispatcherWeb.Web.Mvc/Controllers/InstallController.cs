﻿using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Abp.Domain.Uow;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Install;
using DispatcherWeb.Migrations.Seed.Host;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace DispatcherWeb.Web.Controllers
{
    [DisableAuditing]
    public class InstallController : AbpController
    {
        private readonly IInstallAppService _installAppService;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DatabaseCheckHelper _databaseCheckHelper;
        private readonly DefaultLanguagesCreator _defaultLanguagesCreator;

        public InstallController(
            IInstallAppService installAppService,
            IHostApplicationLifetime applicationLifetime,
            DatabaseCheckHelper databaseCheckHelper,
            DefaultLanguagesCreator defaultLanguagesCreator)
        {
            _installAppService = installAppService;
            _applicationLifetime = applicationLifetime;
            _databaseCheckHelper = databaseCheckHelper;
            _defaultLanguagesCreator = defaultLanguagesCreator;
        }

        [UnitOfWork(IsDisabled = true)]
        public ActionResult Index()
        {
            var appSettings = _installAppService.GetAppSettingsJson();
            var connectionString = GetConnectionString();

            if (_databaseCheckHelper.Exist(connectionString))
            {
                return RedirectToAction("Index", "Home");
            }

            //TODO: Commented out on the .NET 6 upgrade to build the project. Add the correct implementation later.
            throw new System.NotImplementedException();
            //var model = new InstallViewModel
            //{
            //    Languages = _defaultLanguagesCreator.GetInitialLanguages(),
            //    AppSettingsJson = appSettings
            //};

            //return View(model);
        }

        public ActionResult Restart()
        {
            _applicationLifetime.StopApplication();
            return View();
        }

        private string GetConnectionString()
        {
            var appsettingsjson = JObject.Parse(System.IO.File.ReadAllText("appsettings.json"));
            var connectionStrings = (JObject)appsettingsjson["ConnectionStrings"];
            return connectionStrings.Property(DispatcherWebConsts.ConnectionStringName).Value.ToString();
        }
    }
}