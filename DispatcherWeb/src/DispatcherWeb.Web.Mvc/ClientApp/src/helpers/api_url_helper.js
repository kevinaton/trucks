//App Settings
export const GET_TENANT_SETTINGS = '/TenantSettings/GetAllSettings';

//Customers
export const GET_ACTIVE_CUSTOMERS_SELECT_LIST = '/Customer/GetActiveCustomersSelectList';

//Dashboard
export const GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW = '/DashboardView/GetScheduledTruckCountPartialView';

//Designations
export const GET_DESIGNATIONS_SELECT_LIST = '/Designation/GetDesignationSelectList';

//Drivers
export const GET_DRIVERS_SELECT_LIST = '/Driver/GetDriversSelectList';
export const GET_DRIVER_FOR_EDIT = '/Driver/GetDriverForEdit';

//DriverAssigments
export const SET_DRIVER_FOR_TRUCK = '/DriverAssignment/SetDriverForTruck';
export const HAS_ORDER_LINE_TRUCKS = '/DriverAssignment/HasOrderLineTrucks';

//Features
export const IS_FEATURE_ENABLED = '/Features/IsFeatureEnabled';

//Google Authenticator
export const ENABLE_GOOGLE_AUTHENTICATOR = '/Profile/UpdateGoogleAuthenticatorKey';
export const DISABLE_GOOGLE_AUTHENTICATOR = '/Profile/DisableGoogleAuthenticator';

//Layout
export const GET_MENU_ITEMS = '/Layout/GetMenu';
export const GET_SUPPORT_LINK_ADDRESS = '/Layout/GetSupportLinkAddress';
export const GET_CURRENT_USER_LOGIN_INFO = '/Layout/GetUserProfile';

//LeaseHaulers
export const GET_LEASE_HAULER_DRIVERS_SELECT_LIST = '/LeaseHauler/GetLeaseHaulerDriversSelectList';

//LeaseHaulerRequestEdit
export const SET_DRIVER_FOR_LEASE_HAULER_TRUCK = '/LeaseHaulerRequestEdit/SetDriverForLeaseHaulerTruck';
export const REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE = '/LeaseHaulerRequestEdit/RemoveAvailableLeaseHaulerTruckFromSchedule';

//Locations
export const GET_LOCATIONS_SELECT_LIST = '/Location/GetLocationsSelectList';

//Notifications
export const GET_USER_NOTIFICATIONS = '/Notification/GetUserNotifications?maxResultCount=3';
export const GET_USER_PRIORITY_NOTIFICATIONS = '/Notification/GetUnreadPriorityNotifications';
export const GET_USER_NOTIFICATION_SETTINGS = '/Notification/GetNotificationSettings';
export const SET_ALL_NOTIFICATIONS_AS_READ = '/Notification/SetAllNotificationsAsRead';
export const SET_NOTIFICATION_AS_READ = '/Notification/SetNotificationAsRead';
export const UPDATE_USER_NOTIFICATION_SETTINGS = '/Notification/UpdateNotificationSettings';

//Offices
export const GET_OFFICES = '/Office/GetOfficesSelectList';

//Orders
export const GET_ORDER_PRIORITY_SELECT_LIST = '/Order/GetOrderPrioritySelectList';
export const GET_JOB_FOR_EDIT = '/Order/GetJobForEdit';
export const GET_ORDER_FOR_EDIT = '/Order/GetOrderForEdit';

//Scheduling
export const GET_SCHEDULE_TRUCKS = '/Scheduling/GetScheduleTrucks';
export const GET_SCHEDULE_TRUCK_BY_ID = '/Scheduling/GetScheduleTruckById';
export const GET_SCHEDULE_ORDERS = '/Scheduling/GetScheduleOrders';

//Services
export const GET_SERVICES_WITH_TAX_INFO_SELECT_LIST = '/Service/GetServicesWithTaxInfoSelectList';

//TrailerAssignments
export const SET_TRAILER_FOR_TRACTOR = '/TrailerAssignment/SetTrailerForTractor';
export const SET_TRACTOR_FOR_TRAILER = '/TrailerAssignment/SetTractorForTrailer';

//Trucks
export const GET_VEHICLE_CATEGORIES = '/Truck/GetVehicleCategoriesSelectList';
export const GET_ACTIVE_TRAILERS_SELECT_LIST = '/Truck/GetActiveTrailersSelectList';
export const GET_ACTIVE_TRACTORS_SELECT_LIST = '/Truck/GetActiveTractorsSelectList';
export const GET_BED_CONSTRUCTION_SELECT_LIST = '/Truck/GetBedConstructionSelectList';
export const GET_MAKES_SELECT_LIST = '/Truck/GetMakesSelectList';
export const GET_MODELS_SELECT_LIST = '/Truck/GetModelsSelectList';
export const GET_BED_CONSTRUCTIONS = '/Truck/GetBedConstructions';
export const GET_FUEL_TYPE_SELECT_LIST = '/Truck/GetFuelTypeSelectList';
export const GET_TRUCK_FOR_EDIT = '/Truck/GetTruckForEdit';
export const GET_WIALON_DEVICE_TYPES_SELECT_LIST = '/TruckTelematics/GetWialonDeviceTypesSelectList';
export const EDIT_TRUCK = '/Truck/EditTruck';
export const SET_TRUCK_IS_OUT_OF_SERVICE = '/Truck/SetTruckIsOutOfService';

//Units of Measure
export const GET_UNITS_OF_MEASURE_SELECT_LIST = '/UnitOfMeasure/GetUnitsOfMeasureSelectList';

//User
export const GET_CURRENT_LOGIN_INFO = '/Session/GetCurrentLoginInformations';
export const GET_USER_APP_SETTINGS = '/UserSettings/GetUserAppConfig';
export const GET_USER_GENERAL_SETTINGS = '/UserSettings/GetGeneralSettings';
export const GET_USER_SETTING = '/UserSettings/GetUserSettingByName';

//User Link
export const GET_LINKED_USERS = '/UserLink/GetRecentlyUsedLinkedUsers';
export const LINK_TO_USER = '/UserLink/LinkToUser';
export const UNLINK_USER = '/UserLink/UnlinkUser';

//User Profile
export const GET_USER_PROFILE_SETTINGS = '/Profile/GetUserProfileSettings';
export const UPDATE_USER_PROFILE = '/Profile/UpdateCurrentUserProfile';
export const UPDATE_PROFILE_PICTURE = '/Profile/UpdateProfilePicture';
export const UPDATE_SIGNATURE_PICTURE = '/Profile/UpdateSignaturePicture';
export const CHANGE_PASSWORD = '/Profile/ChangePassword';
export const DOWNLOAD_COLLECTED_DATA = '/Profile/PrepareCollectedData';