using System;
using System.IO;
using Abp;
using Abp.AspNetZeroCore;
using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Security.Recaptcha;
using DispatcherWeb.Tests.Configuration;
using DispatcherWeb.Tests.DependencyInjection;
using DispatcherWeb.Tests.TestInfrastructure;
using DispatcherWeb.Tests.Url;
using DispatcherWeb.Tests.Web;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace DispatcherWeb.Tests
{
    [DependsOn(
        typeof(DispatcherWebApplicationModule),
        typeof(DispatcherWebEntityFrameworkCoreModule),
        typeof(AbpTestBaseModule))]
    public class DispatcherWebTestModule : AbpModule
    {
        public DispatcherWebTestModule(
            DispatcherWebEntityFrameworkCoreModule abpZeroTemplateEntityFrameworkCoreModule,
            IIocManager iocManager
        )
        {
            iocManager.Register<IWebHostEnvironment, FakeHostingEnvironment>();
            iocManager.Register<ILogger, NullLogger>();
            abpZeroTemplateEntityFrameworkCoreModule.SkipDbContextRegistration = true;
        }

        public override void PreInitialize()
        {
            var configuration = GetConfiguration();

            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;

            //Deprecated: Automapper will remove static API. See https://github.com/aspnetboilerplate/aspnetboilerplate/issues/4667
            //We'll just remove the below line when they remove the static mapper, since we were already disabling it here
#pragma warning disable CS0618 // Type or member is obsolete.
            //Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
            Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;
#pragma warning restore CS0618 // Type or member is obsolete

            //Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator>();

            IocManager.Register<IAppUrlService, FakeAppUrlService>();
            IocManager.Register<IWebUrlService, FakeWebUrlService>();
            IocManager.Register<IRecaptchaValidator, FakeRecaptchaValidator>();

            Configuration.ReplaceService<IAppConfigurationAccessor, TestAppConfigurationAccessor>();
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);

            Configuration.Modules.AspNetZero().LicenseCode = configuration["AbpZeroLicenseCode"];

            //Uncomment below line to write change logs for the entities below:
            Configuration.EntityHistory.IsEnabled = true;
            Configuration.EntityHistory.Selectors.Add("DispatcherWebEntities", typeof(User), typeof(Tenant));
        }

        public override void Initialize()
        {
            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>()
            where TService : class
        {
            IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton()
            );
        }

        private static IConfigurationRoot GetConfiguration()
        {
            return AppConfigurations.Get(Directory.GetCurrentDirectory(), addUserSecrets: true);
        }

    }
}
