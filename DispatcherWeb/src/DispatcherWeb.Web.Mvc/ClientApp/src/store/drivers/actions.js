import {
    GET_DRIVERS_SELECT_LIST,
    GET_DRIVERS_SELECT_LIST_SUCCESS,
    GET_DRIVERS_SELECT_LIST_FAILURE,
    GET_DRIVER_FOR_EDIT,
    GET_DRIVER_FOR_EDIT_SUCCESS,
    GET_DRIVER_FOR_EDIT_FAILURE,
} from './actionTypes';

export const getDriversSelectList = (includeLeaseHaulerDrivers, maxResultCount, skipCount) => ({
    type: GET_DRIVERS_SELECT_LIST,
    payload: { 
        includeLeaseHaulerDrivers, 
        maxResultCount, 
        skipCount 
    },
});

export const getDriversSelectListSuccess = driversSelectList => ({
    type: GET_DRIVERS_SELECT_LIST_SUCCESS,
    payload: driversSelectList,
});

export const getDriversSelectListFailure = error => ({
    type: GET_DRIVERS_SELECT_LIST_FAILURE,
    payload: error,
});

export const getDriverForEdit = () => ({
    type: GET_DRIVER_FOR_EDIT,
});

export const getDriverForEditSuccess = driverForEdit => ({
    type: GET_DRIVER_FOR_EDIT_SUCCESS,
    payload: driverForEdit,
});

export const getDriverForEditFailure = error => ({
    type: GET_DRIVER_FOR_EDIT_FAILURE,
    payload: error,
});