import {
    GET_ACTIVE_CUSTOMERS_SELECT_LIST,
    GET_ACTIVE_CUSTOMERS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_CUSTOMERS_SELECT_LIST_FAILURE
} from './actionTypes';

const INIT_STATE = {
    isLoadingActiveCustomersOpts: false,
    activeCustomersSelectList: null,
};

const CustomerReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_ACTIVE_CUSTOMERS_SELECT_LIST:
            return {
                ...state,
                isLoadingActiveCustomersOpts: true
            };
        case GET_ACTIVE_CUSTOMERS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingActiveCustomersOpts: false,
                activeCustomersSelectList: action.payload
            };
        case GET_ACTIVE_CUSTOMERS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingActiveCustomersOpts: false,
                error: action.payload
            };
        default:
            return state;
    }
};

export default CustomerReducer;