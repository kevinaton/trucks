import {
    GET_USER_PROFILE_SETTINGS_SUCCESS,
    GET_USER_PROFILE_SETTINGS_FAILURE
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
        default:
            return state;
    }
};

export default UserProfileReducer;