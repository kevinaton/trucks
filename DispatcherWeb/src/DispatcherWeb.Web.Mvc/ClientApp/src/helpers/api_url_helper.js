//App Settings
export const GET_TENANT_SETTINGS = '/TenantSettings/GetAllSettings';

//Layout
export const GET_MENU_ITEMS = '/Layout/GetMenu';
export const GET_SUPPORT_LINK_ADDRESS = '/Layout/GetSupportLinkAddress';
export const GET_CURRENT_USER_LOGIN_INFO = '/Layout/GetUserProfile';

//Features
export const IS_FEATURE_ENABLED = '/Features/IsFeatureEnabled';

//Dashboard
export const GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW = '/DashboardView/GetScheduledTruckCountPartialView';

//User
export const GET_CURRENT_LOGIN_INFO = '/Session/GetCurrentLoginInformations';
export const GET_USER_APP_SETTINGS = '/UserSettings/GetUserAppConfig';
export const GET_USER_GENERAL_SETTINGS = '/UserSettings/GetGeneralSettings';
export const GET_USER_SETTING = '/UserSettings/GetUserSettingByName';

//User Profile
export const GET_USER_PROFILE_SETTINGS = '/Profile/GetUserProfileSettings';
export const UPDATE_USER_PROFILE = '/Profile/UpdateCurrentUserProfile';
export const UPDATE_PROFILE_PICTURE = '/Profile/UpdateProfilePicture';
export const UPDATE_SIGNATURE_PICTURE = '/Profile/UpdateSignaturePicture';
export const CHANGE_PASSWORD = '/Profile/ChangePassword';
export const DOWNLOAD_COLLECTED_DATA = '/Profile/PrepareCollectedData';

//Google Authenticator
export const ENABLE_GOOGLE_AUTHENTICATOR = '/Profile/UpdateGoogleAuthenticatorKey';
export const DISABLE_GOOGLE_AUTHENTICATOR = '/Profile/DisableGoogleAuthenticator';

//User Link
export const GET_LINKED_USERS = '/UserLink/GetRecentlyUsedLinkedUsers';
export const LINK_TO_USER = '/UserLink/LinkToUser';
export const UNLINK_USER = '/UserLink/UnlinkUser';

//Notifications
export const GET_USER_NOTIFICATIONS = '/Notification/GetUserNotifications?maxResultCount=3';
export const GET_USER_PRIORITY_NOTIFICATIONS = '/Notification/GetUnreadPriorityNotifications';
export const GET_USER_NOTIFICATION_SETTINGS = '/Notification/GetNotificationSettings';
export const SET_ALL_NOTIFICATIONS_AS_READ = '/Notification/SetAllNotificationsAsRead';
export const SET_NOTIFICATION_AS_READ = '/Notification/SetNotificationAsRead';
export const UPDATE_USER_NOTIFICATION_SETTINGS = '/Notification/UpdateNotificationSettings';

//Offices
export const GET_OFFICES = '/Office/GetOfficesSelectList';

//Drivers
export const GET_DRIVERS_SELECT_LIST = '/Driver/GetDriversSelectList';
export const GET_DRIVER_FOR_EDIT = '/Driver/GetDriverForEdit';

//Trucks
export const GET_VEHICLE_CATEGORIES = '/Truck/GetVehicleCategoriesSelectList';
export const GET_ACTIVE_TRAILERs_SELECT_LIST = '/Truck/GetActiveTrailersSelectList';
export const GET_BED_CONSTRUCTION_SELECT_LIST = '/Truck/GetBedConstructionSelectList';
export const GET_FUEL_TYPE_SELECT_LIST = '/Truck/GetFuelTypeSelectList';
export const GET_TRUCK_FOR_EDIT = '/Truck/GetTruckForEdit';
export const GET_WIALON_DEVICE_TYPES_SELECT_LIST = '/TruckTelematics/GetWialonDeviceTypesSelectList';
export const EDIT_TRUCK = '/Truck/EditTruck';
export const SET_TRUCK_IS_OUT_OF_SERVICE = '/Truck/SetTruckIsOutOfService';

//LeaseHaulers
export const GET_LEASE_HAULER_DRIVERS_SELECT_LIST = '/LeaseHauler/GetLeaseHaulerDriversSelectList';

//Scheduling
export const GET_SCHEDULE_TRUCKS = '/Scheduling/GetScheduleTrucks';
export const GET_SCHEDULE_TRUCK_BY_ID = '/Scheduling/GetScheduleTruckById';
export const GET_SCHEDULE_ORDERS = '/Scheduling/GetScheduleOrders';