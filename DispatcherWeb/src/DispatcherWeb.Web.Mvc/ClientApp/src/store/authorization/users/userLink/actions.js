import {
    GET_LINKED_USERS,
    GET_LINKED_USERS_SUCCESS,
    GET_LINKED_USERS_FAILURE,
    LINK_TO_USER,
    LINK_TO_USER_SUCCESS,
    LINK_TO_USER_FAILURE,
    LINK_TO_USER_RESET,
    UNLINK_USER,
    UNLINK_USER_SUCCESS,
    UNLINK_USER_FAILURE,
    UNLINK_USER_RESET
} from './actionTypes';

export const getLinkedUsers = () => ({
    type: GET_LINKED_USERS
});

export const getLinkedUsersSuccess = linkedUsers => ({
    type: GET_LINKED_USERS_SUCCESS,
    payload: linkedUsers
});

export const getLinkedUsersFailure = error => ({
    type: GET_LINKED_USERS_FAILURE,
    payload: error
});

export const linkToUser = user => ({
    type: LINK_TO_USER,
    payload: user
});

export const linkToUserSuccess = response => ({
    type: LINK_TO_USER_SUCCESS,
    payload: response
});

export const linkToUserFailure = error => ({
    type: LINK_TO_USER_FAILURE,
    payload: error
});

export const linkToUserReset = () => ({
    type: LINK_TO_USER_RESET
});

export const unlinkUser = linkedUser => ({
    type: UNLINK_USER,
    payload: linkedUser
});

export const unlinkUserSuccess = linkedUser => ({
    type: UNLINK_USER_SUCCESS,
    payload: linkedUser
});

export const unlinkUserFailure = error => ({
    type: UNLINK_USER_FAILURE,
    payload: error
});

export const unlinkUserReset = () => ({
    type: UNLINK_USER_RESET
});