import {
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST_SUCCESS,
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST_FAILURE
} from './actionTypes';

const INIT_STATE = {
    leaseHaulerDriversSelectList: null,
};

const LeaseHaulersReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_LEASE_HAULER_DRIVERS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                leaseHaulerDriversSelectList: action.payload,
            };
        case GET_LEASE_HAULER_DRIVERS_SELECT_LIST_FAILURE:
            return {
                ...state,
                leaseHaulerDriversSelectList: null,
                error: action.payload,
            };
        default:
            return state;
    }
};

export default LeaseHaulersReducer;