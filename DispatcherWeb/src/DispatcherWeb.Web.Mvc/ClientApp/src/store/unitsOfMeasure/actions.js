import {
    GET_UNITS_OF_MEASURE_SELECT_LIST,
    GET_UNITS_OF_MEASURE_SELECT_LIST_SUCCESS,
    GET_UNITS_OF_MEASURE_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getUnitsOfMeasureSelectList = filter => ({
    type: GET_UNITS_OF_MEASURE_SELECT_LIST,
    payload: filter
});

export const getUnitsOfMeasureSelectListSuccess = unitsOfMeasureSelectList => ({
    type: GET_UNITS_OF_MEASURE_SELECT_LIST_SUCCESS,
    payload: unitsOfMeasureSelectList
});

export const getUnitsOfMeasureSelectListFailure = error => ({
    type: GET_UNITS_OF_MEASURE_SELECT_LIST_FAILURE,
    payload: error
});