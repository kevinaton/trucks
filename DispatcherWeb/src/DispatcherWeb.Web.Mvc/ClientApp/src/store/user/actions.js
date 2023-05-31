import {
    GET_USER_INFO,
    GET_USER_INFO_SUCCESS,
    GET_USER_INFO_FAILURE
} from './actionTypes';

export const getUserInfo = () => ({
    type: GET_USER_INFO
});

export const getUserInfoSuccess = userInfo => ({
    type: GET_USER_INFO_SUCCESS,
    payload: userInfo
});

export const getUserInfoFailure = error => ({
    type: GET_USER_INFO_FAILURE,
    payload: error
});