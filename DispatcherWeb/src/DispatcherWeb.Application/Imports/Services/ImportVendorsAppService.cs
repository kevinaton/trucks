using System.Linq;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Locations;

namespace DispatcherWeb.Imports.Services
{
    public class ImportVendorsAppService : ImportDataBaseAppService<VendorImportRow>, IImportVendorsAppService
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<SupplierContact> _supplierContactRepository;

        public ImportVendorsAppService(
            IRepository<Location> locationRepository,
            IRepository<SupplierContact> supplierContactRepository
        )
        {
            _locationRepository = locationRepository;
            _supplierContactRepository = supplierContactRepository;
        }

        protected override bool ImportRow(VendorImportRow row)
        {
            var existingLocation = _locationRepository.GetAll().Where(x => x.Name == row.Name).FirstOrDefault();
            if (existingLocation != null)
            {
                return false;
            }

            var location = new Location
            {
                IsActive = row.IsActive,
                Name = row.Name,
                StreetAddress = row.Address,
                City = row.City,
                State = row.State,
                ZipCode = row.ZipCode,
                CountryCode = row.CountryCode
            };
            _locationRepository.Insert(location);

            if (!row.ContactName.IsNullOrEmpty() || !row.ContactPhone.IsNullOrEmpty())
            {
                var supplierContact = new SupplierContact
                {
                    Location = location,
                    Email = row.MainEmail,
                    Name = row.ContactName.IsNullOrEmpty() ? "Main contact" : row.ContactName,
                    Phone = row.ContactPhone,
                    Title = row.ContactTitle,
                };
                _supplierContactRepository.Insert(supplierContact);
            }

            if (!row.Contact2Name.IsNullOrEmpty() || !row.Contact2Phone.IsNullOrEmpty())
            {
                var supplierContact = new SupplierContact
                {
                    Location = location,
                    Name = row.Contact2Name.IsNullOrEmpty() ? "Alternative contact" : row.Contact2Name,
                    Phone = row.Contact2Phone
                };
                _supplierContactRepository.Insert(supplierContact);
            }

            return true;
        }

        protected override bool IsRowEmpty(VendorImportRow row)
        {
            return row.Name.IsNullOrEmpty();
        }
    }
}
