import {
    GET_USER_PROFILE_SETTINGS,
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    UPDATE_USER_PROFILE,
    UPDATE_USER_PROFILE_SUCCESS,
    UPDATE_USER_PROFILE_FAILURE,
    RESET_UPDATE_USER_PROFILE_STATE,
    CHANGE_PASSWORD,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE,
    UPLOAD_PROFILE_PICTURE_FILE,
    UPLOAD_PROFILE_PICTURE_FILE_SUCCESS,
    UPLOAD_PROFILE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE,
    UPLOAD_SIGNATURE_PICTURE_FILE,
    UPLOAD_SIGNATURE_PICTURE_FILE_SUCCESS,
    UPLOAD_SIGNATURE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_SIGNATURE_PICTURE_FILE_STATE,
    ENABLE_GOOGLE_AUTHENTICATOR,
    ENABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    ENABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    DISABLE_GOOGLE_AUTHENTICATOR,
    DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    DISABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    DOWNLOAD_COLLECTED_DATA,
    DOWNLOAD_COLLECTED_DATA_SUCCESS,
    DOWNLOAD_COLLECTED_DATA_FAILURE,
    RESET_DOWNLOAD_COLLECTED_DATA_STATE
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

export const updateUserProfile = userProfile => ({
    type: UPDATE_USER_PROFILE,
    payload: userProfile
});

export const updateUserProfileSuccess = userProfile => ({
    type: UPDATE_USER_PROFILE_SUCCESS,
    payload: userProfile
});

export const updateUserProfileFailure = error => ({
    type: UPDATE_USER_PROFILE_FAILURE,
    payload: error
});

export const resetUpdateUserProfile = () => ({
    type: RESET_UPDATE_USER_PROFILE_STATE
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

export const uploadSignaturePictureFile = file => ({
    type: UPLOAD_SIGNATURE_PICTURE_FILE,
    payload: file
});

export const uploadSignaturePictureFileSuccess = response => ({
    type: UPLOAD_SIGNATURE_PICTURE_FILE_SUCCESS,
    payload: response
});

export const uploadSignaturePictureFileFailure = error => ({
    type: UPLOAD_SIGNATURE_PICTURE_FILE_FAILURE,
    payload: error
});

export const resetUploadSignaturePictureFileState = () => ({
    type: RESET_UPLOAD_SIGNATURE_PICTURE_FILE_STATE
});

export const enableGoogleAuthenticator = () => ({
    type: ENABLE_GOOGLE_AUTHENTICATOR
});

export const enableGoogleAuthenticatorSuccess = authenticatorKey => ({
    type: ENABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    payload: authenticatorKey
});

export const enableGoogleAuthenticatorFailure = error => ({
    type: ENABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    payload: error
});

export const disableGoogleAuthenticator = () => ({
    type: DISABLE_GOOGLE_AUTHENTICATOR
});

export const disableGoogleAuthenticatorSuccess = () => ({
    type: DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS
});

export const disableGoogleAuthenticatorFailure = error => ({
    type: DISABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    payload: error
});

export const downloadCollectedData = () => ({
    type: DOWNLOAD_COLLECTED_DATA
});

export const downloadCollectedDataSuccess = () => ({
    type: DOWNLOAD_COLLECTED_DATA_SUCCESS
});

export const downloadCollectedDataFailure = error => ({
    type: DOWNLOAD_COLLECTED_DATA_FAILURE,
    payload: error
});

export const resetDownloadCollectedDataState = () => ({
    type: RESET_DOWNLOAD_COLLECTED_DATA_STATE
});