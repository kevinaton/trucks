using System;

namespace DispatcherWeb
{
    public class DispatcherWebConsts
    {
        public const string LocalizationSourceName = "DispatcherWeb";

        public const string ConnectionStringName = "Default";

        [Obsolete(message: "Use IConfigurationRoot.IsMultitenancyEnabled() instead")]
        public const bool MultiTenancyEnabled = true;

        public const bool AllowTenantsToChangeEmailSettings = false;

        public const string Currency = "USD";

        public const string CurrencySign = "$";

        public const string AbpApiClientUserAgent = "AbpApiClient";


        public const int PaymentCacheDurationInMinutes = 30;

        public const string InternalNotesEncryptionKey = "InternalNotesKey";

        public const string PasswordHasntBeenChanged = "*•*•*•*•*•*•*•*•*•*";

        public const string DbTypeDecimal19_4 = "decimal(19, 4)";

        public const string DbTypeDecimalLocation = "decimal(12, 9)";

        public static DateTime MinDateTime = new DateTime(2000, 1, 1);

        // Note:
        // Minimum accepted payment amount. If a payment amount is less then that minimum value payment progress will continue without charging payment
        // Even though we can use multiple payment methods, users always can go and use the highest accepted payment amount.
        //For example, you use Stripe and PayPal. Let say that stripe accepts min 5$ and PayPal accepts min 3$. If your payment amount is 4$.
        // User will prefer to use a payment method with the highest accept value which is a Stripe in this case.
        public const decimal MinimumUpgradePaymentAmount = 1M;

        public static class Session
        {
            public const string IsSidebarCollapsed = "IsSidebarCollapsed";
            public const string IsHideCompletedOrdersSet = "IsHideCompletedOrdersSet";
        }

        public static class Claims
        {
            public const string UserOfficeId = "Application_UserOfficeId";
            public const string UserOfficeName = "Application_UserOfficeName";
            public const string UserOfficeCopyChargeTo = "Application_UserOfficeCopyDeliverToLoadAtChargeTo";
        }

    }
}