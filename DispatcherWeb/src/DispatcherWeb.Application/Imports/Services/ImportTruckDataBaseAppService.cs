using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Castle.Core.Internal;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Imports.Services
{
    public abstract class ImportTruckDataBaseAppService<T> : ImportDataBaseAppService<T>, IImportDataBaseAppService where T : ITruckImportRow
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IOfficeResolver _officeResolver;

        protected ImportTruckDataBaseAppService(
            IRepository<Truck> truckRepository,
            IOfficeResolver officeResolver
        )
        {
            _truckRepository = truckRepository;
            _officeResolver = officeResolver;
        }

        protected override bool ImportRow(T row)
        {
            int? officeId = null;
            if (!row.Office.IsNullOrEmpty())
            {
                officeId = _officeResolver.GetOfficeId(row.Office);
                if (officeId == null)
                {
                    _result.NotFoundOffices.Add(row.Office);
                    return false;
                }
            }
            else
            {
                var truckInOffices = _truckRepository.GetAll()
                    .Where(t => t.TruckCode == row.TruckNumber && t.LocationId != null)
                    .Select(t => new
                    {
                        t.TruckCode,
                        OfficeName = t.Office.Name,
                    })
                    .Distinct()
                    .ToList();
                if (truckInOffices.Count > 1)
                {
                    _result.TruckCodeInOffices.AddRange(
                        truckInOffices.GroupBy(t => t.TruckCode).Select(g => (g.Key, g.Select(t => t.OfficeName).ToList())).ToList()
                    );
                    return false;
                }
            }

            var truckId = GetTruckId(row.TruckNumber, officeId);
            if (truckId == null)
            {
                _result.NotFoundTrucks.Add(row.TruckNumber);
                return false;
            }

            return ImportRow(row, truckId.Value);
        }

        protected abstract bool ImportRow(T row, int truckId);

        protected int? GetTruckId(string truckNumber, int? officeId)
        {
            return _truckRepository.GetAll()
                .Where(t => t.LocationId != null) // Exclude Lease Hauler trucks
                .WhereIf(officeId.HasValue, t => t.LocationId == officeId.Value)
                .Where(t => t.TruckCode == truckNumber)
                .Select(t => (int?)t.Id)
                .FirstOrDefault();
        }
    }
}
