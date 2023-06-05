import { 
    GET_USER_NOTIFICATIONS,
    GET_USER_NOTIFICATIONS_SUCCESS,
    GET_USER_NOTIFICATIONS_FAILURE,
    GET_USER_PRIORITY_NOTIFICATIONS,
    GET_USER_PRIORITY_NOTIFICATIONS_SUCCESS,
    GET_USER_PRIORITY_NOTIFICATIONS_FAILURE,
    SET_ALL_NOTIFICATIONS_AS_READ,
    SET_ALL_NOTIFICATIONS_AS_READ_SUCCESS,
    SET_ALL_NOTIFICATIONS_AS_READ_FAILURE,
    SET_NOTIFICATION_AS_READ,
    SET_NOTIFICATION_AS_READ_SUCCESS,
    SET_NOTIFICATION_AS_READ_FAILURE,
    GET_USER_NOTIFICATION_SETTINGS,
    GET_USER_NOTIFICATION_SETTINGS_SUCCESS,
    GET_USER_NOTIFICATION_SETTINGS_FAILURE,
    UPDATE_USER_NOTIFICATION_SETTINGS,
    UPDATE_USER_NOTIFICATION_SETTINGS_SUCCESS,
    UPDATE_USER_NOTIFICATION_SETTINGS_FAILURE,
    UPDATE_USER_NOTIFICATION_SETTINGS_RESET
} from './actionTypes';

export const getUserNotifications = () => ({
    type: GET_USER_NOTIFICATIONS
});

export const getUserNotificationsSuccess = notifications => ({
    type: GET_USER_NOTIFICATIONS_SUCCESS,
    payload: notifications
});

export const getUserNotificationsFailure = error => ({
    type: GET_USER_NOTIFICATIONS_FAILURE,
    payload: error
});

export const getUserPriorityNotifications = () => ({
    type: GET_USER_PRIORITY_NOTIFICATIONS
});

export const getUserPriorityNotificationsSuccess = priorityNotifications => ({
    type: GET_USER_PRIORITY_NOTIFICATIONS_SUCCESS,
    payload: priorityNotifications
});

export const getUserPriorityNotificationsFailure = error => ({
    type: GET_USER_PRIORITY_NOTIFICATIONS_FAILURE,
    payload: error
});

export const setAllNotificationsAsRead = () => ({
    type: SET_ALL_NOTIFICATIONS_AS_READ,
});

export const setAllNotificationsAsReadSuccess = notifications => ({
    type: SET_ALL_NOTIFICATIONS_AS_READ_SUCCESS,
    payload: notifications
});

export const setAllNotificationsAsReadFailure = error => ({
    type: SET_ALL_NOTIFICATIONS_AS_READ_FAILURE,
    payload: error
});

export const setNotificationAsRead = notification => ({
    type: SET_NOTIFICATION_AS_READ,
    payload: notification
});

export const setNotificationAsReadSuccess = notification => ({
    type: SET_NOTIFICATION_AS_READ_SUCCESS,
    payload: notification
});

export const setNotificationAsReadFailure = error => ({
    type: SET_NOTIFICATION_AS_READ_FAILURE,
    payload: error
});

export const getUserNotificationSettings = () => ({
    type: GET_USER_NOTIFICATION_SETTINGS
});

export const getUserNotificationSettingsSuccess = settings => ({
    type: GET_USER_NOTIFICATION_SETTINGS_SUCCESS,
    payload: settings
});

export const getUserNotificationSettingsFailure = error => ({
    type: GET_USER_NOTIFICATION_SETTINGS_FAILURE,
    payload: error
});

export const updateUserNotificationSettings = settings => ({
    type: UPDATE_USER_NOTIFICATION_SETTINGS,
    payload: settings
});

export const updateUserNotificationSettingsSuccess = settings => ({
    type: UPDATE_USER_NOTIFICATION_SETTINGS_SUCCESS,
    payload: settings
});

export const updateUserNotificationSettingsFailure = error => ({
    type: UPDATE_USER_NOTIFICATION_SETTINGS_FAILURE,
    payload: error
});

export const updateUserNotificationSettingsReset = () => ({
    type: UPDATE_USER_NOTIFICATION_SETTINGS_RESET
});