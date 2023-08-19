import {
    GET_UNIT_OF_MEASURES_SELECT_LIST,
    GET_UNIT_OF_MEASURES_SELECT_LIST_SUCCESS,
    GET_UNIT_OF_MEASURES_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getUnitOfMeasuresSelectList = filter => ({
    type: GET_UNIT_OF_MEASURES_SELECT_LIST,
    payload: filter
});

export const getUnitOfMeasuresSelectListSuccess = unitOfMeasuresSelectList => ({
    type: GET_UNIT_OF_MEASURES_SELECT_LIST_SUCCESS,
    payload: unitOfMeasuresSelectList
});

export const getUnitOfMeasuresSelectListFailure = error => ({
    type: GET_UNIT_OF_MEASURES_SELECT_LIST_FAILURE,
    payload: error
});