import { isEmpty } from 'lodash';
import {
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    UPDATE_USER_PROFILE_SUCCESS,
    UPDATE_USER_PROFILE_FAILURE,
    RESET_UPDATE_USER_PROFILE_STATE,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE,
    UPLOAD_PROFILE_PICTURE_FILE_SUCCESS,
    UPLOAD_PROFILE_PICTURE_FILE_FAILURE,
    RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE,
    ENABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    ENABLE_GOOGLE_AUTHENTICATOR_FAILURE,
    DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS,
    DISABLE_GOOGLE_AUTHENTICATOR_FAILURE
} from './actionTypes';

const INIT_STATE = {
    userProfileSettings: null
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
            }
        case UPDATE_USER_PROFILE_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case RESET_UPDATE_USER_PROFILE_STATE: {
            return {
                ...state,
                profileUpdateSuccess: null
            }
        }
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
            }
        case UPLOAD_PROFILE_PICTURE_FILE_SUCCESS:
            return {
                ...state,
                uploadResponse: action.payload
            }
        case UPLOAD_PROFILE_PICTURE_FILE_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case RESET_UPLOAD_PROFILE_PICTURE_FILE_STATE: 
            return {
                ...state,
                uploadResponse: null
            }
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
            }
        }
        case ENABLE_GOOGLE_AUTHENTICATOR_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case DISABLE_GOOGLE_AUTHENTICATOR_SUCCESS:
            return {
                ...state
            }
        case DISABLE_GOOGLE_AUTHENTICATOR_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state;
    }
};

export default UserProfileReducer;