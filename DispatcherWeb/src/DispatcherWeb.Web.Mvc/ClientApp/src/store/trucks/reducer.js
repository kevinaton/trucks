import {
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    vehicleCategories: null,
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
        default:
            return state;
    }
};

export default TruckReducer;