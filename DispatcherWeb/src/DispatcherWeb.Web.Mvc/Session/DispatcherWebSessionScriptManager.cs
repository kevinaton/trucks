using System.Text;
using Abp.Dependency;
using Abp.Runtime.Session;
using Abp.Web.Sessions;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Web.Utils;

namespace DispatcherWeb.Web.Session
{
    public class DispatcherWebSessionScriptManager : ISessionScriptManager, ISingletonDependency
    {
        public IAbpSession AbpSession { get; set; }

        public DispatcherWebSessionScriptManager()
        {
            AbpSession = NullAbpSession.Instance;
        }

        public string GetScript()
        {
            var script = new StringBuilder();

            script.AppendLine("(function(){");
            script.AppendLine();

            script.AppendLine("    abp.session = abp.session || {};");
            script.AppendLine("    abp.session.userId = " + (AbpSession.UserId.HasValue ? AbpSession.UserId.Value.ToString() : "null") + ";");
            script.AppendLine("    abp.session.tenantId = " + (AbpSession.TenantId.HasValue ? AbpSession.TenantId.Value.ToString() : "null") + ";");
            script.AppendLine("    abp.session.impersonatorUserId = " + (AbpSession.ImpersonatorUserId.HasValue ? AbpSession.ImpersonatorUserId.Value.ToString() : "null") + ";");
            script.AppendLine("    abp.session.impersonatorTenantId = " + (AbpSession.ImpersonatorTenantId.HasValue ? AbpSession.ImpersonatorTenantId.Value.ToString() : "null") + ";");
            script.AppendLine("    abp.session.multiTenancySide = " + ((int)AbpSession.MultiTenancySide) + ";");

            if (AbpSession is AspNetZeroAbpSession session)
            {
                var officeId = session.OfficeId?.ToString() ?? "null";
                script.AppendLine("    abp.session.officeId = " + officeId + ";");
                var officeName = session.OfficeName ?? "null";
                script.AppendLine("    abp.session.officeName = " + HtmlHelper.EscapeJsString(officeName) + ";");
                var officeCopyChargeTo = session.OfficeCopyChargeTo;
                script.AppendLine("    abp.session.officeCopyChargeTo = " + (officeCopyChargeTo ? "true" : "false") + ";");
            }

            script.AppendLine();
            script.Append("})();");

            return script.ToString();
        }
    }
}
