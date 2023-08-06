import {
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
    GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS,
    GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE,
    GET_BED_CONSTRUCTIONS_SUCCESS,
    GET_BED_CONSTRUCTIONS_FAILURE,
    GET_FUEL_TYPE_SELECT_LIST_SUCCESS,
    GET_FUEL_TYPE_SELECT_LIST_FAILURE,
    GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST_SUCCESS,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST_FAILURE,
    GET_TRUCK_FOR_EDIT_SUCCESS,
    GET_TRUCK_FOR_EDIT_FAILURE,
    EDIT_TRUCK_SUCCESS,
    EDIT_TRUCK_FAILURE,
    EDIT_TRUCK_RESET,
    SET_TRUCK_IS_OUT_OF_SERVICE_SUCCESS,
    SET_TRUCK_IS_OUT_OF_SERVICE_FAILURE,
    SET_TRUCK_IS_OUT_OF_SERVICE_RESET,
} from './actionTypes';

const INIT_STATE = {
    vehicleCategories: null,
    bedConstructionSelectList: null,
    bedConstructions: null,
    fuelTypeSelectList: null,
    activeTrailersSelectList: null,
    wialonDeviceTypesSelectList: null,
    truckForEdit: null,
    editTruckSuccess: null,
    setTruckIsOutOfServiceSuccess: null,
    setOutOfServiceResponse: null
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
        case GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS:
            return {
                ...state,
                bedConstructionSelectList: action.payload,
            };
        case GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_BED_CONSTRUCTIONS_SUCCESS:
            return {
                ...state,
                bedConstructions: action.payload
            };
        case GET_BED_CONSTRUCTIONS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        case GET_FUEL_TYPE_SELECT_LIST_SUCCESS:
            return {
                ...state,
                fuelTypeSelectList: action.payload,
            };
        case GET_FUEL_TYPE_SELECT_LIST_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                activeTrailersSelectList: action.payload,  
            };
        case GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_WIALON_DEVICE_TYPES_SELECT_LIST_SUCCESS:
            return {
                ...state,
                wialonDeviceTypesSelectList: action.payload,
            };
        case GET_WIALON_DEVICE_TYPES_SELECT_LIST_FAILURE:
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
        case EDIT_TRUCK_SUCCESS: 
            return {
                ...state,
                editTruckSuccess: true
            };
        case EDIT_TRUCK_FAILURE:
            return {
                ...state,
                editTruckSuccess: false,
                error: action.payload
            };
        case EDIT_TRUCK_RESET: 
            return {
                ...state,
                editTruckSuccess: null
            };
        case SET_TRUCK_IS_OUT_OF_SERVICE_SUCCESS: {
            const payload = action.payload;

            return {
                ...state,
                setTruckIsOutOfServiceSuccess: true,
                setOutOfServiceResponse: payload.isOutOfService ? payload.response : null
            };
        }
        case SET_TRUCK_IS_OUT_OF_SERVICE_FAILURE:
            return {
                ...state,
                setTruckIsOutOfServiceSuccess: false,
                error: action.payload
            };
        case SET_TRUCK_IS_OUT_OF_SERVICE_RESET:
            return {
                ...state,
                setTruckIsOutOfServiceSuccess: null,
                setOutOfServiceResponse: null
            };
        default:
            return state;
    }
};

export default TruckReducer;