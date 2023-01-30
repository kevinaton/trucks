using System;
using System.Diagnostics;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Infrastructure.BackgroundJobs;

namespace DispatcherWeb.Imports
{
    [AbpAuthorize(AppPermissions.Pages_Imports)]
    public class ImportScheduleAppService : DispatcherWebAppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public ImportScheduleAppService(
            IBackgroundJobManager backgroundJobManager
        )
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public void ScheduleImport(ScheduleImportInput input)
        {
            if (input.ImportType != ImportType.FuelUsage && input.JacobusEnergy)
            {
                throw new ArgumentException("The ImportType must be FuelUsage when the JacobusEnergy is true!");
            }
            Debug.Assert(AbpSession.TenantId != null);
            Debug.Assert(AbpSession.UserId != null);

            _backgroundJobManager.Enqueue<ImportJob, ImportJobArgs>(
                new ImportJobArgs
                {
                    RequestorUser = AbpSession.ToUserIdentifier(),
                    File = input.BlobName,
                    FieldMap = input.JacobusEnergy ? JacobusEnergyFieldMap : input.FieldMap,
                    ImportType = input.ImportType,
                    JacobusEnergy = input.JacobusEnergy,
                }
            );

        }

        private FieldMapItem[] JacobusEnergyFieldMap =>
            new[]
            {
                new FieldMapItem { StandardField = FuelUsageColumn.Office, UserField = FuelUsageFromJacobusEnergyColumn.Office},
                new FieldMapItem { StandardField = FuelUsageColumn.TruckNumber, UserField = FuelUsageFromJacobusEnergyColumn.TruckNumber},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelDateTime, UserField = FuelUsageFromJacobusEnergyColumn.FuelDateTime},
                new FieldMapItem { StandardField = FuelUsageColumn.Amount, UserField = FuelUsageFromJacobusEnergyColumn.Amount},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelRate, UserField = FuelUsageFromJacobusEnergyColumn.FuelRate},
                new FieldMapItem { StandardField = FuelUsageColumn.Odometer, UserField = FuelUsageFromJacobusEnergyColumn.Odometer},
                new FieldMapItem { StandardField = FuelUsageColumn.TicketNumber, UserField = FuelUsageFromJacobusEnergyColumn.TicketNumber},
            };

    }
}
