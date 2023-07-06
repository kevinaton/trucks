import {
    GET_PAGE_CONFIG_SUCCESS,
    GET_PAGE_CONFIG_FAILURE,
    GET_SCHEDULE_TRUCKS_SUCCESS,
    GET_SCHEDULE_TRUCKS_FAILURE,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    schedulePageConfig: null,
    scheduleTrucks: null,
    scheduleOrders: null,
};

const SchedulingReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_PAGE_CONFIG_SUCCESS:
            return {
                ...state,
                schedulePageConfig: action.payload,
            };
        case GET_PAGE_CONFIG_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_SCHEDULE_TRUCKS_SUCCESS:
            return {
                ...state,
                scheduleTrucks: action.payload,
            };
        case GET_SCHEDULE_TRUCKS_FAILURE:
            return {
                ...state,
                scheduleTrucks: [],
                error: action.payload,
            };
        case GET_SCHEDULE_ORDERS_SUCCESS:
            return {
                ...state,
                scheduleOrders: action.payload,
            };
        case GET_SCHEDULE_ORDERS_FAILURE:
            return {
                ...state,
                scheduleOrders: [],
                error: action.payload,
            };
        default:
            return state;
    }
};

export default SchedulingReducer;