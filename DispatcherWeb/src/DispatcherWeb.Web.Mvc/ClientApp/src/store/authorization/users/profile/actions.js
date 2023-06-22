import {
    GET_USER_PROFILE_SETTINGS,
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    CHANGE_PASSWORD,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE
} from './actionTypes';

export const getUserProfileSettings = () => ({
    type: GET_USER_PROFILE_SETTINGS
});

export const getUserProfileSettingsSuccess = userProfileSettings => ({
    type: GET_USER_PROFILE_SETTINGS_SUCCESS,
    payload: userProfileSettings
});

export const getUserProfileSettingsFailure = error => ({
    type: GET_USER_PROFILE_SETTINGS_FAILURE,
    payload: error
});

export const changePassword = password => ({
    type: CHANGE_PASSWORD,
    payload: password
});

export const changePasswordSuccess = () => ({
    type: CHANGE_PASSWORD_SUCCESS
});

export const changePasswordFailure = error => ({
    type: CHANGE_PASSWORD_FAILURE,
    payload: error
});

export const resetChangePasswordState = () => ({
    type: RESET_CHANGE_PASSWORD_STATE
});