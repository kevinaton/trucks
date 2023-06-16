import {
    GET_LINKED_USERS,
    GET_LINKED_USERS_SUCCESS,
    GET_LINKED_USERS_FAILURE
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