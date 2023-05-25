import {
    GET_USER_NOTIFICATIONS_SUCCESS,
    GET_USER_NOTIFICATIONS_FAILURE
} from './actionTypes'

const INIT_STATE = {
    notifications: null
}

const NotificationBellReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_USER_NOTIFICATIONS_SUCCESS:
            return {
                ...state,
                notifications: action.payload
            }
        case GET_USER_NOTIFICATIONS_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state
    }   
}

export default NotificationBellReducer