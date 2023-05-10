using Abp.Application.Features;
using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Web.Models.Layout;

namespace DispatcherWeb.Web.Areas.App.Startup
{
    public class AppNavigationProvider : NavigationProvider
    {
        public const string MenuName = "App";

        public override void SetNavigation(INavigationProviderContext context)
        {
            var menu = context.Manager.Menus[MenuName] =
                new MenuDefinition(MenuName, new FixedLocalizableString("Main Menu"));

            menu
                .AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Dashboard,
                        L("Dashboard"),
                        customData: new MenuItemCustomData { Id = "HostDashboardNavbarItem" },
                        url: "App/HostDashboard",
                        icon: "flaticon-line-graph",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Dashboard)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Tenants,
                        L("Tenants"),
                        customData: new MenuItemCustomData { Id = "TenantsNavbarItem" },
                        url: "App/Tenants",
                        icon: "flaticon-list-3",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tenants)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.Editions,
                        L("Editions"),
                        customData: new MenuItemCustomData { Id = "EditionsNavbarItem" },
                        url: "App/Editions",
                        icon: "flaticon-app",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Editions)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.VehicleCategories,
                        L("VehicleCategories"),
                        customData: new MenuItemCustomData { Id = "VehicleCategoriesNavbarItem" },
                        url: "App/VehicleCategories",
                        icon: "flaticon-truck",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleCategories)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Host.HostEmails,
                        L("HostEmails"),
                        customData: new MenuItemCustomData { Id = "HostEmailsNavbarItem" },
                        url: "App/HostEmails",
                        icon: "flaticon-email",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_HostEmails)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Dashboard,
                        L("Dashboard"),
                        customData: new MenuItemCustomData { Id = "TenantDashboardNavbarItem" },
                        url: "App/Dashboard",
                        icon: "flaticon-line-graph",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dashboard),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Customers,
                        L("Customers"),
                        customData: new MenuItemCustomData { Id = "CustomersNavbarItem" },
                        url: "app/customers",
                        icon: "fas fa-user-tie",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Customers),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Services,
                        L("ProductsOrServices"),
                        customData: new MenuItemCustomData { Id = "ProductsOrServicesNavbarItem" },
                        url: "app/services",
                        icon: "fa fa-cubes",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Services),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Drivers,
                        L("Drivers"),
                        customData: new MenuItemCustomData { Id = "DriversNavbarItem" },
                        url: "app/drivers",
                        icon: "fa fa-address-card",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Drivers),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Locations,
                        L("Locations"),
                        customData: new MenuItemCustomData { Id = "LocationsNavbarItem" },
                        url: "app/locations",
                        icon: "fa fa-map-marker-alt",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Locations),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Quotes,
                        L("Quotes"),
                        customData: new MenuItemCustomData { Id = "QuotesNavbarItem" },
                        url: "app/quotes",
                        icon: "fa fa-file",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Quotes_View)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.ViewOrders,
                        L("Orders"),
                        customData: new MenuItemCustomData { Id = "ViewOrdersNavbarItem" },
                        url: "app/orders",
                        icon: "fa fa-search",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Orders_View)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.ProjectsAndQuotes,
                        L("Sales"),
                        customData: new MenuItemCustomData { Id = "SalesNavbarItem" },
                        icon: "fa fa-tasks",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.AddProject,
                            L("AddProject"),
                            customData: new MenuItemCustomData { Id = "AddProjectNavbarItem" },
                            url: "app/projects/details",
                            icon: "fa fa-plus",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Projects)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Projects,
                            L("Projects"),
                            customData: new MenuItemCustomData { Id = "ProjectsNavbarItem" },
                            url: "app/projects",
                            icon: "fa fa-search",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Projects)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Orders,
                        L("Dispatching"),
                        customData: new MenuItemCustomData { Id = "DispatchingNavbarItem" },
                        icon: "fa fa-calendar",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Schedule,
                            L("Schedule"),
                            customData: new MenuItemCustomData { Id = "ScheduleNavbarItem" },
                            url: "app/scheduling",
                            icon: "fa fa-calendar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Schedule)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.DriverMessages,
                            L("DriverMessagesMenuItem"),
                            customData: new MenuItemCustomData { Id = "DriverMessagesNavbarItem" },
                            url: "app/DriverMessages",
                            icon: "fa fa-comments",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_DriverMessages)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.TruckDispatchList,
                            L("TruckDispatchList"),
                            customData: new MenuItemCustomData { Id = "TruckDispatchListNavbarItem" },
                            url: "app/Dispatches/TruckDispatchList",
                            icon: "fa fa-calendar-check",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dispatches),
                            featureDependency: new DispatchSettingFeatureDependency()
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.DriverAssignment,
                            L("DriverAssignment"),
                            customData: new MenuItemCustomData { Id = "DriverAssignmentNavbarItem" },
                            url: "app/driverassignments",
                            icon: "fa fa-calendar-check",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_DriverAssignment)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.TicketsAndLoads,
                        L("TicketsAndLoads"),
                        customData: new MenuItemCustomData { Id = "TicketsAndLoadsNavbarItem" },
                        icon: "fa fa-ticket-alt",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Dispatches,
                            L("LoadHistory"),
                            customData: new MenuItemCustomData { Id = "LoadHistoryNavbarItem" },
                            url: "app/Dispatches",
                            icon: "fa fa-calendar-check",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Dispatches),
                            featureDependency: new DispatchSettingFeatureDependency()
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Tickets,
                            L("Tickets"),
                            customData: new MenuItemCustomData { Id = "TicketsNavbarItem" },
                            url: "app/tickets",
                            icon: "fa fa-ticket-alt",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Tickets_View)
                        )
                     ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.TicketsByDriver,
                            L("TicketsByDriver"),
                            customData: new MenuItemCustomData { Id = "TicketsByDriverNavbarItem" },
                            url: "app/tickets/TicketsByDriver",
                            icon: "fa fa-ticket-alt",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TicketsByDriver)
                        )
                     )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Invoices,
                        L("Invoices"),
                        customData: new MenuItemCustomData { Id = "InvoicesNavbarItem" },
                        url: "app/invoices",
                        icon: "fa fa-file-invoice",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Invoices),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.AllowInvoicingFeature)
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.LeaseHauler,
                        L("LeaseHauler"),
                        customData: new MenuItemCustomData { Id = "LeaseHaulerNavbarItem" },
                        icon: "fa fa-truck",
                        featureDependency: new SimpleFeatureDependency(true, AppFeatures.AllowLeaseHaulersFeature)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulers,
                            L("LeaseHaulers"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulersNavbarItem" },
                            url: "app/leasehaulers",
                            icon: "fa fa-truck", //icon: "fa fa-truck fa-truck-usd",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulers_Edit)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulerRequests,
                            L("LeaseHaulerRequests"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulerRequestsNavbarItem" },
                            url: "app/leasehaulerrequests",
                            icon: "fa fa-truck",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulerRequests)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.LeaseHaulerStatements,
                            L("LeaseHaulerStatements"),
                            customData: new MenuItemCustomData { Id = "LeaseHaulerStatementsNavbarItem" },
                            url: "app/leasehaulerstatements",
                            icon: "fa fa-file-invoice-dollar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_LeaseHaulerStatements),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.AllowLeaseHaulersFeature)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Time,
                        L("Time"),
                        customData: new MenuItemCustomData { Id = "TimeNavbarItem" },
                        icon: "fa fa-clock",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeEntry,
                            L("TimeEntry"),
                            customData: new MenuItemCustomData { Id = "TimeEntryNavbarItem" },
                            url: "app/employeetime",
                            icon: "fa fa-stopwatch",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeEntry),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                     ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeOff,
                            L("TimeOff"),
                            customData: new MenuItemCustomData { Id = "TimeOffNavbarItem" },
                            url: "app/timeoff",
                            icon: "fa fa-user-clock",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeOff),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.TimeClassifications,
                            L("TimeClassifications"),
                            customData: new MenuItemCustomData { Id = "TimeClassificationsNavbarItem" },
                            url: "app/TimeClassifications",
                            icon: "fa fa-industry",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_TimeEntry_EditTimeClassifications),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.DriverPayStatements,
                        L("DriverPay"),
                        customData: new MenuItemCustomData { Id = "DriverPayNavbarItem" },
                        url: "app/driverpaystatements",
                        icon: "fa fa-file-invoice-dollar",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Backoffice_DriverPay),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                ).AddItem(
                    new MenuItemDefinition(
                        AppPageNames.Tenant.Trucks,
                        L("Trucks"),
                        customData: new MenuItemCustomData { Id = "TruckNavbarItem" },
                        icon: "fa fa-truck",
                        permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Trucks),
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.Trucks,
                            L("Trucks"),
                            customData: new MenuItemCustomData { Id = "TrucksNavbarItem" },
                            url: "app/trucks",
                            icon: "fa fa-truck",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Trucks),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Host.ServiceType,
                            L("ServiceType"),
                            customData: new MenuItemCustomData { Id = "VehicleServiceTypeNavbarItem" },
                            url: "app/vehicleservicetypes",
                            icon: "fa fa-list",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleServiceTypes_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.VehicleServices,
                            L("VehicleMaintenanceServices"),
                            customData: new MenuItemCustomData { Id = "VehicleMaintenanceServicesNavbarItem" },
                            url: "app/vehicleservices",
                            icon: "fa fa-list-alt",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleService_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.PreventiveMaintenanceSchedule,
                            L("PreventiveMaintenanceSchedule"),
                            customData: new MenuItemCustomData { Id = "PreventiveMaintenanceScheduleNavbarItem" },
                            url: "app/preventivemaintenanceschedule",
                            icon: "fa fa-calendar",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_PreventiveMaintenanceSchedule_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                     ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.WorkOrders,
                            L("WorkOrders"),
                            customData: new MenuItemCustomData { Id = "WorkOrdersNavbarItem" },
                            url: "app/workorders",
                            icon: "fa fa-wrench",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_WorkOrders_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                     ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.FuelPurchases,
                            L("FuelPurchases"),
                            customData: new MenuItemCustomData { Id = "FuelPurchasesNavbarItem" },
                            url: "app/fuel",
                            icon: "fas fa-gas-pump",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_FuelPurchases_View),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                     ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.OutOfSeriveTrucks,
                            L("OutOfServiceTrucksReport"),
                            customData: new MenuItemCustomData { Id = "OutOfServiceTrucksReportNavbarItem" },
                            url: "app/OutOfServiceTrucksReport",
                            icon: "fa fa-truck",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_OutOfServiceTrucks),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(
                        new MenuItemDefinition(
                            AppPageNames.Tenant.VehicleUsages,
                            L("VehicleUsageMenuItem"),
                            customData: new MenuItemCustomData { Id = "VehicleUsageNavbarItem" },
                            url: "app/vehicleusages",
                            icon: "fas fa-truck-moving",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_VehicleUsages_View)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.Reports,
                        L("Reports"),
                        customData: new MenuItemCustomData { Id = "ReportsNavbarItem" },
                        icon: "fa fa-print",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.PrintOrders,
                            L("DeliveryReport"),
                            customData: new MenuItemCustomData { Id = "PrintOrdersWithDeliveryInfoNavbarItem" },
                            url: "#",
                            icon: "fa fa-calendar-check",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_PrintOrders)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueBreakdown,
                            L("RevenueBreakdownReport"),
                            customData: new MenuItemCustomData { Id = "RevenueBreakdownReportNavbarItem" },
                            url: "app/RevenueBreakdownReport",
                            icon: "fa fa-money-bill",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueBreakdown)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueBreakdownByTruck,
                            L("RevenueBreakdownByTruckReport"),
                            customData: new MenuItemCustomData { Id = "RevenueBreakdownByTruckReportNavbarItem" },
                            url: "app/RevenueBreakdownByTruckReport",
                            icon: "fa fa-money-bill",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueBreakdownByTruck)
                        )
                     ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.RevenueAnalysis,
                            L("RevenueAnalysis"),
                            customData: new MenuItemCustomData { Id = "RevenueAnalysisReportNavbarItem" },
                            url: "app/RevenueAnalysisReport",
                            icon: "fa fa-money-bill",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_RevenueAnalysis)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.ScheduledReports,
                            L("ScheduledReports"),
                            customData: new MenuItemCustomData { Id = "ScheduledReportsNavbarItem" },
                            url: "app/ScheduledReports",
                            icon: "fa fa-list",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_ScheduledReports)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Receipts,
                            L("Receipts"),
                            customData: new MenuItemCustomData { Id = "ReceiptsNavbarItem" },
                            url: "app/orders/receipts",
                            icon: "fas fa-receipt",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_Receipts)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.PaymentReconciliation,
                            L("PaymentReconciliation"),
                            customData: new MenuItemCustomData { Id = "PrintPaymentReconciliationReportNavbarItem" },
                            url: "#",
                            icon: "fa fa-print",
                            featureDependency: new SimpleFeatureDependency(AppFeatures.AllowPaymentProcessingFeature),
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_PaymentReconciliation)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.DriverActivityDetailReport,
                            L("DriverActivityReport"),
                            customData: new MenuItemCustomData { Id = "DriverActivityReportNavbarItem" },
                            url: "app/driveractivitydetailreport",
                            icon: "fa fa-address-card",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Reports_DriverActivityDetail)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Tenant.Imports,
                        L("Imports"),
                        customData: new MenuItemCustomData { Id = "ImportsNavbarItem" },
                        icon: "fas fa-file-import",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                    )
                    .AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.ImportFuelUsage,
                            L("ImportFuelMenuItem"),
                            customData: new MenuItemCustomData { Id = "ImportFuelNavbarItem" },
                            url: "app/ImportFuel",
                            icon: "fas fa-gas-pump",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Imports_FuelUsage)
                        )
                    )
                ).AddItem(new MenuItemDefinition(
                        AppPageNames.Common.Administration,
                        L("Administration"),
                        customData: new MenuItemCustomData { Id = "AdministrationNavbarItem" },
                        icon: "fa fa-wrench",
                        featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Offices,
                            L("Offices"),
                            customData: new MenuItemCustomData { Id = "OfficesNavbarItem" },
                            url: "app/offices",
                            icon: "fa fa-building",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Offices),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                     ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.CannedTexts,
                            L("CannedTexts"),
                            customData: new MenuItemCustomData { Id = "CannedTextsNavbarItem" },
                            url: "app/cannedtexts",
                            icon: "fa fa-align-left",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_CannedText),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.OrganizationUnits,
                            L("OrganizationUnits"),
                            customData: new MenuItemCustomData { Id = "OrganizationUnitsNavbarItem" },
                            url: "App/OrganizationUnits",
                            icon: "flaticon-map",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_OrganizationUnits),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Roles,
                            L("Roles"),
                            customData: new MenuItemCustomData { Id = "RolesNavbarItem" },
                            url: "App/Roles",
                            icon: "flaticon-suitcase",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Roles),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Users,
                            L("Users"),
                            customData: new MenuItemCustomData { Id = "UsersNavbarItem" },
                            url: "App/Users",
                            icon: "flaticon-users",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Users),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.Languages,
                            L("Languages"),
                            customData: new MenuItemCustomData { Id = "LanguagesNavbarItem" },
                            url: "App/Languages",
                            icon: "flaticon-tabs",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Languages),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Common.AuditLogs,
                            L("AuditLogs"),
                            customData: new MenuItemCustomData { Id = "AuditLogsNavbarItem" },
                            url: "App/AuditLogs",
                            icon: "flaticon-folder-1",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_AuditLogs),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Host.Maintenance,
                            L("Maintenance"),
                            customData: new MenuItemCustomData { Id = "MaintenanceNavbarItem" },
                            url: "App/Maintenance",
                            icon: "flaticon-lock",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Maintenance)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.SubscriptionManagement,
                            L("Subscription"),
                            customData: new MenuItemCustomData { Id = "SubscriptionManagementNavbarItem" },
                            url: "App/SubscriptionManagement",
                            icon: "flaticon-refresh",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.PaidFunctionality)
                        )
                    ).AddItem(new MenuItemDefinition(
                            AppPageNames.Host.Settings,
                            L("Settings"),
                            customData: new MenuItemCustomData { Id = "HostSettingsNavbarItem" },
                            url: "App/HostSettings",
                            icon: "flaticon-settings",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Host_Settings)
                        )
                    )
                    .AddItem(new MenuItemDefinition(
                            AppPageNames.Tenant.Settings,
                            L("Settings"),
                            customData: new MenuItemCustomData { Id = "TenantSettingsNavbarItem" },
                            url: "App/Settings",
                            icon: "flaticon-settings",
                            permissionDependency: new SimplePermissionDependency(AppPermissions.Pages_Administration_Tenant_Settings),
                            featureDependency: new SimpleFeatureDependency(AppFeatures.FreeFunctionality)
                        )
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, DispatcherWebConsts.LocalizationSourceName);
        }
    }
}