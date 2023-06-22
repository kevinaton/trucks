import {
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE,
    CHANGE_PASSWORD_SUCCESS,
    CHANGE_PASSWORD_FAILURE,
    RESET_CHANGE_PASSWORD_STATE
} from './actionTypes';

const INIT_STATE = {
    userProfileSettings: null,
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
        default:
            return state;
    }
};

export default UserProfileReducer;