using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DispatcherWeb.Authorization.Roles
{
    public static class DefaultRolePermissions
    {
        private static readonly Dictionary<string, string[]> _defaultRolePermissions = new Dictionary<string, string[]>()
            {
                // Pages
                {
                    AppPermissions.Pages, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.LeaseHaulerDriver,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Quoting,
                    }
                },
                // Pages_Administration
                {
                    AppPermissions.Pages_Administration, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_AuditLogs
                {
                    AppPermissions.Pages_Administration_AuditLogs, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                // Pages_Administration_Languages
                {
                    AppPermissions.Pages_Administration_Languages, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },
                // Pages_Administration_OrganizationUnits
                {
                    AppPermissions.Pages_Administration_OrganizationUnits, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Pages_Administration_Roles
                {
                    AppPermissions.Pages_Administration_Roles, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Roles_Create
                {
                    AppPermissions.Pages_Administration_Roles_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Roles_Delete
                {
                    AppPermissions.Pages_Administration_Roles_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Roles_Edit
                {
                    AppPermissions.Pages_Administration_Roles_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Pages_Administration_Tenant_Settings
                {
                    AppPermissions.Pages_Administration_Tenant_Settings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Pages_Administration_Users
                {
                    AppPermissions.Pages_Administration_Users, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Users_ChangePermissions
                {
                    AppPermissions.Pages_Administration_Users_ChangePermissions, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Users_Create
                {
                    AppPermissions.Pages_Administration_Users_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Users_Delete
                {
                    AppPermissions.Pages_Administration_Users_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Users_Edit
                {
                    AppPermissions.Pages_Administration_Users_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Pages_Administration_Users_Impersonation
                {
                    AppPermissions.Pages_Administration_Users_Impersonation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // CannedText
                {
                    AppPermissions.Pages_CannedText, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Customers
                {
                    AppPermissions.Pages_Customers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Customers_Merge
                {
                    AppPermissions.Pages_Customers_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Dashboard
                {
                    AppPermissions.Pages_Dashboard, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // Dashboard RevenueGraph
                {
                    AppPermissions.Pages_Dashboard_Revenue, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Dispatches
                {
                    AppPermissions.Pages_Dispatches, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers
                    }
                },

                // Dispatches_Edit
                {
                    AppPermissions.Pages_Dispatches_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers
                    }
                },

                //Dispatches_SendSyncRequest
                {
                    AppPermissions.Pages_Dispatches_SendSyncRequest, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // DriverApplication
                {
                    AppPermissions.Pages_DriverApplication, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Driver,
                        StaticRoleNames.Tenants.LeaseHaulerDriver
                    }
                },

                // DriverApplication_Settings
                {
                    AppPermissions.Pages_DriverApplication_Settings, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                
                // DriverAssignment
                {
                    AppPermissions.Pages_DriverAssignment, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Backoffice_DriverPay
                {
                    AppPermissions.Pages_Backoffice_DriverPay, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative
                    }
                },
                // Drivers
                {
                    AppPermissions.Pages_Drivers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // DriverMessages
                {
                    AppPermissions.Pages_DriverMessages, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Imports
                {
                    AppPermissions.Pages_Imports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // Imports_VehicleUsage
                {
                    AppPermissions.Pages_Imports_VehicleUsage, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // LeaseHauler
                {
                    AppPermissions.Pages_LeaseHauler, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // LeaseHaulers_Edit
                {
                    AppPermissions.Pages_LeaseHaulers_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Pages_LeaseHaulerRequests
                {
                    AppPermissions.Pages_LeaseHaulerRequests, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },
                // Pages_LeaseHaulerRequests_Edit
                {
                    AppPermissions.Pages_LeaseHaulerRequests_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },
                // LeaseHaulerStatements
                {
                    AppPermissions.Pages_LeaseHaulerStatements, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Offices
                {
                    AppPermissions.Pages_Offices, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // OfficeAccess All
                {
                    AppPermissions.Pages_OfficeAccess_All, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // OfficeAccess UserOnly
                {
                    AppPermissions.Pages_OfficeAccess_UserOnly, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Dispatching,
                    }
                },

                // Orders View
                {
                    AppPermissions.Pages_Orders_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Orders Edit
                {
                    AppPermissions.Pages_Orders_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // PreventiveMaintenanceSchedule_View
                {
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // PreventiveMaintenanceSchedule_Edit
                {
                    AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },

                // PrintOrders
                {
                    AppPermissions.Pages_PrintOrders, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Products/Services
                {
                    AppPermissions.Pages_Services, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Products/Services_Merge
                {
                    AppPermissions.Pages_Services_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Projects
                {
                    AppPermissions.Pages_Projects, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Quotes_View
                {
                    AppPermissions.Pages_Quotes_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Quotes_Edit
                {
                    AppPermissions.Pages_Quotes_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Quotes_Items_Create
                {
                    AppPermissions.Pages_Quotes_Items_Create, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.LimitedQuoting,
                        StaticRoleNames.Tenants.Quoting,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Reports
                {
                    AppPermissions.Pages_Reports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_DriverActivityDetail
                {
                    AppPermissions.Pages_Reports_DriverActivityDetail, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_OutOfServiceTrucks
                {
                    AppPermissions.Pages_Reports_OutOfServiceTrucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_Receipts
                {
                    AppPermissions.Pages_Reports_Receipts, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_RevenueBreakdown
                {
                    AppPermissions.Pages_Reports_RevenueBreakdown, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_RevenueBreakdownByTruck
                {
                    AppPermissions.Pages_Reports_RevenueBreakdownByTruck, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_ScheduledReports
                {
                    AppPermissions.Pages_Reports_ScheduledReports, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Reports_BillingReconciliation
                {
                    AppPermissions.Pages_Reports_BillingReconciliation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Reports_PaymentReconciliation
                {
                    AppPermissions.Pages_Reports_PaymentReconciliation, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                    }
                },

                // Reports_RevenueAnalysis
                {
                    AppPermissions.Pages_Reports_RevenueAnalysis, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },

                // Schedule
                {
                    AppPermissions.Pages_Schedule, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // SendOrdersToDrivers
                {
                    AppPermissions.Pages_SendOrdersToDrivers, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },

                // Locations
                {
                    AppPermissions.Pages_Locations, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Locations_Merge
                {
                    AppPermissions.Pages_Locations_Merge, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                
                // Tickets_View
                {
                    AppPermissions.Pages_Tickets_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                // Tickets_Edit
                {
                    AppPermissions.Pages_Tickets_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers,
                    }
                },
                
                // Invoices
                {
                    AppPermissions.Pages_Invoices, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        //StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Backoffice,
                        //StaticRoleNames.Tenants.Managers,
                    }
                },

                // Trucks
                {
                    AppPermissions.Pages_Trucks, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Dispatching,
                        StaticRoleNames.Tenants.Managers,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // Trucks
                {
                    AppPermissions.Pages_OutOfServiceHistory_Delete, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // VehicleService_View
                {
                    AppPermissions.Pages_VehicleService_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // VehicleService_Edit
                {
                    AppPermissions.Pages_VehicleService_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // WorkOrders_View
                {
                    AppPermissions.Pages_WorkOrders_View, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // WorkOrders_Edit
                {
                    AppPermissions.Pages_WorkOrders_Edit, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // WorkOrders_EditLimited
                {
                    AppPermissions.Pages_WorkOrders_EditLimited, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Maintenance,
                        StaticRoleNames.Tenants.MaintenanceSupervisor,
                    }
                },
                // TimeOff
                {
                    AppPermissions.Pages_TimeOff, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // TimeEntry
                {
                    AppPermissions.Pages_TimeEntry, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // TimeEntry_EditAll
                {
                    AppPermissions.Pages_TimeEntry_EditAll, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                        StaticRoleNames.Tenants.Backoffice,
                        StaticRoleNames.Tenants.Managers
                    }
                },
                // TimeEntry_EditTimeClassifications
                {
                    AppPermissions.Pages_TimeEntry_EditTimeClassifications, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
                // TimeEntry_EditPersonal
                {
                    AppPermissions.Pages_TimeEntry_EditPersonal, new[]
                    {
                        StaticRoleNames.Tenants.Admin,
                        StaticRoleNames.Tenants.Administrative,
                    }
                },
            };

        public static string[] DefaultPermissions => _defaultRolePermissions.Keys.ToArray();
        public static bool IsPermissionsGrantedToRole(string roleName, string permissionName)
        {
            if (!_defaultRolePermissions.ContainsKey(permissionName))
            {
                return false;
            }
            return _defaultRolePermissions[permissionName].Contains(roleName);
        }

        public static IEnumerable<string> GetRolePermissions(string roleName)
        {
            foreach (string permission in DefaultPermissions)
            {
                if (IsPermissionsGrantedToRole(roleName, permission))
                {
                    yield return permission;
                }
            }
        }

    }
}
