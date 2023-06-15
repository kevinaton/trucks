import {
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_PROFILE_MENU_SUCCESS,
    GET_USER_PROFILE_MENU_FAILURE,
    GET_USER_SETTING_SUCCESS,
    GET_USER_SETTING_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    userInfo: null,
    userProfileMenu : null,
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
        case GET_USER_PROFILE_MENU_SUCCESS:
            return {
                ...state,
                userProfileMenu: action.payload
            }
        case GET_USER_PROFILE_MENU_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case GET_USER_SETTING_SUCCESS:
            return {
                ...state,
                userSetting: action.payload
            }
        case GET_USER_SETTING_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state;
    };
};

export default UserReducer;