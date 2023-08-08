import {
    SET_DRIVER_FOR_TRUCK,
    SET_DRIVER_FOR_TRUCK_SUCCESS,
    SET_DRIVER_FOR_TRUCK_FAILURE,
    SET_DRIVER_FOR_TRUCK_RESET,
    HAS_ORDER_LINE_TRUCKS,
    HAS_ORDER_LINE_TRUCKS_SUCCESS,
    HAS_ORDER_LINE_TRUCKS_FAILURE,
    HAS_ORDER_LINE_TRUCKS_RESET
} from './actionTypes';

export const setDriverForTruck = driverAssignment => ({
    type: SET_DRIVER_FOR_TRUCK,
    payload: driverAssignment,
});

export const setDriverForTruckSuccess = response => ({
    type: SET_DRIVER_FOR_TRUCK_SUCCESS,
    payload: response
});

export const setDriverForTruckFailure = error => ({
    type: SET_DRIVER_FOR_TRUCK_FAILURE,
    payload: error,
});

export const setDriverForTruckReset = () => ({
    type: SET_DRIVER_FOR_TRUCK_RESET
});

export const hasOrderLineTrucks = filter => ({
    type: HAS_ORDER_LINE_TRUCKS,
    payload: filter,
});

export const hasOrderLineTrucksSuccess = response => ({
    type: HAS_ORDER_LINE_TRUCKS_SUCCESS,
    payload: response
});

export const hasOrderLineTrucksFailure = error => ({
    type: HAS_ORDER_LINE_TRUCKS_FAILURE,
    payload: error,
});

export const hasOrderLineTrucksReset = () => ({
    type: HAS_ORDER_LINE_TRUCKS_RESET
});