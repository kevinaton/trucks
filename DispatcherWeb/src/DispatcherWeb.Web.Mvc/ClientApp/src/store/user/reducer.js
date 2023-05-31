import {
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE
} from './actionTypes';

const INIT_STATE = {
    userInfo: null
};

const UserReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_USER_INFO_SUCCESS:
            return {
                ...state,
                userInfo: action.payload
            };
        case GET_USER_INFO_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    };
};

export default UserReducer;