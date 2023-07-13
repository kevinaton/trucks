import {
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
    GET_TRUCK_FOR_EDIT_SUCCESS,
    GET_TRUCK_FOR_EDIT_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    vehicleCategories: null,
    truckForEdit: null
};

const TruckReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_VEHICLE_CATEGORIES_SUCCESS:
            return {
                ...state,
                vehicleCategories: action.payload,
            };
        case GET_VEHICLE_CATEGORIES_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_TRUCK_FOR_EDIT_SUCCESS:
            return {
                ...state,
                truckForEdit: action.payload,
            };
        case GET_TRUCK_FOR_EDIT_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        default:
            return state;
    }
};

export default TruckReducer;