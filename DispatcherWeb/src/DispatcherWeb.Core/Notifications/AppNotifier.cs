using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Notifications;
using Abp.RealTime;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Notifications
{
    public class AppNotifier : DispatcherWebDomainServiceBase, IAppNotifier
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IRealTimeNotifier _realTimeNotifier;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly UserManager _userManager;

        public AppNotifier(
            INotificationPublisher notificationPublisher,
            IRealTimeNotifier realTimeNotifier,
            IOnlineClientManager onlineClientManager,
            UserManager userManager

            )
        {
            _notificationPublisher = notificationPublisher;
            _realTimeNotifier = realTimeNotifier;
            _onlineClientManager = onlineClientManager;
            _userManager = userManager;
        }

        public async Task WelcomeToTheApplicationAsync(User user)
        {
            await _notificationPublisher.PublishAsync(
                AppNotificationNames.WelcomeToTheApplication,
                new MessageNotificationData(L("WelcomeToTheApplicationNotificationMessage")),
                severity: NotificationSeverity.Success,
                userIds: new[] { user.ToUserIdentifier() }
                );
        }

        public async Task NewUserRegisteredAsync(User user)
        {
            var notificationData = new LocalizableMessageNotificationData(
                new LocalizableString(
                    "NewUserRegisteredNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                )
            )
            { ["userName"] = user.UserName, ["emailAddress"] = user.EmailAddress };


            await _notificationPublisher.PublishAsync(AppNotificationNames.NewUserRegistered, notificationData, tenantIds: new[] { user.TenantId });
        }

        public async Task NewTenantRegisteredAsync(Tenant tenant)
        {
            var notificationData = new LocalizableMessageNotificationData(
                new LocalizableString(
                    "NewTenantRegisteredNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                    )
                );

            notificationData["tenancyName"] = tenant.TenancyName;
            await _notificationPublisher.PublishAsync(AppNotificationNames.NewTenantRegistered, notificationData);
        }

        public async Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId)
        {
            var notificationData = new LocalizableMessageNotificationData(
                new LocalizableString(
                    "GdprDataPreparedNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                )
            );

            notificationData["binaryObjectId"] = binaryObjectId;

            await _notificationPublisher.PublishAsync(AppNotificationNames.GdprDataPrepared, notificationData, userIds: new[] { user });
        }

        public async Task OrderSharedAsync(UserIdentifier[] users, Orders.Order order)
        {
            var notificationData = new MessageNotificationData(
                $"Order #{order.Id} has been shared with your office for {order.DeliveryDate?.ToShortDateString()}."
            )
            {
                ["orderId"] = order.Id
            };

            if (users.Length > 0)
            {
                await _notificationPublisher.PublishAsync(
                    AppNotificationNames.OrderShared,
                    notificationData,
                    severity: NotificationSeverity.Info,
                    userIds: users
                    );
            }
        }

        public async Task QuoteEmailDeliveryFailed(UserIdentifier user, Quotes.Quote quote, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for quote #{quote.Id} has failed."
            )
            {
                ["quoteId"] = quote.Id
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.QuoteEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task InvoiceEmailDeliveryFailed(UserIdentifier user, int invoiceId, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for invoice #{invoiceId} has failed."
            )
            {
                ["invoiceId"] = invoiceId
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.InvoiceEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task OrderEmailDeliveryFailed(UserIdentifier user, Orders.Order order, Emailing.TrackableEmailReceiver emailReceiver)
        {
            var notificationData = new MessageNotificationData(
                $"Email delivery to {emailReceiver.Email} for order #{order.Id} has failed."
            )
            {
                ["orderId"] = order.Id
            };

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.OrderEmailDeliveryFailed,
                notificationData,
                severity: NotificationSeverity.Error,
                userIds: new[] { user }
            );
        }

        public async Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await SendMessageAsync(new[] { user }, message, severity);
        }

        public async Task SendMessageAsync(UserIdentifier[] users, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await _notificationPublisher.PublishAsync(
                AppNotificationNames.SimpleMessage,
                new MessageNotificationData(message),
                severity: severity,
                userIds: users
            );
        }

        public Task SendMessageAsync(UserIdentifier user,
            LocalizableString localizableMessage,
            IDictionary<string, object> localizableMessageData = null,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            return SendNotificationAsync(AppNotificationNames.SimpleMessage, user, localizableMessage,
                localizableMessageData, severity);
        }

        public async Task SendNotificationAsync(SendNotificationInput input)
        {
            var users = new List<User>();

            if (input.IncludeLocalUsers)
            {
                users.AddRange(
                    await _userManager.Users
                        .WhereIf(input.OfficeIdFilter?.Any() == true, x => x.OfficeId.HasValue && input.OfficeIdFilter.Contains(x.OfficeId.Value))
                        .ToListAsync()
                );
            }

            if (input.IncludeHostUsers)
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    users.AddRange(await _userManager.Users.Where(x => x.TenantId == null).ToListAsync());
                }
            }

            if (input.RoleFilter?.Any() == true)
            {
                foreach (var user in users.ToList())
                {
                    bool isInAnyRole = false;
                    foreach (var role in input.RoleFilter)
                    {
                        if (await _userManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Dispatching))
                        {
                            isInAnyRole = true;
                            break;
                        }
                    }
                    if (!isInAnyRole)
                    {
                        users.Remove(user);
                    }
                }
            }

            if (input.OnlineFilter.HasValue)
            {
                foreach (var user in users.ToList())
                {
                    if (_onlineClientManager.IsOnline(user.ToUserIdentifier()) != input.OnlineFilter)
                    {
                        users.Remove(user);
                    }
                }
            }

            await _notificationPublisher.PublishAsync(
                input.NotificationName,
                new MessageNotificationData(input.Message),
                severity: input.Severity,
                userIds: users.Select(x => x.ToUserIdentifier()).ToArray()
                );
        }

        public async Task SendPriorityNotification(SendPriorityNotificationInput input)
        {
            var users = await _userManager.Users
                .WhereIf(input.OfficeIdFilter?.Any() == true, x => x.OfficeId.HasValue && input.OfficeIdFilter.Contains(x.OfficeId.Value))
                .ToListAsync();

            if (input.RoleFilter?.Any() == true)
            {
                foreach (var user in users.ToList())
                {
                    bool isInAnyRole = false;
                    foreach (var role in input.RoleFilter)
                    {
                        if (await _userManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Dispatching))
                        {
                            isInAnyRole = true;
                            break;
                        }
                    }
                    if (!isInAnyRole)
                    {
                        users.Remove(user);
                    }
                }
            }

            if (input.OnlineFilter.HasValue)
            {
                foreach (var user in users.ToList())
                {
                    if (_onlineClientManager.IsOnline(user.ToUserIdentifier()) != input.OnlineFilter)
                    {
                        users.Remove(user);
                    }
                }
            }

            await _notificationPublisher.PublishAsync(
                AppNotificationNames.PriorityNotification,
                new MessageNotificationData(input.Message),
                severity: input.Severity,
                userIds: users.Select(x => x.ToUserIdentifier()).ToArray()
                );
        }
        protected async Task SendNotificationAsync(string notificationName, UserIdentifier user,
            LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            var notificationData = new LocalizableMessageNotificationData(localizableMessage);
            if (localizableMessageData != null)
            {
                foreach (var pair in localizableMessageData)
                {
                    notificationData[pair.Key] = pair.Value;
                }
            }

            await _notificationPublisher.PublishAsync(notificationName, notificationData, severity: severity,
                userIds: new[] { user });
        }

        public Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName)
        {
            return SendNotificationAsync(AppNotificationNames.TenantsMovedToEdition, user,
                new LocalizableString(
                    "TenantsMovedToEditionNotificationMessage",
                    DispatcherWebConsts.LocalizationSourceName
                ),
                new Dictionary<string, object>
                {
                    {"sourceEditionName", sourceEditionName},
                    {"targetEditionName", targetEditionName}
                });
        }

        public Task<TResult> TenantsMovedToEdition<TResult>(UserIdentifier argsUser, int sourceEditionId,
            int targetEditionId)
        {
            throw new NotImplementedException();
        }

        public Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName)
        {
            return SendNotificationAsync(AppNotificationNames.DownloadInvalidImportUsers, user,
                new LocalizableString(
                    "ClickToSeeInvalidUsers",
                    DispatcherWebConsts.LocalizationSourceName
                ),
                new Dictionary<string, object>
                {
                    { "fileToken", fileToken },
                    { "fileType", fileType },
                    { "fileName", fileName }
                });
        }
    }
}
