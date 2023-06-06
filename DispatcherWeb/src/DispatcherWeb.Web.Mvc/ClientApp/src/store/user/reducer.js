import {
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_SETTINGS_SUCCESS,
    GET_USER_SETTINGS_FAILURE
} from './actionTypes';

const INIT_STATE = {
    userInfo: null,
    userSettings: null
};

const UserReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_USER_INFO_SUCCESS:
            return {
                ...state,
                userInfo: action.payload
            };
        case GET_USER_INFO_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_USER_SETTINGS_SUCCESS:
            return {
                ...state,
                userSettings: action.payload
            }
        case GET_USER_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state;
    };
};

export default UserReducer;