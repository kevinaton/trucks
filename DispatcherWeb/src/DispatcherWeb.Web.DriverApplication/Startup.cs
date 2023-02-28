using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DispatcherWeb.Web.DriverApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews(options =>
            {
                //options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            });
#if DEBUG
            //.AddRazorRuntimeCompilation()
#endif
            //.AddNewtonsoftJson();

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration["App:ApiUri"])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //services.Configure<RazorViewEngineOptions>(options =>
            //{
            //    options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
            //});
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
                app.UseExceptionHandler("/DriverApplication/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            const string cacheMaxAge = "600";
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                         "Cache-Control", $"public, max-age={cacheMaxAge}");
                }
            });

            app.UseRouting();

            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseCors("default");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=DriverApplication}/{action=Index}/{id?}");
            });
        }
    }
}
