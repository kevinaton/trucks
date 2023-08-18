import {
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST,
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_SUCCESS,
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_FAILURE,
} from './actionTypes';

export const getServicesWithTaxInfoSelectList = filter => ({
    type: GET_SERVICES_WITH_TAX_INFO_SELECT_LIST,
    payload: filter
});

export const getServicesWithTaxInfoSelectListSuccess = servicesWithTaxInfoSelectList => ({
    type: GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_SUCCESS,
    payload: servicesWithTaxInfoSelectList
});

export const getServicesWithTaxInfoSelectListFailure = error => ({
    type: GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_FAILURE,
    payload: error
});