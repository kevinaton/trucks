import { 
    GET_USER_NOTIFICATIONS,
    GET_USER_NOTIFICATIONS_SUCCESS,
    GET_USER_NOTIFICATIONS_FAILURE
} from './actionTypes'

export const getUserNotifications = () => ({
    type: GET_USER_NOTIFICATIONS
})

export const getUserNotificationsSuccess = notifications => ({
    type: GET_USER_NOTIFICATIONS_SUCCESS,
    payload: notifications
})

export const getUserNotificationsFailure = error => ({
    type: GET_USER_NOTIFICATIONS_FAILURE,
    payload: error
})