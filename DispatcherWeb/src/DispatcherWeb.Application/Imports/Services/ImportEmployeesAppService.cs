using System;
using System.Linq;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Threading;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.DataResolvers.OfficeResolvers;
using DispatcherWeb.Imports.RowReaders;

namespace DispatcherWeb.Imports.Services
{
    public class ImportEmployeesAppService : ImportDataBaseAppService<EmployeeImportRow>, IImportEmployeesAppService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly UserManager _userManager;
        private readonly IOfficeResolver _officeResolver;
        private int? _officeId = null;

        public ImportEmployeesAppService(
            IRepository<Driver> driverRepository,
            IDriverUserLinkService driverUserLinkService,
            UserManager userManager,
            IOfficeResolver officeResolver
        )
        {
            _driverRepository = driverRepository;
            _driverUserLinkService = driverUserLinkService;
            _userManager = userManager;
            _officeResolver = officeResolver;
        }

        protected override bool CacheResourcesBeforeImport(IImportReader reader)
        {
            _officeId = _officeResolver.GetOfficeId(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            return base.CacheResourcesBeforeImport(reader);
        }

        protected override bool ImportRow(EmployeeImportRow row)
        {
            var (firstName, middle, lastName) = ParseName(row);

            var email = row.Email;
            if (email.IsNullOrEmpty())
            {
                email = null;
            }

            var hasUsersWithSameNameOrEmail = _userManager.Users.Where(x => (x.Name == firstName && x.Surname == lastName) || email != null && x.EmailAddress == email).Any();

            var hasDriversWithSameNameOrEmail = _driverRepository.GetAll().Where(x => (x.FirstName == firstName && x.LastName == lastName) || email != null && x.EmailAddress == email).Any();

            if (hasUsersWithSameNameOrEmail || hasDriversWithSameNameOrEmail)
            {
                row.AddParseErrorIfNotExist("Name", $"Driver or User already exists with the same name or email", typeof(string));
                return false;
            }

            var driver = new Driver
            {
                IsInactive = false,
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = row.Email,
                OfficeId = _officeId,
                CellPhoneNumber = row.Phone,
                Address = row.Address,
                City = row.City,
                State = row.State,
                ZipCode = row.Zip,
                OrderNotifyPreferredFormat = row.NotifyPreferredFormat ?? OrderNotifyPreferredFormat.Neither,
                TenantId = _tenantId,
            };
            _driverRepository.Insert(driver);
            CurrentUnitOfWork.SaveChanges();

            if (email == null)
            {
                row.AddParseErrorIfNotExist("Email", $"Email is empty for user {row.Name}", typeof(string));
                return true;
            }

            try
            {
                var sendEmail = row.SendEmail;
                AsyncHelper.RunSync(() => _driverUserLinkService.UpdateUser(driver, sendEmail));
            }
            catch (Exception e)
            {
                row.AddParseErrorIfNotExist("-", e.Message, typeof(string));
                return false;
            }

            return true;
        }

        private (string firstName, string middle, string lastName) ParseName(EmployeeImportRow row)
        {
            if (row.Name.Contains(","))
            {
                //assume LastName, FirstName MiddleInitial format
                var commaParts = row.Name.Split(",");
                var lastName = string.Join(",", commaParts.SkipLast(1));
                var parts = commaParts.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Last().Length == 1)
                {
                    var firstName = string.Join(" ", parts.SkipLast(1));
                    var middleName = parts.Last();
                    return (firstName, middleName, lastName);
                }
                else
                {
                    var firstName = string.Join(" ", parts);
                    return (firstName, null, lastName);
                }
            }
            else
            {
                //assume FirstName MiddleInitial LastName format
                var parts = row.Name.Split(" ");
                if (parts.Length == 1)
                {
                    return (parts.First(), null, "-");
                }

                if (parts.Length == 2)
                {
                    return (parts.First(), null, parts.Last());
                }

                if (parts.Length == 3 && parts[1].EndsWith("."))
                {
                    return (parts.First(), parts[1], parts.Last());
                }

                return (string.Join(" ", parts.Take(parts.Length - 1)), null, parts.Last());
            }
        }

        protected override bool IsRowEmpty(EmployeeImportRow row)
        {
            return row.Name.IsNullOrEmpty();
        }
    }
}
