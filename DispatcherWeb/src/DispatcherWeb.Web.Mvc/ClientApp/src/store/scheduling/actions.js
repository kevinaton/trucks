import {
    GET_SCHEDULE_TRUCKS,
    GET_SCHEDULE_TRUCKS_SUCCESS,
    GET_SCHEDULE_TRUCKS_FAILURE,
    GET_SCHEDULE_ORDERS,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
} from './actionTypes';

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