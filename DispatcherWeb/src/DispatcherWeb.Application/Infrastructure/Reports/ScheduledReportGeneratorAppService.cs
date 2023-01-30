using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.Threading;
using Abp.Timing.Timezone;
using DispatcherWeb.Application.Infrastructure.Utilities;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Reports.Dto;
using DispatcherWeb.Infrastructure.Utilities;
using DispatcherWeb.Trucks.OutOfServiceTrucksReport;

namespace DispatcherWeb.Infrastructure.Reports
{
	[RemoteService(false)]
	public class ScheduledReportGeneratorAppService : DispatcherWebAppServiceBase, IScheduledReportGeneratorAppService
	{
		private readonly IocManager _iocManager;
		private readonly IEmailSender _emailSender;
		private readonly IRepository<UserPermissionSetting, long> _userPermissionRepository;
		private readonly IRepository<RolePermissionSetting, long> _rolePermissionRepository;
		private readonly IRepository<UserRole, long> _userRoleRepository;
		private CustomSession _customSession;

		public ScheduledReportGeneratorAppService(
			IocManager iocManager,
			IEmailSender emailSender,
			IRepository<UserPermissionSetting, long> userPermissionRepository,
			IRepository<RolePermissionSetting, long> rolePermissionRepository,
			IRepository<UserRole, long> userRoleRepository
		)
		{
			_iocManager = iocManager;
			_emailSender = emailSender;
			_userPermissionRepository = userPermissionRepository;
			_rolePermissionRepository = rolePermissionRepository;
			_userRoleRepository = userRoleRepository;
		}

		[UnitOfWork]
		public virtual async Task GenerateReport(ScheduledReportGeneratorInput scheduledReportGeneratorInput)
		{
            Logger.Info($"Start generating scheduled report {scheduledReportGeneratorInput.ReportType}");
			_customSession = scheduledReportGeneratorInput.CustomSession;
            using(AbpSession.Use(_customSession.TenantId, _customSession.UserId))
            using (CurrentUnitOfWork.SetTenantId(_customSession.TenantId))
            {
                if(!PermissionChecker.IsGranted(AbpSession.ToUserIdentifier(), scheduledReportGeneratorInput.ReportType.GetPermissionName()))
                {
                    Logger.Error($"The user don't have the {scheduledReportGeneratorInput.ReportType.GetPermissionName()} permission for the {scheduledReportGeneratorInput.ReportType} report.");
                    return;
                }

                ReportAppServiceBase<EmptyInput> reportAppService = null;
                try
                {
                    reportAppService = GetReportService(scheduledReportGeneratorInput.ReportType);
                    EmptyInput input = new EmptyInput();
                    FileDto file;
                    if(scheduledReportGeneratorInput.ReportFormat == ReportFormat.Pdf)
                    {
                        file = await reportAppService.CreatePdf(input);
                    }
                    else
                    {
                        file = await reportAppService.CreateCsv(input);
                    }
                    SendEmails(scheduledReportGeneratorInput.EmailAddresses, file, scheduledReportGeneratorInput.ReportType);

                }
                catch(Exception e)
                {
                    Logger.Error($"Error when generating report: {e}\n input: {scheduledReportGeneratorInput}");
                }
                finally
                {
                    _iocManager.Release(reportAppService);
                }
            }

        }

		private void SendEmails(string[] emailAdresses, FileDto file, ReportType reportType)
		{
			byte[] fileBytes = AttachmentHelper.GetReportFile(_customSession.UserId.ToString(), file.FileToken);
			string from = SettingManager.GetSettingValue(EmailSettingNames.DefaultFromAddress);

			using(Stream stream = new MemoryStream(fileBytes))
			{
				Attachment attachment = CreateAttachment(stream, file.FileName, file.FileType);
				foreach(var emailAdress in emailAdresses)
				{
					SendEmail(from, emailAdress, attachment, reportType);
				}
			}
		}

		private void SendEmail(string from, string to, Attachment attachment, ReportType reportType)
		{
			try
			{
				MailMessage message = new MailMessage(from, to, $"Generated report {reportType}", "");
				message.Attachments.Add(attachment);
				_emailSender.Send(message);

			}
			catch(Exception e)
			{
				Logger.Error($"Error sending email from {from} to {to}: {e}");
			}
		}

		private Attachment CreateAttachment(Stream stream, string fileName, string fileType)
		{
			return new Attachment(stream, fileName, fileType);
		}

		private ReportAppServiceBase<EmptyInput> GetReportService(ReportType reportType)
		{
			switch(reportType)
			{
				case ReportType.OutOfServiceTrucks:
					var report = _iocManager.Resolve<OutOfServiceTrucksReportAppService>();
					report.CustomSession = _customSession;
					return report;
				default:
					throw new ApplicationException($"Unsupported report type: {reportType}");
			}
		}

		private bool IsPermissionGranted(string permissionName)
		{
			var isGrantedToUser = _userPermissionRepository.GetAll()
				.Where(p => p.UserId == _customSession.UserId && p.Name == permissionName)
				.Select(p => (bool?)p.IsGranted)
				.FirstOrDefault();
			if (isGrantedToUser.HasValue)
			{
				return isGrantedToUser.Value;
			}
			var isGrantedToUserRole =
				(from ur in _userRoleRepository.GetAll()
				where ur.UserId == _customSession.UserId
				join rp in _rolePermissionRepository.GetAll().Where(p => p.Name == permissionName) on ur.Id equals rp.RoleId
				select (bool?)rp.IsGranted
				).FirstOrDefault();

			return isGrantedToUserRole.HasValue && isGrantedToUserRole.Value;
		}

	}
}
