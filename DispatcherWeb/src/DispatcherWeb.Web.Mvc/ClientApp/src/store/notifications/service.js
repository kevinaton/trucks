import { get, post } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// get user notifications
export const getUserNotifications = () => get(url.GET_USER_NOTIFICATIONS);

// set all notifications as read
export const setAllNotificationsAsRead = () => post(url.SET_ALL_NOTIFICATIONS_AS_READ);

// set notification as read
export const setNotificationAsRead = notification => post(url.SET_NOTIFICATION_AS_READ, notification);