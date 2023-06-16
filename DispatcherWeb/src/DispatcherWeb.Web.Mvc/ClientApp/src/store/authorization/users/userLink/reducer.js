import {
    GET_LINKED_USERS_SUCCESS,
    GET_LINKED_USERS_FAILURE
} from './actionTypes';

const INIT_STATE = {
    linkedUsers: null,
};

const UserLinkReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_LINKED_USERS_SUCCESS:
            return {
                ...state,
                linkedUsers: action.payload
            };
        case GET_LINKED_USERS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    }
};

export default UserLinkReducer;