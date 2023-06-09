import {
    GET_SCHEDULE_TRUCKS_SUCCESS,
    GET_SCHEDULE_TRUCKS_FAILURE,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    scheduleTrucks: null,
    scheduleOrders: null,
};

const SchedulingReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_SCHEDULE_TRUCKS_SUCCESS:
            return {
                ...state,
                scheduleTrucks: action.payload,
            };
        case GET_SCHEDULE_TRUCKS_FAILURE:
            return {
                ...state,
                scheduleTrucks: [],
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
            };
        default:
            return state;
    }
};

export default SchedulingReducer;