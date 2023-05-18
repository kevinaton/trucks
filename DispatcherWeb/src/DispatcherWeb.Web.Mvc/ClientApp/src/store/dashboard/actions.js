import {
    GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW,
    GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_SUCCESS,
    GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_FAILURE
} from './actionTypes'

export const getScheduledTruckCountPartialView = () => ({
    type: GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW
})

export const getScheduledTruckCountPartialViewSuccess = partialView => ({
    type: GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_SUCCESS,
    payload: partialView
})

export const getScheduledTruckCountPartialViewFailure = error => ({
    type: GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_FAILURE,
    payload: error
})