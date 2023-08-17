import {
    GET_ORDER_FOR_EDIT,
    GET_ORDER_FOR_EDIT_SUCCESS,
    GET_ORDER_FOR_EDIT_FAILURE,
} from './actionTypes';

export const getOrderForEdit = input => ({
    type: GET_ORDER_FOR_EDIT,
    payload: input
});

export const getOrderForEditSuccess = orderForEdit => ({
    type: GET_ORDER_FOR_EDIT_SUCCESS,
    payload: orderForEdit
});

export const getOrderForEditFailure = error => ({
    type: GET_ORDER_FOR_EDIT_FAILURE,
    payload: error
});