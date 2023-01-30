using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Validation;
using Abp.Timing;
using Castle.Core.Internal;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Offices;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public abstract class ImportTruckDataBaseAppService<T> : ImportDataBaseAppService<T>, IImportDataBaseAppService where T: ITruckImportRow
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
