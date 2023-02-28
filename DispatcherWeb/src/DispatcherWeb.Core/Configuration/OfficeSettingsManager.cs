using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Security;
using DispatcherWeb.Offices;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Configuration
{
    public class OfficeSettingsManager : IOfficeSettingsManager, ISingletonDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IRepository<Office> _officeRepository;
        private readonly AspNetZeroAbpSession _session;

        public OfficeSettingsManager(
            ISettingManager settingManager,
            IRepository<Office> officeRepository,
            AspNetZeroAbpSession session
            )
        {
            _settingManager = settingManager;
            _officeRepository = officeRepository;
            _session = session;
        }

        [UnitOfWork]
        public async Task<string> GetHeartlandPublicKeyAsync()
        {
            var office = await _officeRepository.GetAll()
                .Where(x => x.Id == _session.OfficeId)
                .Select(x => new
                {
                    x.HeartlandPublicKey
                }).FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(office?.HeartlandPublicKey))
            {
                return office.HeartlandPublicKey;
            }

            var heartlandPublicKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);

            return heartlandPublicKey;
        }

        [UnitOfWork]
        public async Task<string> GetHeartlandSecretKeyAsync()
        {
            var office = await _officeRepository.GetAll()
                .Where(x => x.Id == _session.OfficeId)
                .Select(x => new
                {
                    x.HeartlandSecretKey
                }).FirstOrDefaultAsync();

            var officeHeartlandSecretKey = SimpleStringCipher.Instance.Decrypt(office?.HeartlandSecretKey);

            if (!string.IsNullOrEmpty(officeHeartlandSecretKey))
            {
                return officeHeartlandSecretKey;
            }

            var heartlandSecretKey = SimpleStringCipher.Instance.Decrypt(await _settingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey));

            return heartlandSecretKey;
        }

        [UnitOfWork]
        public async Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices()
        {
            var officeIds = await _officeRepository.GetAll().Select(x => x.Id).ToListAsync();

            return await GetHeartlandKeysForOffices(officeIds);
        }

        [UnitOfWork]
        public async Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices(IEnumerable<int> officeIds)
        {
            var officeIdList = officeIds.ToList();

            var result = officeIdList.Select(x => new OfficeHeartlandKeys { OfficeId = x }).ToList();

            var officeSpecificKeys = await _officeRepository.GetAll()
                .Where(x => officeIdList.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.HeartlandPublicKey,
                    x.HeartlandSecretKey
                }).ToListAsync();

            foreach (var key in officeSpecificKeys)
            {
                var resultItem = result.First(x => x.OfficeId == key.Id);

                if (!string.IsNullOrEmpty(key.HeartlandPublicKey))
                {
                    resultItem.PublicKey = key.HeartlandPublicKey;
                }

                var secretKey = SimpleStringCipher.Instance.Decrypt(key.HeartlandSecretKey);
                if (!string.IsNullOrEmpty(key.HeartlandSecretKey))
                {
                    resultItem.SecretKey = secretKey;
                }
            }

            if (result.All(x => !string.IsNullOrEmpty(x.PublicKey) && !string.IsNullOrEmpty(x.SecretKey)))
            {
                return result;
            }

            var tenantPublicKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);
            var tenantSecretKey = SimpleStringCipher.Instance.Decrypt(await _settingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey));

            foreach (var resultItem in result)
            {
                if (string.IsNullOrEmpty(resultItem.PublicKey))
                {
                    resultItem.PublicKey = tenantPublicKey;
                }

                if (string.IsNullOrEmpty(resultItem.SecretKey))
                {
                    resultItem.SecretKey = tenantSecretKey;
                }
            }

            return result;
        }
    }
}
