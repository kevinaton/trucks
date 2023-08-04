import {
    SET_DRIVER_FOR_TRUCK_SUCCESS,
    SET_DRIVER_FOR_TRUCK_FAILURE,
    SET_DRIVER_FOR_TRUCK_RESET
} from './actionTypes';

const INIT_STATE = {
    setDriverForTruckSuccess: null,
};

const DriverAssignmentReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case SET_DRIVER_FOR_TRUCK_SUCCESS:
            return {
                ...state,
                setDriverForTruckSuccess: true,
            };
        case SET_DRIVER_FOR_TRUCK_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case SET_DRIVER_FOR_TRUCK_RESET:
            return {
                ...state,
                setDriverForTruckSuccess: null,
            };
        default:
            return state;
    }
};

export default DriverAssignmentReducer;