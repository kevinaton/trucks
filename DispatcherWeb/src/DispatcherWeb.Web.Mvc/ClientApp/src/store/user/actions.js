import {
    GET_USER_INFO,
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_GENERAL_SETTINGS,
    GET_USER_GENERAL_SETTINGS_SUCCESS,
    GET_USER_GENERAL_SETTINGS_FAILURE,
    GET_USER_SETTING,
    GET_USER_SETTING_SUCCESS,
    GET_USER_SETTING_FAILURE,
    GET_USER_PROFILE_MENU,
    GET_USER_PROFILE_MENU_SUCCESS,
    GET_USER_PROFILE_MENU_FAILURE
} from './actionTypes';

export const getUserInfo = () => ({
    type: GET_USER_INFO
});

export const getUserInfoSuccess = userInfo => ({
    type: GET_USER_INFO_SUCCESS,
    payload: userInfo
});

export const getUserInfoFailure = error => ({
    type: GET_USER_INFO_FAILURE,
    payload: error
});

export const getUserGeneralSettings = () => ({
    type: GET_USER_GENERAL_SETTINGS
});

export const getUserGeneralSettingsSuccess = userGeneralSettings => ({
    type: GET_USER_GENERAL_SETTINGS_SUCCESS,
    payload: userGeneralSettings
});

export const getUserGeneralSettingsFailure = error => ({
    type: GET_USER_GENERAL_SETTINGS_FAILURE,
    payload: error
});

export const getUserSettingByName = settingName => ({
    type: GET_USER_SETTING,
    payload: settingName
});

export const getUserSettingByNameSuccess = userSetting => ({
    type: GET_USER_SETTING_SUCCESS,
    payload: userSetting
});

export const getUserSettingByNameFailure = error => ({
    type: GET_USER_SETTING_FAILURE,
    payload: error
});

export const getUserProfileMenu = () => ({
    type: GET_USER_PROFILE_MENU
});

export const getUserProfileMenuSuccess = userProfileMenu => ({
    type: GET_USER_PROFILE_MENU_SUCCESS,
    payload: userProfileMenu
});

export const getUserProfileMenuFailure = error => ({
    type: GET_USER_PROFILE_MENU_FAILURE,
    payload: error
});