using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.ReportCenter.Models.DTO;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DispatcherWeb.ReportCenter.Services
{
    public class ReportAppService
    {
        private readonly DispatcherWebDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        private static readonly Dictionary<string, (string Description, string Path, bool HasAccess)> _reportAccessDictionary = new();

        public ReportAppService(DispatcherWebDbContext db, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task RefreshReportsDictionary()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var hostApiUrl = _configuration["IdentityServer:Authority"];

            using var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync($"{hostApiUrl}/api/services/app/permission/GetGrantedPermissions");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var availableReports = await GetAvailableReports();
                var contentJson = await response.Content.ReadAsStringAsync();
                var grantedReportNames = JObject.Parse(contentJson)
                                        .SelectTokens("$..result.items[*]")
                                        .Where(jtoken => jtoken.ToString().StartsWith("Pages.ActiveReports."))
                                        .Select(jtoken => jtoken.ToString().Replace("Pages.ActiveReports.", string.Empty));

                lock (_reportAccessDictionary)
                {
                    _reportAccessDictionary.Clear();
                }

                foreach (var (reportName, reportDescription, reportPath) in availableReports)
                {
                    lock (_reportAccessDictionary)
                    {
                        var canAccess = grantedReportNames.Any(reportName.Equals);
                        _reportAccessDictionary.Add(reportName, (reportDescription, reportPath, canAccess));
                    }
                }
            }

            await Task.CompletedTask.WaitAsync(new CancellationToken(false));
        }

        public async Task<List<ReportListItemDto>> GetAvailableReportsList()
        {
            await RefreshReportsDictionary();

            var reportListItems = _reportAccessDictionary
                                    .Select(report => new ReportListItemDto()
                                    {
                                        Name = report.Key,
                                        Description = report.Value.Description,
                                        Path = report.Value.Path
                                    })
                                    .ToList();

            return reportListItems;
        }

        public async Task<bool> CanAccessReport(string reportPath, bool refresh = false)
        {
            try
            {
                if (refresh)
                    await RefreshReportsDictionary();

                var keyValReport = _reportAccessDictionary
                                    .FirstOrDefault(p => p.Value.Path == reportPath);

                if (!_reportAccessDictionary.Keys.Any() ||
                    string.IsNullOrEmpty(keyValReport.Key) ||
                    keyValReport.Value == default)
                    return false;

                return keyValReport.Value.HasAccess;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task EnsureCanAccessReport(string reportPath)
        {
            if (!await CanAccessReport(reportPath))
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("sub")).Value;
                var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("name")).Value;
                throw new UnauthorizedAccessException($"User '{userName}' ({userId}) does not have access to the report. > {reportPath}'");
            }
        }

        #region private methods

        private async Task<List<(string Name, string Description, string Path)>> GetAvailableReports()
        {
            var reports = await _db.Reports
                    .Distinct()
                    .Select(p => new { p.Name, p.Description, p.Path })
                    .OrderBy(p => p.Name)
                    .ToListAsync();

            return reports.Select(p => (p.Name, p.Description, p.Path)).ToList();
        }

        #endregion private methods
    }
}
