import {
    BACK_TO_IMPERSONATOR_SUCCESS,
    BACK_TO_IMPERSONATOR_FAILURE,
    IMPERSONATE_SIGNIN_SUCCESS,
    IMPERSONATE_SIGNIN_FAILURE,
    SWITCH_TO_USER_SUCCESS,
    SWITCH_TO_USER_FAILURE
} from './actionTypes';

const INIT_STATE = {
    backToImpersonatorResponse: null,
    switchAccountResponse: null
};

const AccountReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case BACK_TO_IMPERSONATOR_SUCCESS:
            return {
                ...state,
                backToImpersonatorResponse: action.payload
            }
        case BACK_TO_IMPERSONATOR_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case IMPERSONATE_SIGNIN_SUCCESS:
            return {
                ...state,
                response: action.payload
            }
        case IMPERSONATE_SIGNIN_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        case SWITCH_TO_USER_SUCCESS:
            return {
                ...state,
                switchAccountResponse: action.payload
            };
        case SWITCH_TO_USER_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    }
};

export default AccountReducer;