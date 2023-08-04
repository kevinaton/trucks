import {
    SET_DRIVER_FOR_TRUCK,
    SET_DRIVER_FOR_TRUCK_SUCCESS,
    SET_DRIVER_FOR_TRUCK_FAILURE,
    SET_DRIVER_FOR_TRUCK_RESET
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