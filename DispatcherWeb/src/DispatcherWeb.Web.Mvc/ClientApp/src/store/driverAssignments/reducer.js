import {
    SET_DRIVER_FOR_TRUCK_SUCCESS,
    SET_DRIVER_FOR_TRUCK_FAILURE,
    SET_DRIVER_FOR_TRUCK_RESET,
    HAS_ORDER_LINE_TRUCKS_SUCCESS,
    HAS_ORDER_LINE_TRUCKS_FAILURE,
    HAS_ORDER_LINE_TRUCKS_RESET
} from './actionTypes';

const INIT_STATE = {
    setDriverForTruckSuccess: null,
    hasOrderLineTrucksResponse: null
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
        case HAS_ORDER_LINE_TRUCKS_SUCCESS:
            {
                const { truckId, response } = action.payload;
                return {
                    ...state,
                    hasOrderLineTrucksResponse: {
                        truckId,
                        response
                    },
                };
            }
        case HAS_ORDER_LINE_TRUCKS_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case HAS_ORDER_LINE_TRUCKS_RESET:
            return {
                ...state,
                hasOrderLineTrucksResponse: null,
            };
        default:
            return state;
    }
};

export default DriverAssignmentReducer;