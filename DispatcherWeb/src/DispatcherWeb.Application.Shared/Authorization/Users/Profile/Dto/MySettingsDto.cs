using System;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using System.Collections.Generic;
using Abp.Extensions;

namespace DispatcherWeb.Authorization.Users.Profile.Dto
{
	public class MySettingsDto : CurrentUserProfileEditDto
    {
        public List<ComboboxItemDto> TimezoneItems { get; set; }

        public bool SmsVerificationEnabled { get; set; }

        public bool CanChangeUserName => UserName != AbpUserBase.AdminUserName;

        public string Code { get; set; }

        public MySettingsDto(CurrentUserProfileEditDto currentUserProfileEditDto)
        {
            this.Name = currentUserProfileEditDto.Name;
            this.Surname = currentUserProfileEditDto.Surname;
            this.UserName = currentUserProfileEditDto.UserName;
            this.EmailAddress = currentUserProfileEditDto.EmailAddress;
            this.PhoneNumber = currentUserProfileEditDto.PhoneNumber;
            this.IsPhoneNumberConfirmed = currentUserProfileEditDto.IsPhoneNumberConfirmed;
            this.Timezone = currentUserProfileEditDto.Timezone;
            this.QrCodeSetupImageUrl = currentUserProfileEditDto.QrCodeSetupImageUrl;
            this.IsGoogleAuthenticatorEnabled = currentUserProfileEditDto.IsGoogleAuthenticatorEnabled;
            this.Options = currentUserProfileEditDto.Options;
        }

        public bool CanVerifyPhoneNumber
        {
            get
            {
                return SmsVerificationEnabled && !PhoneNumber.IsNullOrEmpty() && !PhoneNumber.Trim().IsNullOrEmpty();
            }
        }
    }
}