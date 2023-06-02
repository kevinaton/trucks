import {
    GET_USER_NOTIFICATIONS_SUCCESS,
    GET_USER_NOTIFICATIONS_FAILURE,
    SET_ALL_NOTIFICATIONS_AS_READ_SUCCESS,
    SET_ALL_NOTIFICATIONS_AS_READ_FAILURE,
    SET_NOTIFICATION_AS_READ_SUCCESS,
    SET_NOTIFICATION_AS_READ_FAILURE,
    GET_USER_NOTIFICATION_SETTINGS_SUCCESS,
    GET_USER_NOTIFICATION_SETTINGS_FAILURE,
    UPDATE_USER_NOTIFICATION_SETTINGS_SUCCESS,
    UPDATE_USER_NOTIFICATION_SETTINGS_FAILURE,
    UPDATE_USER_NOTIFICATION_SETTINGS_RESET
} from './actionTypes';
import { notificationState } from '../../common/enums/notificationState';

const INIT_STATE = {
    notifications: null,
    error: {},
    notificationSettings: null,
    updateSuccess: null
};

const NotificationReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_USER_NOTIFICATIONS_SUCCESS:
            return {
                ...state,
                notifications: action.payload
            };
        case GET_USER_NOTIFICATIONS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case SET_ALL_NOTIFICATIONS_AS_READ_SUCCESS: {
            const items = state.notifications.result.items.map(notification => {
                return {
                    ...notification,
                    state: notificationState.READ
                };
            });
            
            const result = {
                ...state.notifications.result,
                items,
                unreadCount: 0
            };

            return {
                ...state,
                notifications: {
                    ...state.notifications,
                    result
                }
            };
        }
        case SET_ALL_NOTIFICATIONS_AS_READ_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case SET_NOTIFICATION_AS_READ_SUCCESS: {
            const items = state.notifications.result.items.map(notification => {
                if (notification.id === action.payload.id) {
                    return {
                        ...notification,
                        state: notificationState.READ
                    };
                }
                return notification;
            });

            const unreadCount = state.notifications.result.unreadCount - 1;
            
            const result = {
                ...state.notifications.result,
                items,
                unreadCount
            };

            return {
                ...state,
                notifications: {
                    ...state.notifications,
                    result
                }
            };
        }
        case SET_NOTIFICATION_AS_READ_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_USER_NOTIFICATION_SETTINGS_SUCCESS:
            return {
                ...state,
                notificationSettings: action.payload
            };
        case GET_USER_NOTIFICATION_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case UPDATE_USER_NOTIFICATION_SETTINGS_SUCCESS:
            return {
                ...state,
                notificationSettings: {
                    ...state.notificationSettings,
                    result: action.payload
                },
                updateSuccess: true
            };
        case UPDATE_USER_NOTIFICATION_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case UPDATE_USER_NOTIFICATION_SETTINGS_RESET:
            return {
                ...state,
                updateSuccess: null
            }
        default:
            return state;
    }   
};

export default NotificationReducer;