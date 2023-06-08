import {
    GET_USER_INFO,
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE,
    GET_USER_SETTING,
    GET_USER_SETTING_SUCCESS,
    GET_USER_SETTING_FAILURE
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