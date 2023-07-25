import {
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    UPDATE_USER_PROFILE_SUCCESS,
    UPDATE_USER_PROFILE_FAILURE,
    RESET_UPDATE_USER_PROFILE_STATE, 
    UPDATE_PROFILE_PICTURE_SUCCESS,
    UPDATE_PROFILE_PICTURE_FAILURE,
    RESET_UPDATE_PROFILE_PICTURE_STATE,
    UPDATE_SIGNATURE_PICTURE_SUCCESS,
    UPDATE_SIGNATURE_PICTURE_FAILURE,
    RESET_UPDATE_SIGNATURE_PICTURE_STATE,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE,
    UPLOAD_PROFILE_PICTURE_FILE_SUCCESS,
    UPLOAD_PROFILE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE,
    UPLOAD_SIGNATURE_PICTURE_FILE_SUCCESS,
    UPLOAD_SIGNATURE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_SIGNATURE_PICTURE_FILE_STATE,
    ENABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    ENABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    DISABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    DOWNLOAD_COLLECTED_DATA_SUCCESS,
    DOWNLOAD_COLLECTED_DATA_FAILURE,
    RESET_DOWNLOAD_COLLECTED_DATA_STATE
} from './actionTypes';

const INIT_STATE = {
    userProfileSettings: null,
    profileUpdateSuccess: null, 
    profilePictureUpdateSuccess: null,
    signatureUpdateSuccess: null,
    updateSuccess: null, 
    uploadResponse: null,
    downloadSuccess: null
};

const UserProfileReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_USER_PROFILE_SETTINGS_SUCCESS:
            return {
                ...state,
                userProfileSettings: action.payload
            };
        case GET_USER_PROFILE_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case UPDATE_USER_PROFILE_SUCCESS:
            return {
                ...state,
                userProfileSettings: action.payload,
                profileUpdateSuccess: true
            };
        case UPDATE_USER_PROFILE_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_UPDATE_USER_PROFILE_STATE: {
            return {
                ...state,
                profileUpdateSuccess: null
            };
        }
        case UPDATE_PROFILE_PICTURE_SUCCESS:
            return {
                ...state,
                profilePictureUpdateSuccess: true
            };
        case UPDATE_PROFILE_PICTURE_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_UPDATE_PROFILE_PICTURE_STATE:
            return {
                ...state,
                profilePictureUpdateSuccess: null
            };
        case UPDATE_SIGNATURE_PICTURE_SUCCESS:
            return {
                ...state,
                signatureUpdateSuccess: true
            };
        case UPDATE_SIGNATURE_PICTURE_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_UPDATE_SIGNATURE_PICTURE_STATE:
            return {
                ...state,
                signatureUpdateSuccess: null
            };
        case CHANGE_PASSWORD_SUCCESS:
            return {
                ...state,
                updateSuccess: true
            };
        case CHANGE_PASSWORD_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_CHANGE_PASSWORD_STATE:
            return {
                ...state,
                updateSuccess: null
            };
        case UPLOAD_PROFILE_PICTURE_FILE_SUCCESS:
            return {
                ...state,
                uploadResponse: action.payload
            };
        case UPLOAD_PROFILE_PICTURE_FILE_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE: 
            return {
                ...state,
                uploadResponse: null
            };
        case UPLOAD_SIGNATURE_PICTURE_FILE_SUCCESS:
            return {
                ...state,
                signatureUploadResponse: action.payload
            };
        case UPLOAD_SIGNATURE_PICTURE_FILE_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case RESET_UPLOAD_SIGNATURE_PICTURE_FILE_STATE:
            return {
                ...state,
                signatureUploadResponse: null
            };
        case ENABLE_GOOGLE_AUTHENTICATOR_SUCCESS: {
            const response = action.payload;
            const { qrCodeSetupImageUrl } = response.result;
            const result = {
                ...state.userProfileSettings.result,
                qrCodeSetupImageUrl,
                isGoogleAuthenticatorEnabled: true
            };

            return {
                ...state,
                userProfileSettings: {
                    ...state.userProfileSettings,
                    result
                }
            };
        }
        case ENABLE_GOOGLE_AUTHENTICATOR_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS:
            return {
                ...state
            };
        case DISABLE_GOOGLE_AUTHENTICATOR_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case DOWNLOAD_COLLECTED_DATA_SUCCESS:
            return {
                ...state,
                downloadSuccess: true
            }
        case DOWNLOAD_COLLECTED_DATA_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case RESET_DOWNLOAD_COLLECTED_DATA_STATE: 
            return {
                ...state,
                downloadSuccess: null
            }
        default:
            return state;
    }
};

export default UserProfileReducer;