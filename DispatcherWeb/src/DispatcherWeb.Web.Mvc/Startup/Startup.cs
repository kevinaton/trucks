using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.Mvc.Caching;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.AspNetZeroCore.Web.Authentication.JwtBearer;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Hangfire;
using Abp.PlugIns;
using Abp.Timing;
using Castle.Facilities.Logging;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Identity;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.RecurringJobs;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Web.Chat.SignalR;
using DispatcherWeb.Web.Common;
using DispatcherWeb.Web.Extensions;
using DispatcherWeb.Web.HealthCheck;
using DispatcherWeb.Web.IdentityServer;
using DispatcherWeb.Web.Resources;
using DispatcherWeb.Web.SignalR;
using DispatcherWeb.Web.Swagger;
using Hangfire;
using Hangfire.Azure.ServiceBusQueue;
using HealthChecks.UI.Client;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Owl.reCAPTCHA;
using Stripe;
using HealthChecksUISettings = HealthChecks.UI.Configuration.Settings;

namespace DispatcherWeb.Web.Startup
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
            _hostingEnvironment = env;
        }

        private string[] GetCorsOrigins()
        {
            return _appConfiguration["App:CorsOrigins"]?.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToArray() ?? new string[0];
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            // MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            })
#if DEBUG
                .AddRazorRuntimeCompilation()
#endif
                .AddNewtonsoftJson();

            if (bool.Parse(_appConfiguration["KestrelServer:IsEnabled"]))
            {
                ConfigureKestrel(services);
            }

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromHours(2);
            });

            IdentityRegistrar.Register(services);

            //Identity server
            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                IdentityServerRegistrar.Register(services, _appConfiguration, options =>
                    options.UserInteraction = new UserInteractionOptions
                    {
                        LoginUrl = "/Account/Login",
                        LogoutUrl = "/Account/LogOut",
                        ErrorUrl = "/Error"
                    });
            }
            else
            {
                services.Configure<SecurityStampValidatorOptions>(opts =>
                {
                    opts.OnRefreshingPrincipal = SecurityStampValidatorCallback.UpdatePrincipal;
                });
            }

            AuthConfigurer.Configure(services, _appConfiguration);

            if (WebConsts.SwaggerUiEnabled)
            {
                ConfigureSwagger(services);
            }

            //Recaptcha
            services.AddreCAPTCHAV2(x =>
            {
                x.SiteKey = _appConfiguration["Recaptcha:SiteKey"];
                x.SiteSecret = _appConfiguration["Recaptcha:SecretKey"];
            });

            AttachmentHelper.StorageConnectionString = _appConfiguration["Abp:StorageConnectionString"];

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire (Enable to use Hangfire instead of default job manager)
                services.AddHangfire(config =>
                {
                    var sqlStorage = config.UseSqlServerStorage(_appConfiguration.GetConnectionString("Default"));
                    var serviceBusConnectionString = _appConfiguration["Abp:ServiceBusConnectionString"];
                    if (!string.IsNullOrEmpty(serviceBusConnectionString))
                    {
                        sqlStorage.UseServiceBusQueues(serviceBusConnectionString);
                    }
                });
                services.AddHangfireServer(o => o.WorkerCount = 1);
            }

            services.AddScoped<IWebResourceManager, WebResourceManager>();

            services.AddSignalR().AddNewtonsoftJsonProtocol();

            //if (WebConsts.GraphQL.Enabled)
            //{
            //    services.AddAndConfigureGraphQL();
            //}

            var corsOrigins = GetCorsOrigins();
            if (corsOrigins.Any())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.WithOrigins(corsOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                });
            }

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                services.AddAbpZeroHealthCheck();

                var healthCheckUISection = _appConfiguration.GetSection("HealthChecks")?.GetSection("HealthChecksUI");

                if (bool.Parse(healthCheckUISection["HealthChecksUIEnabled"]))
                {
                    services.Configure<HealthChecksUISettings>(settings =>
                    {
                        healthCheckUISection.Bind(settings, c => c.BindNonPublicProperties = true);
                    });

                    services.AddHealthChecksUI()
                        .AddInMemoryStorage();
                }
            }

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
            });

            //Configure Abp and Dependency Injection
            return services.AddAbp<DispatcherWebWebMvcModule>(options =>
            {
                //Configure Log4Net logging
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(
                        "log4net.config"
                        //_hostingEnvironment.IsDevelopment()
                        //    ? "log4net.config"
                        //    : "log4net.Production.config"
                    )
                );

                options.PlugInSources.AddFolder(Path.Combine(_hostingEnvironment.WebRootPath, "Plugins"),
                    SearchOption.AllDirectories);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var policyCollection = new HeaderPolicyCollection()
                .AddContentSecurityPolicy(builder => 
                //.AddContentSecurityPolicyReportOnly(builder => // report-only
                {
                    builder.AddReportUri()
                        .To("/app/CspReports/Post");
                    builder.AddDefaultSrc()
                        .Self()
                        .UnsafeInline()
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:WebSiteRootAddress"])}")
                        .From("https://fonts.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/")
                        .From("https://www.google.com/recaptcha/")
                        .From("https://www.youtube.com/")
#if DEBUG
                        .From("http://localhost:*") //browserLink
#endif
                        ;
                    builder.AddScriptSrc()
                        .Self()
                        .UnsafeInline()
                        .From("https://maps.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/")
                        .From("https://www.google.com/recaptcha/")
                        .From("https://www.gstatic.com/recaptcha/")
                        .From("https://az416426.vo.msecnd.net/")  //appinsights
                        .From("https://static.userguiding.com/")
#if DEBUG
                        .From("http://localhost:*") //browserLink
#endif
                        ;
                    builder.AddConnectSrc()
                        .Self()
                        .UnsafeInline()
                        .From($"wss://{Utilities.GetDomainFromUrl(_appConfiguration["App:WebSiteRootAddress"])}")
                        .From("https://maps.googleapis.com/")
                        .From("https://api2-c.heartlandportico.com")
                        .From("https://hps.github.io/token/")
                        .From("https://dc.services.visualstudio.com/v2/track") //appinsights
                        .From("https://user.userguiding.com/sdk/")
                        .From("https://nps.userguiding.com/api/")
                        .From("https://api.userguiding.com/")
                        .From("https://metrics.userguiding.com/")
#if DEBUG
                        .From("http://localhost:*") //browserLink
                        .From("ws://localhost:*")
#endif
                        ;
                    builder.AddCustomDirective("worker-src", "'self' blob:");
                    builder.AddImgSrc()
                        .Self()
                        .From("https://maps.gstatic.com/")
                        .From("https://*.googleapis.com/") //maps., khms0., khms1.googleapis.com
                        .From("https://*.mapbox.com")
                        .From("https://api-s.mqcdn.com")
                        .From("https://static.userguiding.com/")
                        .Data();
                    builder.AddFontSrc()
                        .Self()
                        .From("chrome-extension:")
                        .From("https://fonts.gstatic.com/")
                        .From("https://fonts.googleapis.com/")
                        .Data();
                    builder.AddObjectSrc()
                        .None();
                    var corsOrigins = GetCorsOrigins();
                    var frameAncestors = builder.AddFrameAncestors()
                        .Self();
                    foreach (var corsOrigin in corsOrigins)
                    {
                        frameAncestors = frameAncestors.From(corsOrigin);
                    }
                });
            app.UseSecurityHeaders(policyCollection);

            app.UseGetScriptsResponsePerUserCache();

            //Initializes ABP framework.
            app.UseAbp(options =>
            {
                options.UseAbpRequestLocalization = false; //used below: UseAbpRequestLocalization
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDispatcherWebForwardedHeaders();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("~/Error?statusCode={0}");
                app.UseExceptionHandler("/Error");
                app.UseDispatcherWebForwardedHeaders();
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();


            Clock.Provider = ClockProviders.Utc;

            app.UseAuthentication();
            app.UseCors("default");

            if (bool.Parse(_appConfiguration["Authentication:JwtBearer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware();
            }

            if (bool.Parse(_appConfiguration["IdentityServer:IsEnabled"]))
            {
                app.UseJwtTokenMiddleware("IdentityBearer");
                app.UseIdentityServer();
            }

            app.UseAuthorization();

            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                Secure = CookieSecurePolicy.Always,
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                if (scope.ServiceProvider.GetService<DatabaseCheckHelper>()
                    .Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    app.UseAbpRequestLocalization();
                }
            }

            if (WebConsts.HangfireDashboardEnabled)
            {
                //Hangfire dashboard & server (Enable to use Hangfire instead of default job manager)
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[]
                        {new AbpHangfireAuthorizationFilter(AppPermissions.Pages_Administration_HangfireDashboard)}
                });
            }

            StaticReccuringJobs.CreateAll(_appConfiguration);

            ConfigureSemaphores();

            if (bool.Parse(_appConfiguration["Payment:Stripe:IsActive"]))
            {
                StripeConfiguration.ApiKey = _appConfiguration["Payment:Stripe:SecretKey"];
            }

            //if (WebConsts.GraphQL.Enabled)
            //{
            //    app.UseGraphQL<MainSchema>();
            //    if (WebConsts.GraphQL.PlaygroundEnabled)
            //    {
            //        app.UseGraphQLPlayground(
            //            new GraphQLPlaygroundOptions()); //to explorer API navigate https://*DOMAIN*/ui/playground
            //    }
            //}

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapHub<ChatHub>("/signalr-chat");
                endpoints.MapHub<SignalRHub>("/signalr-dispatcher");

                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                }

                app.ApplicationServices.GetRequiredService<IAbpAspNetCoreConfiguration>().EndpointConfiguration
                    .ConfigureAllEndpoints(endpoints);
            });

            if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksEnabled"]))
            {
                if (bool.Parse(_appConfiguration["HealthChecks:HealthChecksUI:HealthChecksUIEnabled"]))
                {
                    app.UseHealthChecksUI();
                }
            }

            if (WebConsts.SwaggerUiEnabled)
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                //Enable middleware to serve swagger - ui assets(HTML, JS, CSS etc.)
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(_appConfiguration["App:SwaggerEndPoint"], "DispatcherWeb API V1");
                    options.IndexStream = () => Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("DispatcherWeb.Web.wwwroot.swagger.ui.index.html");
                    options.InjectBaseUrl(_appConfiguration["App:WebSiteRootAddress"]);
                }); //URL: /swagger
            }
        }

        private void ConfigureSemaphores()
        {
            Sms.SmsAppService.ConfigureSmsCallbackSemaphore(_appConfiguration);
        }

        private void ConfigureKestrel(IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                    listenOptions =>
                    {
                        var certPassword = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                        var certPath = _appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath,
                            certPassword);
                        listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                        {
                            ServerCertificate = cert
                        });
                    });
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() { Title = "DispatcherWeb API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();

                // Add summaries to swagger
                var canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (!canShowSummaries)
                {
                    return;
                }

                var mvcXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var mvcXmlPath = Path.Combine(AppContext.BaseDirectory, mvcXmlFile);
                options.IncludeXmlComments(mvcXmlPath);

                var applicationXml = $"DispatcherWeb.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                options.IncludeXmlComments(applicationXmlPath);

                var webCoreXmlFile = $"DispatcherWeb.Web.Core.xml";
                var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                options.IncludeXmlComments(webCoreXmlPath);
            }).AddSwaggerGenNewtonsoftSupport();
        }

    }
}
