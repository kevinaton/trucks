using System.Collections.Generic;
using System.Linq;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Services;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportServicesAppService : ImportDataBaseAppService<ServiceImportRow>, IImportServicesAppService
    {
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<OfficeServicePrice> _officeServicePriceRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly IOfficeResolver _officeResolver;
        private Dictionary<string, int> _uomCache = null;
        private int? _officeId = null;

        public ImportServicesAppService(
            IRepository<Service> serviceRepository,
            IRepository<OfficeServicePrice> officeServicePriceRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IOfficeResolver officeResolver
        )
        {
            _serviceRepository = serviceRepository;
            _officeServicePriceRepository = officeServicePriceRepository;
            _uomRepository = uomRepository;
            _officeResolver = officeResolver;
        }

        protected override bool CacheResourcesBeforeImport(IImportReader reader)
        {
            _uomCache = _uomRepository.GetAll()
                .Select(x => new { x.Id, x.Name })
                .ToDictionary(x => x.Name, x => x.Id);

            _officeId = _officeResolver.GetOfficeId(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            return base.CacheResourcesBeforeImport(reader);
        }

        protected override bool ImportRow(ServiceImportRow row)
        {
            var serviceType = ParseServiceType(row);
            if (serviceType == null)
            {
                return false;
            }

            var service = _serviceRepository.GetAll()
                .Include(x => x.OfficeServicePrices)
                .Where(x => x.Service1 == row.Service1).FirstOrDefault();
            if (service == null)
            {
                service = new Service
                {
                    Service1 = row.Service1
                };
                _serviceRepository.Insert(service);
            }

            service.IsActive = row.IsActive;
            service.Description = row.Description;
            service.Type = serviceType;
            service.IsTaxable = row.IsTaxable;
            service.IncomeAccount = row.IncomeAccount;
            service.IsInQuickBooks = true;

            if (row.Price != null && _officeId.HasValue && ServiceTypeHasPricing(serviceType))
            {
                var uomId = ParseUomId(row);
                OfficeServicePrice officeServicePrice = null;
                if (service.Id > 0)
                {
                    switch (serviceType)
                    {
                        case ServiceType.Service:
                        case ServiceType.OtherCharge:
                            officeServicePrice = service?.OfficeServicePrices.FirstOrDefault(x => x.OfficeId == _officeId.Value && x.FreightUomId == uomId && x.Designation == DesignationEnum.FreightOnly);
                            break;
                        case ServiceType.InventoryPart:
                        case ServiceType.InventoryAssembly:
                        case ServiceType.NonInventoryPart:
                            officeServicePrice = service?.OfficeServicePrices.FirstOrDefault(x => x.OfficeId == _officeId.Value && x.MaterialUomId == uomId && x.Designation == DesignationEnum.MaterialOnly);
                            break;
                        case ServiceType.Discount:
                        case ServiceType.Payment:
                        case ServiceType.SalesTaxItem:
                            break;
                    }
                }
                if (officeServicePrice == null)
                {
                    officeServicePrice = new OfficeServicePrice
                    {
                        Service = service,
                        OfficeId = _officeId.Value,
                    };
                    _officeServicePriceRepository.Insert(officeServicePrice);
                }

                switch (serviceType)
                {
                    case ServiceType.Service:
                    case ServiceType.OtherCharge:
                        officeServicePrice.FreightRate = row.Price;
                        officeServicePrice.Designation = DesignationEnum.FreightOnly;
                        officeServicePrice.FreightUomId = uomId;
                        break;
                    case ServiceType.InventoryPart:
                    case ServiceType.InventoryAssembly:
                    case ServiceType.NonInventoryPart:
                        officeServicePrice.PricePerUnit = row.Price;
                        officeServicePrice.Designation = DesignationEnum.MaterialOnly;
                        officeServicePrice.MaterialUomId = uomId;
                        break;
                    case ServiceType.Discount:
                    case ServiceType.Payment:
                    case ServiceType.SalesTaxItem:
                        break;
                }
            }

            return true;
        }

        private bool ServiceTypeHasPricing(ServiceType? serviceType)
        {
            switch (serviceType)
            {
                default:
                case ServiceType.Discount:
                case ServiceType.Payment:
                case ServiceType.SalesTaxItem:
                    return false;
                case ServiceType.Service:
                case ServiceType.OtherCharge:
                case ServiceType.InventoryPart:
                case ServiceType.InventoryAssembly:
                case ServiceType.NonInventoryPart:
                    return true;
            }
        }

        private ServiceType? ParseServiceType(ServiceImportRow row)
        {
            var typeString = row.Type;

            if (Utilities.TryGetEnumFromDisplayName<ServiceType>(typeString, out var type))
            {
                return type;
            }

            row.AddParseErrorIfNotExist("Type", row.Type, typeof(string));
            return null;

            //var serviceTypes = new[] { "Service", "Other Charge" };
            //var materialTypes = new[] { "Inventory Part", "Inventory Assembly", "Non-inventory Part" };
            //var ignoreTypes = new[] { "Subtotal", "Group", "Discount", "Payment", "Sales Tax Item", "Sales Tax Group" };

            //if (string.IsNullOrEmpty(typeString))
            //{
            //    return null;
            //}

            //if (serviceTypes.Any(s => typeString.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    return true;
            //}

            //if (materialTypes.Any(s => typeString.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    return false;
            //}

            //if (ignoreTypes.Any(s => typeString.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    return null;
            //}
        }

        private int? ParseUomId(ServiceImportRow row)
        {
            if (_uomCache?.Keys.Any() != true)
            {
                return null;
            }

            if (!row.Uom.IsNullOrEmpty())
            {
                foreach (var uom in _uomCache.Keys)
                {
                    if (row.Uom.ToLower().Contains(uom.ToLower().TrimEnd('s')))
                    {
                        return _uomCache[uom];
                    }
                }
                row.AddParseErrorIfNotExist("UOM", row.Uom, typeof(string));
            }

            return _uomCache.First().Value;
        }

        protected override bool IsRowEmpty(ServiceImportRow row)
        {
            return row.Service1.IsNullOrEmpty();
        }
    }
}
