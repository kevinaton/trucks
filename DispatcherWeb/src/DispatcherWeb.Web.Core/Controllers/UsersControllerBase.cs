using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Storage;
using Abp.BackgroundJobs;
using DispatcherWeb.Authorization;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization.Users.Importing;

namespace DispatcherWeb.Web.Controllers
{
    public abstract class UsersControllerBase : DispatcherWebControllerBase
    {

        protected UsersControllerBase()
        {
        }
    }
}
