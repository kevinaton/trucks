using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.TimeClassifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultTimeClassificationCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultTimeClassificationCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            if (!_context.TimeClassifications.Any(x => x.TenantId == _tenantId))
            {
                var job = new TimeClassification { TenantId = _tenantId, Name = "Drive Truck" };
                _context.TimeClassifications.Add(job);
                _context.SaveChanges();
                _context.Settings.Add(new Abp.Configuration.Setting
                {
                    Name = AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId,
                    TenantId = _tenantId,
                    Value = job.Id.ToString(),
                    CreationTime = Clock.Now
                });
                _context.TimeClassifications.Add(new TimeClassification { TenantId = _tenantId, Name = "Training" });
                _context.TimeClassifications.Add(new TimeClassification { TenantId = _tenantId, Name = "Vacation" });
                _context.TimeClassifications.Add(new TimeClassification { TenantId = _tenantId, Name = "Production Pay", IsProductionBased = true });
                _context.SaveChanges();
            }
        }
    }
}
