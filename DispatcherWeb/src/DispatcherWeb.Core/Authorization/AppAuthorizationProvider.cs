using Abp.Application.Features;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;
using DispatcherWeb.Features;

namespace DispatcherWeb.Authorization
{
    /// <summary>
    /// Application's authorization provider.
    /// Defines permissions for the application.
    /// See <see cref="AppPermissions"/> for all permission names.
    /// </summary>
    public class AppAuthorizationProvider : AuthorizationProvider
    {
        private readonly bool _isMultiTenancyEnabled;

        public AppAuthorizationProvider(bool isMultiTenancyEnabled)
        {
            _isMultiTenancyEnabled = isMultiTenancyEnabled;
        }

        public AppAuthorizationProvider(IMultiTenancyConfig multiTenancyConfig)
        {
            _isMultiTenancyEnabled = multiTenancyConfig.IsEnabled;
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)
            var pages = context.GetPermissionOrNull(AppPermissions.Pages) ?? context.CreatePermission(AppPermissions.Pages, L("Pages"));


            var administration = pages.CreateChildPermission(AppPermissions.Pages_Administration, L("Administration"));


            var roles = administration.CreateChildPermission(AppPermissions.Pages_Administration_Roles, L("Roles"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Create, L("CreatingNewRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Edit, L("EditingRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Delete, L("DeletingRole"));

            var users = administration.CreateChildPermission(AppPermissions.Pages_Administration_Users, L("Users"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Create, L("CreatingNewUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Edit, L("EditingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Delete, L("DeletingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangePermissions, L("ChangingPermissions"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Impersonation, L("LoginForUsers"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Unlock, L("Unlock"));

            var languages = administration.CreateChildPermission(AppPermissions.Pages_Administration_Languages, L("Languages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Create, L("CreatingNewLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Edit, L("EditingLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Delete, L("DeletingLanguages"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeTexts, L("ChangingTexts"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeDefaultLanguage, L("ChangeDefaultLanguage"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_AuditLogs, L("AuditLogs"));

            var organizationUnits = administration.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits, L("OrganizationUnits"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree, L("ManagingOrganizationTree"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, L("ManagingMembers"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles, L("ManagingRoles"));

            //TENANT-SPECIFIC PERMISSIONS
            var dashboard = pages.CreateChildPermission(AppPermissions.Pages_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);
            dashboard.CreateChildPermission(AppPermissions.Pages_Dashboard_Dispatching, L("Dispatching"), multiTenancySides: MultiTenancySides.Tenant);
            dashboard.CreateChildPermission(AppPermissions.Pages_Dashboard_DriverDotRequirements, L("DriverDotRequirements"), multiTenancySides: MultiTenancySides.Tenant);
            dashboard.CreateChildPermission(AppPermissions.Pages_Dashboard_TruckMaintenance, L("TruckMaintenance"), multiTenancySides: MultiTenancySides.Tenant);
            dashboard.CreateChildPermission(AppPermissions.Pages_Dashboard_Revenue, L("Revenue"), multiTenancySides: MultiTenancySides.Tenant);
            dashboard.CreateChildPermission(AppPermissions.Pages_Dashboard_TruckUtilization, L("TruckUtilization"), multiTenancySides: MultiTenancySides.Tenant);
            var orders = pages.CreateChildPermission(AppPermissions.Pages_Orders_View, L("Orders"), multiTenancySides: MultiTenancySides.Tenant);
            orders.CreateChildPermission(AppPermissions.Pages_Orders_Edit, L("EditingOrder"), multiTenancySides: MultiTenancySides.Tenant);
            orders.CreateChildPermission(AppPermissions.Pages_PrintOrders, L("PrintOrders"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_Schedule, L("Schedule"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_DriverAssignment, L("DriverAssignment"), multiTenancySides: MultiTenancySides.Tenant);
            var leaseHauler = pages.CreateChildPermission(AppPermissions.Pages_LeaseHauler, L("LeaseHauler"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowLeaseHaulersFeature));
            leaseHauler.CreateChildPermission(AppPermissions.Pages_LeaseHaulers_Edit, L("LeaseHaulersEdit"), multiTenancySides: MultiTenancySides.Tenant);
            leaseHauler.CreateChildPermission(AppPermissions.Pages_LeaseHaulers_SetHaulingCompanyTenantId, L("SetTenantId"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowSendingOrdersToDifferentTenant));
            leaseHauler.CreateChildPermission(AppPermissions.Pages_LeaseHaulerStatements, L("LeaseHaulerStatements"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowLeaseHaulersFeature));
            var leaseHaulerRequests = leaseHauler.CreateChildPermission(AppPermissions.Pages_LeaseHaulerRequests, L("LeaseHaulerRequests"), multiTenancySides: MultiTenancySides.Tenant);
            leaseHaulerRequests.CreateChildPermission(AppPermissions.Pages_LeaseHaulerRequests_Edit, L("LeaseHaulerRequestsEdit"), multiTenancySides: MultiTenancySides.Tenant);

            pages.CreateChildPermission(AppPermissions.Pages_Trucks, L("Trucks"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_OutOfServiceHistory_Delete, L("OutOfServiceHistoryDelete"), multiTenancySides: MultiTenancySides.Tenant);
            var customers = pages.CreateChildPermission(AppPermissions.Pages_Customers, L("Customers"), multiTenancySides: MultiTenancySides.Tenant);
            customers.CreateChildPermission(AppPermissions.Pages_Customers_Merge, L("MergingCustomers"), multiTenancySides: MultiTenancySides.Tenant);
            var services = pages.CreateChildPermission(AppPermissions.Pages_Services, L("ProductsOrServices"), multiTenancySides: MultiTenancySides.Tenant);
            services.CreateChildPermission(AppPermissions.Pages_Services_Merge, L("MergingProductsOrServices"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_Drivers, L("Drivers"), multiTenancySides: MultiTenancySides.Tenant);
            var locations = pages.CreateChildPermission(AppPermissions.Pages_Locations, L("Locations"), multiTenancySides: MultiTenancySides.Tenant);
            locations.CreateChildPermission(AppPermissions.Pages_Locations_Merge, L("MergingLocations"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_Projects, L("Projects"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowProjects));
            pages.CreateChildPermission(AppPermissions.EditInvoicedOrdersAndTickets, L("EditInvoicedOrdersAndTickets"), multiTenancySides: MultiTenancySides.Tenant);
            var quotes = pages.CreateChildPermission(AppPermissions.Pages_Quotes_View, L("Quotes"), multiTenancySides: MultiTenancySides.Tenant);
            quotes.CreateChildPermission(AppPermissions.Pages_Quotes_Edit, L("EditingQuotes"), multiTenancySides: MultiTenancySides.Tenant);
            quotes.CreateChildPermission(AppPermissions.Pages_Quotes_Items_Create, L("AddingLineItemsToExistingQuotes"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_CannedText, L("CannedTexts"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_Offices, L("Offices"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_TicketsByDriver, L("TicketsByDriver"), multiTenancySides: MultiTenancySides.Tenant);
            var tickets = pages.CreateChildPermission(AppPermissions.Pages_Tickets_View, L("Tickets"), multiTenancySides: MultiTenancySides.Tenant);
            tickets.CreateChildPermission(AppPermissions.Pages_Tickets_Edit, L("EditingTicket"), multiTenancySides: MultiTenancySides.Tenant);
            pages.CreateChildPermission(AppPermissions.Pages_Invoices, L("Invoices"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowInvoicingFeature));
            pages.CreateChildPermission(AppPermissions.DriverProductionPay, L("DriverProductionPay"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.DriverProductionPayFeature));
            pages.CreateChildPermission(AppPermissions.AllowImportingTruxEarnings, L("AllowImportingTruxEarnings"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowImportingTruxEarnings));
            pages.CreateChildPermission(AppPermissions.AllowImportingLuckStoneEarnings, L("AllowImportingLuckStoneEarnings"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowImportingLuckStoneEarnings));

            var vehicleService = pages.CreateChildPermission(AppPermissions.Pages_VehicleService_View, L("VehicleService"), multiTenancySides: MultiTenancySides.Tenant);
            vehicleService.CreateChildPermission(AppPermissions.Pages_VehicleService_Edit, L("EditingVehicleService"), multiTenancySides: MultiTenancySides.Tenant);
            var preventiveMaintenanceSchedule = pages.CreateChildPermission(AppPermissions.Pages_PreventiveMaintenanceSchedule_View, L("PreventiveMaintenanceSchedule"), multiTenancySides: MultiTenancySides.Tenant);
            preventiveMaintenanceSchedule.CreateChildPermission(AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit, L("EditingPreventiveMaintenanceSchedule"), multiTenancySides: MultiTenancySides.Tenant);
            var workOrders = pages.CreateChildPermission(AppPermissions.Pages_WorkOrders_View, L("WorkOrders"), multiTenancySides: MultiTenancySides.Tenant);
            workOrders.CreateChildPermission(AppPermissions.Pages_WorkOrders_Edit, L("EditingWorkOrders"), multiTenancySides: MultiTenancySides.Tenant);
            workOrders.CreateChildPermission(AppPermissions.Pages_WorkOrders_EditLimited, L("EditingWorkOrdersLimited"), multiTenancySides: MultiTenancySides.Tenant);
            var fuelPurchases = pages.CreateChildPermission(AppPermissions.Pages_FuelPurchases_View, L("FuelPurchases"), multiTenancySides: MultiTenancySides.Tenant);
            fuelPurchases.CreateChildPermission(AppPermissions.Pages_FuelPurchases_Edit, L("EditingFuelPurchases"), multiTenancySides: MultiTenancySides.Tenant);

            var vehicleUsages = pages.CreateChildPermission(AppPermissions.Pages_VehicleUsages_View, L("VehicleUsages"), multiTenancySides: MultiTenancySides.Tenant);
            vehicleUsages.CreateChildPermission(AppPermissions.Pages_VehicleUsages_Edit, L("EditingVehicleUsages"), multiTenancySides: MultiTenancySides.Tenant);

            var dispatches = pages.CreateChildPermission(AppPermissions.Pages_Dispatches, L("Dispatches"), multiTenancySides: MultiTenancySides.Tenant);
            var editingDispatches = dispatches.CreateChildPermission(AppPermissions.Pages_Dispatches_Edit, L("EditingDispatches"), multiTenancySides: MultiTenancySides.Tenant);
            editingDispatches.CreateChildPermission(AppPermissions.Pages_SendOrdersToDrivers, L("SendOrdersToDrivers"), multiTenancySides: MultiTenancySides.Tenant);
            editingDispatches.CreateChildPermission(AppPermissions.Pages_Dispatches_SendSyncRequest, L("SendSyncRequest"), multiTenancySides: MultiTenancySides.Tenant);

            var timeEntry = pages.CreateChildPermission(AppPermissions.Pages_TimeEntry, L("TimeEntry"), multiTenancySides: MultiTenancySides.Tenant);
            timeEntry.CreateChildPermission(AppPermissions.Pages_TimeEntry_EditAll, L("EditAll"), multiTenancySides: MultiTenancySides.Tenant);
            timeEntry.CreateChildPermission(AppPermissions.Pages_TimeEntry_EditPersonal, L("EditPersonal"), multiTenancySides: MultiTenancySides.Tenant);
            timeEntry.CreateChildPermission(AppPermissions.Pages_TimeEntry_ViewOnly, L("ViewOnly"), multiTenancySides: MultiTenancySides.Tenant);
            timeEntry.CreateChildPermission(AppPermissions.Pages_TimeEntry_EditTimeClassifications, L("EditTimeClassifications"), multiTenancySides: MultiTenancySides.Tenant);

            pages.CreateChildPermission(AppPermissions.Pages_TimeOff, L("TimeOff"), multiTenancySides: MultiTenancySides.Tenant);

            pages.CreateChildPermission(AppPermissions.Pages_Backoffice_DriverPay, L("DriverPay"), multiTenancySides: MultiTenancySides.Tenant);

            var driverApp = pages.CreateChildPermission(AppPermissions.Pages_DriverApplication, L("DriverApplication"), multiTenancySides: MultiTenancySides.Tenant);
            driverApp.CreateChildPermission(AppPermissions.Pages_DriverApplication_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);

            var reports = pages.CreateChildPermission(AppPermissions.Pages_Reports, L("Reports"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_ScheduledReports, L("ScheduledReports"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_OutOfServiceTrucks, L("OutOfServiceTrucksReport"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_RevenueBreakdown, L("RevenueBreakdownReport"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_RevenueBreakdownByTruck, L("RevenueBreakdownByTruckReport"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_Receipts, L("Receipts"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_PaymentReconciliation, L("PaymentReconciliation"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.AllowPaymentProcessingFeature));
            reports.CreateChildPermission(AppPermissions.Pages_Reports_DriverActivityDetail, L("DriverActivityReport"), multiTenancySides: MultiTenancySides.Tenant);
            reports.CreateChildPermission(AppPermissions.Pages_Reports_RevenueAnalysis, L("RevenueAnalysis"), multiTenancySides: MultiTenancySides.Tenant);

            var activeReports = pages.CreateChildPermission(AppPermissions.Pages_ActiveReports, L("ActiveReports"), multiTenancySides: MultiTenancySides.Host | MultiTenancySides.Tenant);
            activeReports.CreateChildPermission(AppPermissions.Pages_ActiveReports_TenantStatisticsReport, L("TenantStatisticsReport"), multiTenancySides: MultiTenancySides.Host);
            activeReports.CreateChildPermission(AppPermissions.Pages_ActiveReports_VehicleMaintenanceWorkOrderReport, L("VehicleMaintenanceWorkOrderReport"), multiTenancySides: MultiTenancySides.Tenant);

            var imports = pages.CreateChildPermission(AppPermissions.Pages_Imports, L("Imports"), multiTenancySides: MultiTenancySides.Tenant);
            imports.CreateChildPermission(AppPermissions.Pages_Imports_FuelUsage, L("ImportFuelUsagePermission"), multiTenancySides: MultiTenancySides.Tenant);
            imports.CreateChildPermission(AppPermissions.Pages_Imports_VehicleUsage, L("ImportVehicleUsagePermission"), multiTenancySides: MultiTenancySides.Tenant);
            imports.CreateChildPermission(AppPermissions.Pages_Imports_Customers, L("ImportCustomersPermission"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.QuickbooksImportFeature));
            imports.CreateChildPermission(AppPermissions.Pages_Imports_Trucks, L("ImportTrucksPermission"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.QuickbooksImportFeature));
            imports.CreateChildPermission(AppPermissions.Pages_Imports_Vendors, L("ImportVendorsPermission"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.QuickbooksImportFeature));
            imports.CreateChildPermission(AppPermissions.Pages_Imports_Services, L("ImportServicesPermission"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.QuickbooksImportFeature));
            imports.CreateChildPermission(AppPermissions.Pages_Imports_Employees, L("ImportEmployeesPermission"), multiTenancySides: MultiTenancySides.Tenant,
                featureDependency: new SimpleFeatureDependency(AppFeatures.QuickbooksImportFeature));

            var officeAccess = pages.CreateChildPermission(AppPermissions.Pages_OfficeAccess_UserOnly, L("OfficeAccessUserOnly"), multiTenancySides: MultiTenancySides.Tenant);
            officeAccess.CreateChildPermission(AppPermissions.Pages_OfficeAccess_All, L("OfficeAccessAll"), multiTenancySides: MultiTenancySides.Tenant);

            pages.CreateChildPermission(AppPermissions.Pages_DriverMessages, L("DriverMessagesPermission"), multiTenancySides: MultiTenancySides.Tenant);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

            //HOST-SPECIFIC PERMISSIONS

            var editions = pages.CreateChildPermission(AppPermissions.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Create, L("CreatingNewEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Edit, L("EditingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Delete, L("DeletingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_MoveTenantsToAnotherEdition, L("MoveTenantsToAnotherEdition"), multiTenancySides: MultiTenancySides.Host);

            var tenants = pages.CreateChildPermission(AppPermissions.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Create, L("CreatingNewTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Edit, L("EditingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Delete, L("DeletingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_AddMonthToDriver, L("AddMonthToDriverDOTRequirements"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_AddDemoData, L("AddDemoData"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_DeleteDispatchData, L("DeleteDispatchData"), multiTenancySides: MultiTenancySides.Host);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Maintenance, L("Maintenance"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_HangfireDashboard, L("HangfireDashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);

            var vehicleServiceTypes = pages.CreateChildPermission(AppPermissions.Pages_VehicleServiceTypes_View, L("VehicleServiceTypes"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            vehicleServiceTypes.CreateChildPermission(AppPermissions.Pages_VehicleServiceTypes_Edit, L("EditingVehicleServiceTypes"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);

            var hostEmails = pages.CreateChildPermission(AppPermissions.Pages_HostEmails, L("HostEmails"), multiTenancySides: MultiTenancySides.Host);
            hostEmails.CreateChildPermission(AppPermissions.Pages_HostEmails_Send, L("SendHostEmails"), multiTenancySides: MultiTenancySides.Host);

            pages.CreateChildPermission(AppPermissions.Pages_VehicleCategories, L("VehicleCategories"), multiTenancySides: MultiTenancySides.Host);

            pages.CreateChildPermission(AppPermissions.Pages_DemoUiComponents, L("DemoUiComponents"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DispatcherWebConsts.LocalizationSourceName);
        }
    }


}

