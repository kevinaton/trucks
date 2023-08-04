import {
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_APP_CONFIG_SUCCESS,
    GET_USER_APP_CONFIG_FAILURE,
    GET_USER_GENERAL_SETTINGS_SUCCESS,
    GET_USER_GENERAL_SETTINGS_FAILURE,
    GET_USER_SETTING_SUCCESS,
    GET_USER_SETTING_FAILURE,
    GET_USER_PROFILE_MENU_SUCCESS,
    GET_USER_PROFILE_MENU_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    userInfo: null,
    userAppConfig: null,
    userGeneralSettings: null,
    userSettings: null,
    userProfileMenu : null
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
        case GET_USER_APP_CONFIG_SUCCESS:
            return {
                ...state,
                userAppConfig: action.payload
            };
        case GET_USER_APP_CONFIG_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_USER_GENERAL_SETTINGS_SUCCESS:
            return {
                ...state,
                userGeneralSettings: action.payload
            };
        case GET_USER_GENERAL_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_USER_SETTING_SUCCESS:
            return {
                ...state,
                userSettings: action.payload
            };
        case GET_USER_SETTING_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_USER_PROFILE_MENU_SUCCESS:
            return {
                ...state,
                userProfileMenu: action.payload
            };
        case GET_USER_PROFILE_MENU_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    };
};

export default UserReducer;