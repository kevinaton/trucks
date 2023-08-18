import {
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST,
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_SUCCESS,
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingServicesWithTaxInfoOpts: false,
    servicesWithTaxInfoSelectList: null,
};

const ServiceReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_SERVICES_WITH_TAX_INFO_SELECT_LIST:
            return {
                ...state,
                isLoadingServicesWithTaxInfoOpts: true
            }
        case GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingServicesWithTaxInfoOpts: false,
                servicesWithTaxInfoSelectList: action.payload
            };
        case GET_SERVICES_WITH_TAX_INFO_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingServicesWithTaxInfoOpts: false,
                error: action.payload
            };
        default: 
            return state;
    }
};

export default ServiceReducer;