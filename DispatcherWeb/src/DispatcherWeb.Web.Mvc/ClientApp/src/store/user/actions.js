import {
    GET_USER_INFO,
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_SETTINGS,
    GET_USER_SETTINGS_SUCCESS,
    GET_USER_SETTINGS_FAILURE
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

export const getUserSettingsByName = settingName => ({
    type: GET_USER_SETTINGS,
    payload: settingName
});

export const getUserSettingsByNameSuccess = userSettings => ({
    type: GET_USER_SETTINGS_SUCCESS,
    payload: userSettings
});

export const getUserSettingsByNameFailure = error => ({
    type: GET_USER_SETTINGS_FAILURE,
    payload: error
});