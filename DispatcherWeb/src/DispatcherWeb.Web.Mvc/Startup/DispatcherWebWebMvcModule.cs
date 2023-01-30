using System.IO;
using System.Reflection;
using Abp.AspNetZeroCore;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.IO;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Web.Sessions;
using Abp.Web.Timing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.BackgroundWorkers;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Storage;
using DispatcherWeb.Web.Areas.App.Startup;
using DispatcherWeb.Web.Session;
using DispatcherWeb.Web.Timing;

namespace DispatcherWeb.Web.Startup
{
    [DependsOn(
        typeof(DispatcherWebWebCoreModule)
    )]
    public class DispatcherWebWebMvcModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public DispatcherWebWebMvcModule(
            IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:WebSiteRootAddress"] ?? "https://localhost:44332/";
            Configuration.Modules.AspNetZero().LicenseCode = _appConfiguration["AbpZeroLicenseCode"];
            Configuration.Navigation.Providers.Add<AppNavigationProvider>();
            
            Configuration.ReplaceService<ISessionScriptManager, DispatcherWebSessionScriptManager>();
            Configuration.ReplaceService<ITimingScriptManager, DispatcherWebTimingScriptManager>();
            Configuration.ReplaceService<IBinaryObjectManager, AzureBlobBinaryObjectManager>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebWebMvcModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            SetAppFolders();

            if (!IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
            {
                return;
            }

            using (var scope = IocManager.CreateScope())
            {
                if (!scope.Resolve<DatabaseCheckHelper>().Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    return;
                }
            }

            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
            workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
            workManager.Add(IocManager.Resolve<DeleteExpiredImportFilesWorker>());
        }

        private void SetAppFolders()
        {
            var appFolders = IocManager.Resolve<AppFolders>();

            appFolders.SampleProfileImagesFolder = Path.Combine(_env.WebRootPath, @"Common\Images\SampleProfilePics");
            appFolders.TempFileDownloadFolder = Path.Combine(_env.WebRootPath, @"Temp\Downloads");
            appFolders.WebLogsFolder = Path.Combine(_env.ContentRootPath, @"App_Data\Logs");

            if(_env.IsDevelopment())
            {
                var currentAssemblyDirectoryPath = Assembly.GetExecutingAssembly().GetDirectoryPathOrNull();
                if(currentAssemblyDirectoryPath != null)
                {
                    appFolders.WebLogsFolder = Path.Combine(currentAssemblyDirectoryPath, @"App_Data\Logs");
                }
            }

            try
            {
                DirectoryHelper.CreateIfNotExists(appFolders.TempFileDownloadFolder);
            }
            catch { }
        }

    }
}