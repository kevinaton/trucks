import {
    GET_ORDER_PRIORITY_SELECT_LIST,
    GET_ORDER_PRIORITY_SELECT_LIST_SUCCESS,
    GET_ORDER_PRIORITY_SELECT_LIST_FAILURE,
    GET_ORDER_FOR_EDIT_SUCCESS,
    GET_ORDER_FOR_EDIT_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingOrderPriorityOpts: false,
    orderPrioritySelectList: null,
    orderForEdit: null
};

const OrderReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_ORDER_PRIORITY_SELECT_LIST:
            return {
                ...state,
                isLoadingOrderPriorityOpts: true
            };
        case GET_ORDER_PRIORITY_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingOrderPriorityOpts: false,
                orderPrioritySelectList: action.payload
            };
        case GET_ORDER_PRIORITY_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingOrderPriorityOpts: false,
                error: action.payload
            };
        case GET_ORDER_FOR_EDIT_SUCCESS:
            return {
                ...state,
                orderForEdit: action.payload
            }
        case GET_ORDER_FOR_EDIT_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state;
    }
};

export default OrderReducer;