import {
    GET_LOCATIONS_SELECT_LIST,
    GET_LOCATIONS_SELECT_LIST_SUCCESS,
    GET_LOCATIONS_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getLocationsSelectList = filter => ({
    type: GET_LOCATIONS_SELECT_LIST,
    payload: filter
});

export const getLocationsSelectListSuccess = locationsSelectList => ({
    type: GET_LOCATIONS_SELECT_LIST_SUCCESS,
    payload: locationsSelectList
});

export const getLocationsSelectListFailure = error => ({
    type: GET_LOCATIONS_SELECT_LIST_FAILURE,
    payload: error
});