import {
    GET_ACTIVE_CUSTOMERS_SELECT_LIST,
    GET_ACTIVE_CUSTOMERS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_CUSTOMERS_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getActiveCustomersSelectList = filter => ({
    type: GET_ACTIVE_CUSTOMERS_SELECT_LIST,
    payload: filter
});

export const getActiveCustomersSelectListSuccess = activeCustomersSelectList => ({
    type: GET_ACTIVE_CUSTOMERS_SELECT_LIST_SUCCESS,
    payload: activeCustomersSelectList
});

export const getActiveCustomersSelectListFailure = error => ({
    type: GET_ACTIVE_CUSTOMERS_SELECT_LIST_FAILURE,
    payload: error
});