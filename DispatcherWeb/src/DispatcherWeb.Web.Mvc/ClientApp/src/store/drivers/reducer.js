import {
    GET_DRIVERS_SELECT_LIST_SUCCESS,
    GET_DRIVERS_SELECT_LIST_FAILURE,
    GET_DRIVER_FOR_EDIT_SUCCESS,
    GET_DRIVER_FOR_EDIT_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    driversSelectList: null,
    driverForEdit: null,
};

const DriverReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_DRIVERS_SELECT_LIST_SUCCESS: 
            return {
                ...state,
                driversSelectList: action.payload,
            };
        case GET_DRIVERS_SELECT_LIST_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_DRIVER_FOR_EDIT_SUCCESS:
            return {
                ...state,
                driverForEdit: action.payload,
            };
        case GET_DRIVER_FOR_EDIT_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        default:
            return state;
    }
};

export default DriverReducer;