import {
    BACK_TO_IMPERSONATOR,
    BACK_TO_IMPERSONATOR_SUCCESS,
    BACK_TO_IMPERSONATOR_FAILURE,
    IMPERSONATE_SIGNIN,
    IMPERSONATE_SIGNIN_SUCCESS,
    IMPERSONATE_SIGNIN_FAILURE,
    SWITCH_TO_USER,
    SWITCH_TO_USER_SUCCESS,
    SWITCH_TO_USER_FAILURE
} from './actionTypes';

export const backToImpersonator = () => ({
    type: BACK_TO_IMPERSONATOR
});

export const backToImpersonatorSuccess = response => ({
    type: BACK_TO_IMPERSONATOR_SUCCESS,
    payload: response
});

export const backToImpersonatorFailure = error => ({
    type: BACK_TO_IMPERSONATOR_FAILURE,
    payload: error
});

export const impersonateSignin = tokenId => ({
    type: IMPERSONATE_SIGNIN,
    payload: tokenId
});

export const impersonateSigninSuccess = response => ({
    type: IMPERSONATE_SIGNIN_SUCCESS,
    payload: response
});

export const impersonateSigninFailure = error => ({
    type: IMPERSONATE_SIGNIN_FAILURE,
    payload: error
});

export const switchToUser = account => ({
    type: SWITCH_TO_USER,
    payload: account
});

export const switchToUserSuccess = response => ({
    type: SWITCH_TO_USER_SUCCESS,
    payload: response
});

export const switchToUserFailure = error => ({
    type: SWITCH_TO_USER_FAILURE,
    payload: error
});