//User
export const GET_CURRENT_LOGIN_INFO = '/Session/GetCurrentLoginInformations';
export const GET_USER_SETTING = '/UserSettings/GetUserSettingByName';

//Layout
export const GET_MENU_ITEMS = '/Layout/GetMenu';
export const GET_SUPPORT_LINK_ADDRESS = '/Layout/GetSupportLinkAddress';

//Features
export const IS_FEATURE_ENABLED = '/Features/IsFeatureEnabled';

//Dashboard
export const GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW = '/DashboardView/GetScheduledTruckCountPartialView';

//Notifications
export const GET_USER_NOTIFICATIONS = '/Notification/GetUserNotifications?maxResultCount=3';
export const GET_USER_PRIORITY_NOTIFICATIONS = '/Notification/GetUnreadPriorityNotifications';
export const GET_USER_NOTIFICATION_SETTINGS = '/Notification/GetNotificationSettings';
export const SET_ALL_NOTIFICATIONS_AS_READ = '/Notification/SetAllNotificationsAsRead';
export const SET_NOTIFICATION_AS_READ = '/Notification/SetNotificationAsRead';
export const UPDATE_USER_NOTIFICATION_SETTINGS = '/Notification/UpdateNotificationSettings';

//Offices
export const GET_OFFICES = '/Office/GetOfficesSelectList';

//Scheduling
export const GET_SCHEDULE_TRUCKS = '/Scheduling/GetScheduleTrucks';
export const GET_SCHEDULE_ORDERS = '/Scheduling/GetScheduleOrders';