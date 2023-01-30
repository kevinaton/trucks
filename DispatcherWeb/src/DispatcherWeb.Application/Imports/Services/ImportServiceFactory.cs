using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;

namespace DispatcherWeb.Imports.Services
{
    public static class ImportServiceFactory
    {
        public enum OfficeResolverType
        {
            ByName,
            ByFuelId
        }
        public static IImportDataBaseAppService GetImportAppService(
            IIocResolver iocResolver, 
            ImportType importType,
            OfficeResolverType officeResolverType
        )
        {
            IOfficeResolver officeResolver;
            switch (importType)
            {
                case ImportType.FuelUsage:
                    officeResolver = GetOfficeResolver(iocResolver, officeResolverType);
                    return iocResolver.Resolve<IImportFuelUsageAppService>(new {officeResolver});
                case ImportType.VehicleUsage:
                    officeResolver = GetOfficeResolver(iocResolver, officeResolverType);
                    return iocResolver.Resolve<IImportVehicleUsageAppService>(new { officeResolver });
                case ImportType.Customers:
                    return iocResolver.Resolve<IImportCustomersAppService>();
                case ImportType.Vendors:
                    return iocResolver.Resolve<IImportVendorsAppService>();
                case ImportType.Services:
                    officeResolver = iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByUserIdResolver));
                    return iocResolver.Resolve<IImportServicesAppService>(new { officeResolver });
                case ImportType.Trucks:
                    officeResolver = iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByUserIdResolver));
                    return iocResolver.Resolve<IImportTrucksAppService>(new { officeResolver });
                case ImportType.Employees:
                    officeResolver = iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByUserIdResolver));
                    return iocResolver.Resolve<IImportEmployeesAppService>(new { officeResolver });
                case ImportType.Trux:
                    return iocResolver.Resolve<IImportTruxEarningsAppService>();
                case ImportType.LuckStone:
                    return iocResolver.Resolve<IImportLuckStoneEarningsAppService>();
                default:
                    throw new ArgumentOutOfRangeException($"Not supported {nameof(importType)}.", nameof(importType));

            }

        }

        private static IOfficeResolver GetOfficeResolver(
            IIocResolver iocResolver,
            OfficeResolverType officeResolverType
        )
        {
            switch (officeResolverType)
            {
                case OfficeResolverType.ByName:
                    return iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByNameResolver));
                case OfficeResolverType.ByFuelId:
                    return iocResolver.Resolve<IOfficeResolver>(typeof(OfficeByFuelIdResolver));
                default:
                    throw new ArgumentOutOfRangeException($"Not supported {nameof(officeResolverType)}.", nameof(officeResolverType));
            }
        }
    }
}
