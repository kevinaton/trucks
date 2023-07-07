namespace DispatcherWeb.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public static string[] ManualPermissionsList = new[]
        {
            AppPermissions.DriverProductionPay,
            AppPermissions.Pages_LeaseHaulerStatements,
            AppPermissions.Pages_Imports_Customers,
            AppPermissions.Pages_Imports_Trucks,
            AppPermissions.Pages_Imports_Vendors,
            AppPermissions.Pages_Imports_Services,
            AppPermissions.Pages_Imports_Employees,
            AppPermissions.Pages_DriverApplication_ReactNativeDriverApp
        };

        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)
        public const string Pages = "Pages";

        // public const string Pages_DemoUiComponents= "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";
        public const string Pages_Administration_Languages_ChangeDefaultLanguage = "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";
        public const string Pages_Administration_OrganizationUnits_ManageRoles = "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";
        public const string Pages_Administration_WebhookSubscription_Create = "Pages.Administration.WebhookSubscription.Create";
        public const string Pages_Administration_WebhookSubscription_Edit = "Pages.Administration.WebhookSubscription.Edit";
        public const string Pages_Administration_WebhookSubscription_ChangeActivity = "Pages.Administration.WebhookSubscription.ChangeActivity";
        public const string Pages_Administration_WebhookSubscription_Detail = "Pages.Administration.WebhookSubscription.Detail";
        public const string Pages_Administration_Webhook_ListSendAttempts = "Pages.Administration.Webhook.ListSendAttempts";
        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";
        public const string Pages_Administration_DynamicProperties_Create = "Pages.Administration.DynamicProperties.Create";
        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";
        public const string Pages_Administration_DynamicProperties_Delete = "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";
        public const string Pages_Administration_DynamicPropertyValue_Create = "Pages.Administration.DynamicPropertyValue.Create";
        public const string Pages_Administration_DynamicPropertyValue_Edit = "Pages.Administration.DynamicPropertyValue.Edit";
        public const string Pages_Administration_DynamicPropertyValue_Delete = "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties = "Pages.Administration.DynamicEntityProperties";
        public const string Pages_Administration_DynamicEntityProperties_Create = "Pages.Administration.DynamicEntityProperties.Create";
        public const string Pages_Administration_DynamicEntityProperties_Edit = "Pages.Administration.DynamicEntityProperties.Edit";
        public const string Pages_Administration_DynamicEntityProperties_Delete = "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue = "Pages.Administration.DynamicEntityPropertyValue";
        public const string Pages_Administration_DynamicEntityPropertyValue_Create = "Pages.Administration.DynamicEntityPropertyValue.Create";
        public const string Pages_Administration_DynamicEntityPropertyValue_Edit = "Pages.Administration.DynamicEntityPropertyValue.Edit";
        public const string Pages_Administration_DynamicEntityPropertyValue_Delete = "Pages.Administration.DynamicEntityPropertyValue.Delete";

        //TENANT-SPECIFIC PERMISSIONS
        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";
        public const string Pages_Dashboard = "Pages.Dashboard";
        public const string Pages_Dashboard_Dispatching = "Pages.Dashboard.Dispatching";
        public const string Pages_Dashboard_DriverDotRequirements = "Pages.Dashboard.DriverDotRequirements";
        public const string Pages_Dashboard_TruckMaintenance = "Pages.Dashboard.TruckMaintenance";
        public const string Pages_Dashboard_Revenue = "Pages.Dashboard.Revenue";
        public const string Pages_Dashboard_TruckUtilization = "Pages.Dashboard.TruckUtilization";
        public const string Pages_Orders_View = "Pages.Orders.View"; //read only
        public const string Pages_Orders_Edit = "Pages.Orders.Edit"; //create, edit, delete
        public const string Pages_Schedule = "Pages.Schedule"; //full
        public const string Pages_PrintOrders = "Pages.PrintOrders";
        public const string Pages_SendOrdersToDrivers = "Pages.SendOrdersToDrivers";
        public const string Pages_DriverAssignment = "Pages.DriverAssignment";
        public const string Pages_LeaseHauler = "Pages.LeaseHauler";
        public const string Pages_LeaseHaulers_Edit = "Pages.LeaseHaulers.Edit";
        public const string Pages_LeaseHaulers_SetHaulingCompanyTenantId = "Pages.LeaseHaulers.SetHaulingCompanyTenantId";
        public const string Pages_LeaseHaulerStatements = "Pages.LeaseHaulers.CreateStatements";
        public const string Pages_LeaseHaulerRequests = "Pages.LeaseHaulerRequests";
        public const string Pages_LeaseHaulerRequests_Edit = "Pages.LeaseHaulerRequests.Edit";
        public const string Pages_Trucks = "Pages.Trucks";
        public const string Pages_OutOfServiceHistory_Delete = "Pages.OutOfServiceHistory.Delete";
        public const string Pages_Customers = "Pages.Customers";
        public const string Pages_Customers_Merge = "Pages.Customers.Merge";
        public const string Pages_Services = "Pages.Services";
        public const string Pages_Services_Merge = "Pages.Services.Merge";
        public const string Pages_Drivers = "Pages.Drivers";
        public const string Pages_Locations = "Pages.Locations";
        public const string Pages_Locations_Merge = "Pages.Locations.Merge";
        public const string Pages_Projects = "Pages.Projects";
        public const string Pages_Quotes_View = "Pages.Quotes.View"; //read only (including read of Quote items)
        public const string Pages_Quotes_Edit = "Pages.Quotes.Edit"; //create, edit, delete quotes (inluding create, edit, delete Quote Items). Includes Pages_Quotes_Items_Create
        public const string Pages_Quotes_Items_Create = "Pages.Quotes.Items.Create"; //create quote items only
        public const string Pages_CannedText = "Pages.CannedText";
        public const string Pages_Offices = "Pages.Offices";
        public const string Pages_Tickets_View = "Pages.Tickets.View"; //read only
        public const string Pages_Tickets_Edit = "Pages.Tickets.Edit"; //create, edit, delete
        public const string Pages_TicketsByDriver = "Pages.TicketsByDriver";
        public const string Pages_Invoices = "Pages.Invoices";
        public const string DriverProductionPay = "DriverProductionPay";
        public const string AllowImportingTruxEarnings = "AllowImportingTruxEarnings";
        public const string AllowImportingLuckStoneEarnings = "AllowImportingLuckStoneEarnings";
        public const string EditInvoicedOrdersAndTickets = "EditInvoicedOrdersAndTickets";

        public const string Pages_VehicleService_View = "Pages.VehicleService.View";
        public const string Pages_VehicleService_Edit = "Pages.VehicleService.Edit";
        public const string Pages_PreventiveMaintenanceSchedule_View = "Pages.PreventiveMaintenanceSchedule.View";
        public const string Pages_PreventiveMaintenanceSchedule_Edit = "Pages.PreventiveMaintenanceSchedule.Edit";
        public const string Pages_WorkOrders_View = "Pages.WorkOrders.View";
        public const string Pages_WorkOrders_Edit = "Pages.WorkOrders.Edit";
        public const string Pages_WorkOrders_EditLimited = "Pages.WorkOrders.EditLimited";
        public const string Pages_FuelPurchases_View = "Pages.FuelPurchases.View";
        public const string Pages_FuelPurchases_Edit = "Pages.FuelPurchases.Edit";
        public const string Pages_VehicleUsages_View = "Pages.VehicleUsages.View";
        public const string Pages_VehicleUsages_Edit = "Pages.VehicleUsages.Edit";

        public const string Pages_Reports = "Pages.Reports";
        public const string Pages_Reports_ScheduledReports = "Pages.Reports.ScheduledReports";
        public const string Pages_Reports_OutOfServiceTrucks = "Pages.Reports.OutOfServiceTrucks";
        public const string Pages_Reports_RevenueBreakdown = "Pages.Reports.RevenueBreakdown";
        public const string Pages_Reports_RevenueBreakdownByTruck = "Pages.Reports.RevenueBreakdownByTruck";
        public const string Pages_Reports_Receipts = "Pages.Reports.Receipts";
        public const string Pages_Reports_PaymentReconciliation = "Pages.Reports.PaymentReconciliation";
        public const string Pages_Reports_DriverActivityDetail = "Pages.Reports.DriverActivityDetail";
        public const string Pages_Reports_RevenueAnalysis = "Pages.Reports.RevenueAnalysis";

        public const string Pages_ActiveReports = "Pages.ActiveReports";
        public const string Pages_ActiveReports_TenantStatisticsReport = "Pages.ActiveReports.TenantStatisticsReport";

        public const string Pages_Imports = "Pages.Imports";
        public const string Pages_Imports_FuelUsage = "Pages.Imports.FuelUsage";
        public const string Pages_Imports_VehicleUsage = "Pages.Imports.VehicleUsage";
        public const string Pages_Imports_Customers = "Pages.Imports.Customer";
        public const string Pages_Imports_Trucks = "Pages.Imports.Trucks";
        public const string Pages_Imports_Vendors = "Pages.Imports.Vendors";
        public const string Pages_Imports_Services = "Pages.Imports.Services";
        public const string Pages_Imports_Employees = "Pages.Imports.Employees";

        public const string Pages_OfficeAccess_All = "Pages.OfficeAccess.All";
        public const string Pages_OfficeAccess_UserOnly = "Pages.OfficeAccess.UserOnly";

        public const string Pages_DriverMessages = "Pages.DriverMessages";
        public const string Pages_DriverApplication = "Pages.DriverApplication";
        public const string Pages_DriverApplication_WebBasedDriverApp = "Pages.DriverApplication.WebBasedDriverApp";
        public const string Pages_DriverApplication_ReactNativeDriverApp = "Pages.DriverApplication.ReactNativeDriverApp";
        public const string Pages_DriverApplication_Settings = "Pages.DriverApplication.Settings";
        public const string Pages_Dispatches = "Pages.Dispatches";
        public const string Pages_Dispatches_Edit = "Pages.Dispatches.Edit";
        public const string Pages_Dispatches_SendSyncRequest = "Pages.Dispatches.SendSyncRequest";

        public const string Pages_TimeEntry = "Pages.TimeEntry";
        public const string Pages_TimeEntry_EditAll = "Pages.TimeEntry.EditAll";
        public const string Pages_TimeEntry_EditPersonal = "Pages.TimeEntry.EditPersonal";
        public const string Pages_TimeEntry_ViewOnly = "Pages.TimeEntry.ViewOnly";
        public const string Pages_TimeEntry_EditTimeClassifications = "Pages.TimeEntry.EditTimeClassifications";

        public const string Pages_TimeOff = "Pages.TimeOff";

        public const string Pages_Backoffice_DriverPay = "Pages.Backoffice.DriverPay";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        public const string Pages_CustomerPortal = "Pages.CustomerPortal";
        public const string Pages_CustomerPortal_TicketsList = "Pages.CustomerPortal.TicketsList";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";
        public const string Pages_Tenants_AddMonthToDriver = "Pages.Tenants.AddMonthToDriver";
        public const string Pages_Tenants_AddDemoData = "Pages.Tenants.AddDemoData";
        public const string Pages_Tenants_DeleteDispatchData = "Pages.Tenants.DeleteDispatchData";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";

        public const string Pages_VehicleServiceTypes_View = "Pages.VehicleServiceTypes.View";
        public const string Pages_VehicleServiceTypes_Edit = "Pages.VehicleServiceTypes.Edit";

        public const string Pages_VehicleCategories = "Pages.VehicleCategories";

        public const string Pages_HostEmails = "Pages.HostEmails";
        public const string Pages_HostEmails_Send = "Pages.HostEmails.Send";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
    }
}
