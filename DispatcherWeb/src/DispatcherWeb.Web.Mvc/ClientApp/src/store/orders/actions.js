import {
    GET_ORDER_PRIORITY_SELECT_LIST,
    GET_ORDER_PRIORITY_SELECT_LIST_SUCCESS,
    GET_ORDER_PRIORITY_SELECT_LIST_FAILURE,
    GET_ORDER_FOR_EDIT,
    GET_ORDER_FOR_EDIT_SUCCESS,
    GET_ORDER_FOR_EDIT_FAILURE,
} from './actionTypes';

export const getOrderPrioritySelectList = () => ({
    type: GET_ORDER_PRIORITY_SELECT_LIST
});

export const getOrderPrioritySelectListSuccess = orderPrioritySelectList => ({
    type: GET_ORDER_PRIORITY_SELECT_LIST_SUCCESS,
    payload: orderPrioritySelectList
});

export const getOrderPrioritySelectListFailure = error => ({
    type: GET_ORDER_PRIORITY_SELECT_LIST_FAILURE,
    payload: error
});

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