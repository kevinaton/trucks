import {
    GET_DESIGNATIONS_SELECT_LIST,
    GET_DESIGNATIONS_SELECT_LIST_SUCCESS,
    GET_DESIGNATIONS_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getDesignationsSelectList = () => ({
    type: GET_DESIGNATIONS_SELECT_LIST,
});

export const getDesignationsSelectListSuccess = designationsSelectList => ({
    type: GET_DESIGNATIONS_SELECT_LIST_SUCCESS,
    payload: designationsSelectList,
});

export const getDesignationsSelectListFailure = error => ({
    type: GET_DESIGNATIONS_SELECT_LIST_FAILURE,
    payload: error,
});