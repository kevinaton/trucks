import {
    GET_USER_PROFILE_SETTINGS,
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    CHANGE_PASSWORD,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE,
    UPLOAD_PROFILE_PICTURE_FILE,
    UPLOAD_PROFILE_PICTURE_FILE_SUCCESS,
    UPLOAD_PROFILE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE
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

export const uploadProfilePictureFile = file => ({
    type: UPLOAD_PROFILE_PICTURE_FILE,
    payload: file
});

export const uploadProfilePictureFileSuccess = response => ({
    type: UPLOAD_PROFILE_PICTURE_FILE_SUCCESS,
    payload: response
});

export const uploadProfilePictureFileFailure = error => ({
    type: UPLOAD_PROFILE_PICTURE_FILE_FAILURE,
    payload: error
});

export const resetUploadProfilePictureFileState = () => ({
    type: RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE
});