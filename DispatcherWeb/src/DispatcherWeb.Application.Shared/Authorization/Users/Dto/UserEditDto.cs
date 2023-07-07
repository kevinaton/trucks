using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Authorization.Users.Dto
{
    //Mapped to/from User in CustomDtoMapper
    public class UserEditDto : IPassivable, IOfficeIdNameDto
    {
        /// <summary>
        /// Set null to create a new user. Set user's Id to update a user
        /// </summary>
        public long? Id { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [StringLength(UserConsts.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        public int? OfficeId { get; set; }

        public string OfficeName { get; set; }

        // Not used "Required" attribute since empty value is used to 'not change password'
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool IsActive { get; set; }

        public bool ShouldChangePasswordOnNextLogin { get; set; }

        public virtual bool IsTwoFactorEnabled { get; set; }

        public virtual bool IsLockoutEnabled { get; set; }

        public bool IsSingleOffice { get; set; }

        public string Title { get; set; }

        public int? CustomerContactId { get; set; }

        public CustomerContactDto CustomerContact { get; set; }

        int IOfficeIdNameDto.OfficeId { get => OfficeId ?? 0; set => OfficeId = value; }
    }
}