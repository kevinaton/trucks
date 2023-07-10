import {
    GET_OFFICES,
    GET_OFFICES_SUCCESS,
    GET_OFFICES_FAILURE,
} from './actionTypes';

export const getOffices = () => ({
    type: GET_OFFICES
});

export const getOfficesSuccess = offices => ({
    type: GET_OFFICES_SUCCESS,
    payload: offices,
});

export const getOfficesFailure = error => ({
    type: GET_OFFICES_FAILURE,
    payload: error,
});