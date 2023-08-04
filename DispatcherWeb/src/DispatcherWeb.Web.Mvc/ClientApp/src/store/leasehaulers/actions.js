import {
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST,
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST_SUCCESS,
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST_FAILURE
} from './actionTypes';

export const getLeaseHaulerDriversSelectList = filter => ({
    type: GET_LEASE_HAULER_DRIVERS_SELECT_LIST,
    payload: filter,
});

export const getLeaseHaulerDriversSelectListSuccess = leaseHaulerDriversSelectList => ({
    type: GET_LEASE_HAULER_DRIVERS_SELECT_LIST_SUCCESS,
    payload: leaseHaulerDriversSelectList,
});

export const getLeaseHaulerDriversSelectListFailure = error => ({
    type: GET_LEASE_HAULER_DRIVERS_SELECT_LIST_FAILURE,
    payload: error,
});