using System.Collections.Generic;
using System.Linq;
using Abp.AutoMapper;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Security;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Users
{
    [AutoMapFrom(typeof(GetUserForEditOutput))]
    public class CreateOrEditUserModalViewModel : GetUserForEditOutput, IOrganizationUnitsEditViewModel
    {
        public bool CanChangeUserName
        {
            get { return User.UserName != Authorization.Users.User.AdminUserName; }
        }

        public int AssignedRoleCount
        {
            get { return Roles.Count(r => r.IsAssigned); }
        }

        public bool IsEditMode
        {
            get { return User.Id.HasValue; }
        }

        public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

        public CreateOrEditUserModalViewModel(GetUserForEditOutput output)
        {
            this.ProfilePictureId = output.ProfilePictureId;
            this.User = output.User;
            this.Roles = output.Roles;
            this.AllOrganizationUnits = output.AllOrganizationUnits;
            this.MemberedOrganizationUnits = output.MemberedOrganizationUnits;
            //output.MapTo(this);
        }
    }
}