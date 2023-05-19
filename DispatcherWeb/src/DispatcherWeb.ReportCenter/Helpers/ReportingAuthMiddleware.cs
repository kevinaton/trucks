using DispatcherWeb.ReportCenter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Threading.Tasks;

namespace DispatcherWeb.ReportCenter.Helpers
{
    public class ReportingAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public ReportingAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ReportAppService reportAppService)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException();
            }

            var url = UriHelper.GetEncodedUrl(context.Request);

            const string prefix = "/api/reporting/reports/";
            const string suffix = ".rdlx/";
            if (url.Contains(prefix) && url.Contains(suffix) && url.IndexOf(suffix) > url.IndexOf(prefix))
            {
                var startIndex = url.IndexOf(prefix) + prefix.Length;
                var endIndex = url.IndexOf(suffix);
                var name = url[startIndex..endIndex];

                await reportAppService.EnsureCanAccessReport(name);
            }

            await _next(context);
        }
    }
}
