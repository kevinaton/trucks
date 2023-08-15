import {
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE,
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_SUCCESS,
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_FAILURE,
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_RESET,
} from './actionTypes';

export const removeAvailableLeaseHaulerTruckFromSchedule = (truckId, filter) => ({
    type: REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE,
    payload: {
        truckId,
        filter
    },
});

export const removeAvailableLeaseHaulerTruckFromScheduleSuccess = response => ({
    type: REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_SUCCESS,
    payload: response,
});

export const removeAvailableLeaseHaulerTruckFromScheduleFailure = error => ({
    type: REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_FAILURE,
    payload: error,
});

export const removeAvailableLeaseHaulerTruckFromScheduleReset = () => ({
    type: REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_RESET,
});