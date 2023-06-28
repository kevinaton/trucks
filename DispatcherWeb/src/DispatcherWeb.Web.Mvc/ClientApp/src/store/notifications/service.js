import { get, post, put } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get user notifications
export const getUserNotifications = () => get(url.GET_USER_NOTIFICATIONS);

// get user priority notifications
export const getUserPriorityNotifications = () => get(url.GET_USER_PRIORITY_NOTIFICATIONS);

// set all notifications as read
export const setAllNotificationsAsRead = () => post(url.SET_ALL_NOTIFICATIONS_AS_READ);

// set notification as read
export const setNotificationAsRead = notification => post(url.SET_NOTIFICATION_AS_READ, notification);

// get user notification settings
export const getUserNotificationSettings = () => get(url.GET_USER_NOTIFICATION_SETTINGS);

// update user notification settings
export const updateUserNotificationSettings = settings => put(url.UPDATE_USER_NOTIFICATION_SETTINGS, settings);