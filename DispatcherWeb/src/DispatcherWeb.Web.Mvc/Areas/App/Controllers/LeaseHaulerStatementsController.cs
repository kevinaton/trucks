using Abp.AspNetCore.Mvc.Authorization;
using DispatcherWeb.Authorization;
using DispatcherWeb.PayStatements;
using DispatcherWeb.Web.Controllers;
using DispatcherWeb.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.app.Controllers
{
    [Area("App")]
    [AbpMvcAuthorize]
    public class LeaseHaulerStatementsController : DispatcherWebControllerBase
    {
        private readonly IPayStatementAppService _payStatementAppService;

        public LeaseHaulerStatementsController(
            IPayStatementAppService payStatementAppService
        )
        {
            _payStatementAppService = payStatementAppService;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public IActionResult Index()
        {
            return View();
        }

        [Modal]
        [AbpMvcAuthorize(AppPermissions.Pages_LeaseHaulerStatements)]
        public PartialViewResult AddLeaseHaulerStatementModal()
        {
            return PartialView("_AddLeaseHaulerStatementModal");
        }

    }
}
