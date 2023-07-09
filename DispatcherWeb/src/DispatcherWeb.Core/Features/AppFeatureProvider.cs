using Abp.Application.Features;
using Abp.Localization;
using Abp.Runtime.Validation;
using Abp.UI.Inputs;

namespace DispatcherWeb.Features
{
    public class AppFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            context.Create(
                AppFeatures.MaxUserCount,
                defaultValue: "0", //0 = unlimited
                displayName: L("MaximumUserCount"),
                description: L("MaximumUserCount_Description"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, int.MaxValue))
            )[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            {
                ValueTextNormalizer = value => value == "0" ? L("Unlimited") : new FixedLocalizableString(value),
                IsVisibleOnPricingTable = true
            };

            #region ######## Example Features - You can delete them #########

            //context.Create("TestTenantScopeFeature", "false", L("TestTenantScopeFeature"), scope: FeatureScopes.Tenant);
            //context.Create("TestEditionScopeFeature", "false", L("TestEditionScopeFeature"), scope: FeatureScopes.Edition);

            //context.Create(
            //    AppFeatures.TestCheckFeature,
            //    defaultValue: "false",
            //    displayName: L("TestCheckFeature"),
            //    inputType: new CheckboxInputType()
            //)[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            //{
            //    IsVisibleOnPricingTable = true,
            //    TextHtmlColor = value => value == "true" ? "#5cb85c" : "#d9534f"
            //};

            //context.Create(
            //    AppFeatures.TestCheckFeature2,
            //    defaultValue: "true",
            //    displayName: L("TestCheckFeature2"),
            //    inputType: new CheckboxInputType()
            //)[FeatureMetadata.CustomFeatureKey] = new FeatureMetadata
            //{
            //    IsVisibleOnPricingTable = true,
            //    TextHtmlColor = value => value == "true" ? "#5cb85c" : "#d9534f"
            //};

            #endregion

            var chatFeature = context.Create(
                AppFeatures.ChatFeature,
                defaultValue: "false",
                displayName: L("ChatFeature"),
                inputType: new CheckboxInputType()
            );

            chatFeature.CreateChildFeature(
                AppFeatures.TenantToTenantChatFeature,
                defaultValue: "false",
                displayName: L("TenantToTenantChatFeature"),
                inputType: new CheckboxInputType()
            );

            chatFeature.CreateChildFeature(
                AppFeatures.TenantToHostChatFeature,
                defaultValue: "false",
                displayName: L("TenantToHostChatFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowMultiOfficeFeature,
                defaultValue: "true",
                displayName: L("AllowMultiOfficeFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.NumberOfTrucksFeature,
                defaultValue: "100",
                displayName: L("NumberOfTrucksFeature"),
                inputType: new SingleLineStringInputType(new NumericValueValidator(0, 1000000))
            );

            context.Create(
                AppFeatures.AllowSharedOrdersFeature,
                defaultValue: "true",
                displayName: L("AllowSharedOrdersFeature"),
                inputType: new CheckboxInputType()
            );
            context.Create(
                AppFeatures.AllowPaymentProcessingFeature,
                defaultValue: "false",
                displayName: L("AllowPaymentProcessingFeature"),
                inputType: new CheckboxInputType(),
                scope: 0 //display for neither Editions nor individual Tenants
            );

            context.Create(
                AppFeatures.AllowLeaseHaulersFeature,
                defaultValue: "false",
                displayName: L("AllowLeaseHaulersFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowInvoicingFeature,
                defaultValue: "false",
                displayName: L("AllowInvoicingFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.DriverProductionPayFeature,
                defaultValue: "false",
                displayName: L("DriverProductionPayFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowProjects,
                defaultValue: "false",
                displayName: L("AllowProjects"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.GpsIntegrationFeature,
                defaultValue: "false",
                displayName: L("GpsIntegrationFeature"),
                inputType: new CheckboxInputType()
            );
            context.Create(
                AppFeatures.SmsIntegrationFeature,
                defaultValue: "false",
                displayName: L("SmsIntegrationFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.DispatchingFeature,
                defaultValue: "false",
                displayName: L("DispatchingFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.QuickbooksFeature,
                defaultValue: "false",
                displayName: L("QuickbooksFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.QuickbooksImportFeature,
                defaultValue: "false",
                displayName: L("QuickbooksImportFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowCopyingZeroQuantityOrderLineItemsFeature,
                defaultValue: "false",
                displayName: L("AllowCopyingZeroQuantityOrderLineItemsFeature"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowImportingTruxEarnings,
                defaultValue: "false",
                displayName: L("AllowImportingTruxEarnings"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowImportingLuckStoneEarnings,
                defaultValue: "false",
                displayName: L("AllowImportingLuckStoneEarnings"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.AllowSendingOrdersToDifferentTenant,
                defaultValue: "false",
                displayName: L("AllowSendingOrdersToDifferentTenant"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.FreeFunctionality,
                defaultValue: "true",
                displayName: L("FreeFunctionality"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.PaidFunctionality,
                defaultValue: "true",
                displayName: L("PaidFunctionality"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.WebBasedDriverApp,
                defaultValue: "false",
                displayName: L("WebBasedDriverApp"),
                inputType: new CheckboxInputType()
            );

            context.Create(
                AppFeatures.ReactNativeDriverApp,
                defaultValue: "false",
                displayName: L("ReactNativeDriverApp"),
                inputType: new CheckboxInputType()
            );
            
            context.Create(
                AppFeatures.CustomerPortal,
                defaultValue: "false",
                displayName: L("CustomerPortal"),
                inputType: new CheckboxInputType()
            );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DispatcherWebConsts.LocalizationSourceName);
        }
    }
}
