import {
    GET_ORDER_FOR_EDIT_SUCCESS,
    GET_ORDER_FOR_EDIT_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    orderForEdit: null
};

const OrderReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
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