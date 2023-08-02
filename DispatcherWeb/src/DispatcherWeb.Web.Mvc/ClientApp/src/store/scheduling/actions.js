import {
    GET_PAGE_CONFIG,
    GET_PAGE_CONFIG_SUCCESS,
    GET_PAGE_CONFIG_FAILURE,
    GET_SCHEDULE_TRUCKS,
    GET_SCHEDULE_TRUCKS_SUCCESS,
    GET_SCHEDULE_TRUCKS_FAILURE,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_SUCCESS,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_FAILURE,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_RESET,
    GET_SCHEDULE_ORDERS,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
    REMOVE_TRUCK_FROM_SCHEDULE,
    REMOVE_TRUCK_FROM_SCHEDULE_SUCCESS,
    REMOVE_TRUCK_FROM_SCHEDULE_FAILURE,
} from './actionTypes';

export const getSchedulePageConfig = () => ({
    type: GET_PAGE_CONFIG,
});

export const getSchedulePageConfigSuccess = pageConfig => ({
    type: GET_PAGE_CONFIG_SUCCESS,
    payload: pageConfig,
});

export const getSchedulePageConfigFailure = error => ({
    type: GET_PAGE_CONFIG_FAILURE,
    payload: error,
});

export const getScheduleTrucks = filter => ({
    type: GET_SCHEDULE_TRUCKS,
    payload: filter,
});

export const getScheduleTrucksSuccess = scheduleTrucks => ({
    type: GET_SCHEDULE_TRUCKS_SUCCESS,
    payload: scheduleTrucks,
});

export const getScheduleTrucksFailure = error => ({
    type: GET_SCHEDULE_TRUCKS_FAILURE,
    payload: error,
});

export const getScheduleTruckBySyncRequest = filter => ({
    type: GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST,
    payload: filter,
});

export const getScheduleTruckBySyncRequestSuccess = scheduleTruck => ({
    type: GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_SUCCESS,
    payload: scheduleTruck,
});

export const getScheduleTruckBySyncRequestFailure = error => ({
    type: GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_FAILURE,
    payload: error,
});

export const getScheduleTruckBySyncRequestReset = () => ({
    type: GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_RESET,
});

export const getScheduleOrders = filter => ({
    type: GET_SCHEDULE_ORDERS,
    payload: filter,
});

export const getScheduleOrdersSuccess = scheduleOrders => ({
    type: GET_SCHEDULE_ORDERS_SUCCESS,
    payload: scheduleOrders,
});

export const getScheduleOrdersFailure = error => ({
    type: GET_SCHEDULE_ORDERS_FAILURE,
    payload: error,
});

export const removeTruckFromSchedule = truckIds => ({
    type: REMOVE_TRUCK_FROM_SCHEDULE,
    payload: truckIds,
});