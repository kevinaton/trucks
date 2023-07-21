import {
    GET_DRIVERS_SELECT_LIST,
    GET_DRIVERS_SELECT_LIST_SUCCESS,
    GET_DRIVERS_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getDriversSelectList = () => ({
    type: GET_DRIVERS_SELECT_LIST,
});

export const getDriversSelectListSuccess = driversSelectList => ({
    type: GET_DRIVERS_SELECT_LIST_SUCCESS,
    payload: driversSelectList,
});

export const getDriversSelectListFailure = error => ({
    type: GET_DRIVERS_SELECT_LIST_FAILURE,
    payload: error,
});