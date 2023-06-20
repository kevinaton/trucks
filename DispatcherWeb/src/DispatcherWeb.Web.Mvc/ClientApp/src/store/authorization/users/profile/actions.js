import {
    GET_USER_PROFILE_SETTINGS,
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE
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