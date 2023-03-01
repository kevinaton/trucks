using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Internal;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Infrastructure.Telematics.Dto.IntelliShift;
using DispatcherWeb.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUglify.Helpers;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public class IntelliShiftTelematics : IIntelliShiftTelematics, ITransientDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IAbpSession _abpSession;
        private readonly ISettingDefinitionManager _settingDefinitionManager;

        public ILogger Logger { get; set; }

        private readonly string _apiAuthUrl = string.Empty;
        private readonly string _apiBaseUrl = string.Empty;

        public IntelliShiftTelematics(
            ISettingManager settingManager,
            ILocalizationManager localizationManager,
            IAbpSession abpSession,
            ISettingDefinitionManager settingDefinitionManager
        )
        {
            _settingManager = settingManager;
            _localizationManager = localizationManager;
            _abpSession = abpSession;

            _settingDefinitionManager = settingDefinitionManager;
            _apiAuthUrl = _settingDefinitionManager.GetSettingDefinition(AppSettings.GpsIntegration.IntelliShift.ApiAuthUrl).DefaultValue;
            _apiBaseUrl = _settingDefinitionManager.GetSettingDefinition(AppSettings.GpsIntegration.IntelliShift.BaseUrl).DefaultValue;
        }

        public async Task<TokenLoginResult> LoginToApiAsync()
        {
            var settings = await _settingManager.GetIntelliShiftSettingsAsync();

            if (settings.IsEmpty())
            {
                throw new ApplicationException("IntelliShift host credential settings are missing");
            }

            using var httpClient = new HttpClient();
            var apiResponse = await httpClient.PostAsJsonAsync(_apiAuthUrl, new
            {
                email = settings.User,
                password = settings.Password
            });

            if (apiResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new UserFriendlyException(L("IntelliShiftAuthError"));
            }

            apiResponse.EnsureSuccessStatusCode();
            var jsonApiResult = await apiResponse.Content.ReadAsStringAsync();
            var apiAuthResult = TokenLoginResult.Parse(jsonApiResult);

            if (!string.IsNullOrEmpty(apiAuthResult.Error) ||
                !string.IsNullOrEmpty(apiAuthResult.ErrorDescription))
            {
                var innerException = new Exception($"{apiAuthResult.Error}: {apiAuthResult.ErrorDescription}");
                Logger.LogError(innerException, innerException.ToString());
                throw new UserFriendlyException(L("IntelliShiftAuthError"), innerException);
            }
            return apiAuthResult;
        }

        public async Task<List<TruckUnitDto>> GetAllUnitsAsync(TokenLoginResult tokenLoginResult = null)
        {
            var truckUnits = new List<TruckUnitDto>();

            if (await _settingManager.GetGpsPlatformAsync() != GpsPlatform.IntelliShift)
            {
                return truckUnits;
            }

            if (tokenLoginResult == null)
            {
                tokenLoginResult = await LoginToApiAsync();
            }

            var currentPage = 1;
            var pageSize = 250;
            TruckUnitsPage truckUnitsPage;

            using var httpClient = new HttpClient();
            do
            {
                try
                {
                    Thread.Sleep(10); //API has a limit of max 150 requests per second (one per 7ms)

                    var requestUrl = $"{_apiBaseUrl}/assets/vehicles?ShowInactive=true&PageNumber={currentPage}&PageSize={pageSize}&SortBy=Id&SortDirection=ASC&Filter=";
                    using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUrl));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLoginResult.AccessToken);
                    var rawApiResult = await httpClient.SendAsync(request);
                    if (rawApiResult.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UserFriendlyException(L("IntelliShiftAuthError"));
                    }
                    rawApiResult.EnsureSuccessStatusCode();
                    var jsonApiResult = await rawApiResult.Content.ReadAsStringAsync();

                    truckUnitsPage = TruckUnitsPage.Parse(jsonApiResult);
                    truckUnits.AddRange(truckUnitsPage.TruckUnitsCollection);
                    currentPage++;
                }
                catch (HttpRequestException e)
                {
                    Logger.LogError(e, e.ToString());
                    throw new UserFriendlyException(L("IntelliShiftError"));
                }
            }
            while (truckUnitsPage.HasMorePages);
            return truckUnits;
        }

        public async Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync()
        {
            var tokenLoginResult = await LoginToApiAsync();
            var truckUnits = await GetAllUnitsAsync(tokenLoginResult);
            var results = new List<TruckCurrentData>();

            truckUnits.ForEach(truckUnit =>
            {
                results.Add(new TruckCurrentData()
                {
                    TruckCodeOrUniqueId = truckUnit.Name,
                    CurrentHours = (double)(truckUnit.CumulativeHours ?? 0),
                    CurrentMileage = UnitConverter.ConvertMetresToMiles((double)(truckUnit.Odometer ?? 0))
                });
            });
            return results;
        }

        public async Task<bool> UpdateUnit(int remoteVehicleId,
                                            TokenLoginResult tokenLoginResult = null,
                                            params (string PropertyName, object PropertyValue)[] fieldsToUpdate)
        {
            if (tokenLoginResult == null)
            {
                tokenLoginResult = await LoginToApiAsync();
            }

            var fieldsDic = new Dictionary<string, object>();
            fieldsToUpdate.ForEach(selector =>
            {
                var jsonAttribute = TruckUnitDto.GetJsonPropertyAttribute(selector.PropertyName);
                fieldsDic.Add(jsonAttribute, selector.PropertyValue);
            });
            var fieldsJson = JsonConvert.SerializeObject(fieldsDic);

            var requestUrl = $"{_apiBaseUrl}/assets/vehicles/{remoteVehicleId}";
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Patch, new Uri(requestUrl))
            {
                Content = new StringContent(fieldsJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLoginResult.AccessToken);
            var rawApiResult = await httpClient.SendAsync(request);
            if (rawApiResult.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UserFriendlyException(L("IntelliShiftAuthError"));
            }
            rawApiResult.EnsureSuccessStatusCode();
            return rawApiResult.IsSuccessStatusCode;
        }

        #region private members

        private string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        #endregion
    }
}
