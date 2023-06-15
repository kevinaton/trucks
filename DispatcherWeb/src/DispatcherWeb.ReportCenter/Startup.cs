using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions;
using DispatcherWeb.ReportCenter.Services;
using GrapeCity.ActiveReports.Aspnetcore.Viewer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DispatcherWeb.ReportCenter
{
    public class Startup
    {
        public static string EmbeddedReportsPrefix = "DispatcherWeb.ReportCenter.Reports";
        public IConfiguration Configuration { get; }
        public IHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Env = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            if (Env.IsDevelopment())
                IdentityModelEventSource.ShowPII = true;

            var connectionString = Configuration.GetConnectionString("Default");

            services.AddHttpContextAccessor();
            services.AddScoped<ReportAppService>();
            services.AddScoped<TenantStatisticsReportDataDefinitions>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                    .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(60))
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.Authority = Configuration["IdentityServer:Authority"];
                        options.ClientId = Configuration["IdentityServer:ClientId"];
                        options.ClientSecret = Configuration["IdentityServer:Secret"];
                        options.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken; //"id_token token";
                        options.RequireHttpsMetadata = !Env.IsDevelopment();
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("default-api");
                        options.SaveTokens = true;
                    });

            services
                .AddReporting()
                .AddMvc(option =>
                {
                    option.EnableEndpointRouting = false;
                    option.Filters.Add(new AuthorizeFilter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseHsts();

            app.UseStatusCodePages();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWhen(context => UriHelper.GetEncodedUrl(context.Request).Contains("/api/reporting"), (app) =>
            {
                app.UseMiddleware<ReportingAuthMiddleware>();
            });

            app.UseReporting(settings =>
            {
                //https://www.grapecity.com/forums/ar-dev/customstore-method-called-multiple-times
                var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10000 });

                settings.UseEmbeddedTemplates(EmbeddedReportsPrefix, Assembly.GetAssembly(GetType()));
                settings.UseCompression = true;
                settings.UseCustomStore(reportId =>
                {
                    //cache.Remove(reportId);
                    return cache.GetOrCreate(reportId, entry =>
                    {
                        entry.SetSlidingExpiration(TimeSpan.FromSeconds(30));
                        entry.SetSize(1000);
                        reportId = reportId.Replace(".rdlx", string.Empty);

                        using var scope = app.ApplicationServices.CreateScope();
                        var reportAppSrvc = scope.ServiceProvider.GetRequiredService<ReportAppService>();
                        var reportDataDefinition = reportAppSrvc.GetReportDataDefinition(reportId, true).Result;

                        return reportDataDefinition.ThisPageReport.Document.PageReport.Report;
                    });
                });

                settings.SetLocateDataSource(args =>
                {
                    var reportId = args.Report.Description.Replace(".rdlx", string.Empty);

                    using var scope = app.ApplicationServices.CreateScope();
                    var reportAppSrvc = scope.ServiceProvider.GetRequiredService<ReportAppService>();
                    var reportDataDefinition = reportAppSrvc.GetReportDataDefinition(reportId).Result;
                    var (_, dataSourceJson) = reportDataDefinition.LocateDataSource(args).Result;

                    return dataSourceJson;
                });
            });

            app.UseMvc();
        }

    }
}
