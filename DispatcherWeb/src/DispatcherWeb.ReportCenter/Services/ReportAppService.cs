﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DispatcherWeb.ActiveReports.Dto;
using DispatcherWeb.ReportCenter.Helpers;
using DispatcherWeb.ReportCenter.Models.ReportDataDefinitions.Base;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace DispatcherWeb.ReportCenter.Services
{
    public class ReportAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private static readonly Dictionary<string, (string Description, string Path, bool HasAccess)> _reportAccessDictionary = new();

        public ReportAppService(IServiceProvider serviceProvider,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
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
                var activeReports = await GetActiveReports();
                var contentJson = await response.Content.ReadAsStringAsync();
                var grantedReportNames = JObject.Parse(contentJson)
                                        .SelectTokens("$..result.items[*]")
                                        .Where(jtoken => jtoken.ToString().StartsWith("Pages.ActiveReports."))
                                        .Select(jtoken => jtoken.ToString().Replace("Pages.ActiveReports.", string.Empty));

                lock (_reportAccessDictionary)
                {
                    _reportAccessDictionary.Clear();
                }

                foreach (var report in activeReports)
                {
                    lock (_reportAccessDictionary)
                    {
                        var canAccess = grantedReportNames.Any(report.Name.Equals);
                        _reportAccessDictionary.Add(report.Name, (report.Description, report.Path, canAccess));
                    }
                }
            }
        }

        public async Task<List<ActiveReportListItemDto>> GetAvailableReportsList()
        {
            await RefreshReportsDictionary();

            var reportListItems = _reportAccessDictionary
                                    .Select(report => new ActiveReportListItemDto()
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

        public bool TryGetReport(string reportId, out (string Description, string Path, bool HasAccess) reportInfo)
        {
            if (_reportAccessDictionary.Count == 0)
            {
                reportInfo = (string.Empty, string.Empty, false);
                return false;
            }

            reportInfo = _reportAccessDictionary[reportId.Replace(".rdlx", string.Empty)];
            return true;
        }

        public async Task<IReportDataDefinition> GetReportDataDefinition(string reportId, bool initialize = false)
        {
            var reportDataDefinition = _serviceProvider.Identify(reportId);
            if (initialize)
                await reportDataDefinition.Initialize();
            return reportDataDefinition;
        }

        #region private methods

        private async Task<List<ActiveReportListItemDto>> GetActiveReports()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var hostApiUrl = _configuration["IdentityServer:Authority"];

            using var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var response = await client.GetAsync($"{hostApiUrl}/api/services/app/activeReports/getActiveReportsList");

            var availableReports = new List<ActiveReportListItemDto>();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var contentJson = await response.Content.ReadAsStringAsync();

                availableReports = JObject.Parse(contentJson)
                                        .SelectTokens("$..result[*]")
                                        .Select(jtoken => ActiveReportListItemDto.ReadFromJson(jtoken.ToString()))
                                        .ToList();
            }

            return availableReports;
        }

        #endregion private methods
    }
}
