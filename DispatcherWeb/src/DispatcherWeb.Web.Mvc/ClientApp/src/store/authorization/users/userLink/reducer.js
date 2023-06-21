import {
    GET_LINKED_USERS_SUCCESS,
    GET_LINKED_USERS_FAILURE,
    UNLINK_USER_SUCCESS,
    UNLINK_USER_FAILURE,
    UNLINK_USER_RESET
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
        case UNLINK_USER_SUCCESS: {
            const linkedUserToRemove = action.payload;
            const updatedLinkedUsers = state.linkedUsers.result.items.filter(user => user.id !== linkedUserToRemove.userId);
            const result = {
                ...state.linkedUsers.result,
                items: updatedLinkedUsers
            }

            return {
                ...state,
                linkedUsers: {
                    ...state.linkedUsers,
                    result
                },
                unlinkSuccess: true
            };
        }
        case UNLINK_USER_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case UNLINK_USER_RESET:
            return {
                ...state,
                unlinkSuccess: null
            }
        default:
            return state;
    }
};

export default UserLinkReducer;