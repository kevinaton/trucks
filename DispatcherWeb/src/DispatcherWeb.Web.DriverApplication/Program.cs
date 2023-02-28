using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DispatcherWeb.Web.DriverApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIIS()
                .UseIISIntegration()
                .UseStartup<Startup>()
            ;
    }
}
