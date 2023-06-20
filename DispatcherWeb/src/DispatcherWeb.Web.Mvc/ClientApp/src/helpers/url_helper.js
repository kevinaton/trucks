//Layout
export const GET_MENU_ITEMS = '/Layout/GetMenu';
export const GET_SUPPORT_LINK_ADDRESS = '/Layout/GetSupportLinkAddress';
export const GET_CURRENT_USER_LOGIN_INFO = '/Layout/GetUserProfile';

//Dashboard
export const GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW = '/DashboardView/GetScheduledTruckCountPartialView';

//User
export const GET_CURRENT_LOGIN_INFO = '/Session/GetCurrentLoginInformations';
export const GET_USER_SETTING = '/UserSettings/GetUserSettingByName';

//User Profile
export const GET_USER_PROFILE_SETTINGS = '/Profile/GetUserProfileSettings';

//User Link
export const GET_LINKED_USERS = '/UserLink/GetRecentlyUsedLinkedUsers';
export const LINK_TO_USER = '/UserLink/LinkToUser';

//Notifications
export const GET_USER_NOTIFICATIONS = '/Notification/GetUserNotifications?maxResultCount=3';
export const GET_USER_PRIORITY_NOTIFICATIONS = '/Notification/GetUnreadPriorityNotifications';
export const GET_USER_NOTIFICATION_SETTINGS = '/Notification/GetNotificationSettings';
export const SET_ALL_NOTIFICATIONS_AS_READ = '/Notification/SetAllNotificationsAsRead';
export const SET_NOTIFICATION_AS_READ = '/Notification/SetNotificationAsRead';
export const UPDATE_USER_NOTIFICATION_SETTINGS = '/Notification/UpdateNotificationSettings';