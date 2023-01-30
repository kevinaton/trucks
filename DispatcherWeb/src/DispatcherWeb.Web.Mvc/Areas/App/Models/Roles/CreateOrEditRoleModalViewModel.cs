﻿using Abp.AutoMapper;
using DispatcherWeb.Authorization.Roles.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Roles
{
    [AutoMapFrom(typeof(GetRoleForEditOutput))]
    public class CreateOrEditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
    {
        public bool IsEditMode
        {
            get { return Role.Id.HasValue; }
        }

        public CreateOrEditRoleModalViewModel(GetRoleForEditOutput output)
        {
            this.Role = output.Role;
            this.Permissions = output.Permissions;
            this.GrantedPermissionNames = output.GrantedPermissionNames;
            //this.IsEditMode = IsEditMode;
        }
    }
}