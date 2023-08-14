import {
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
    GET_BED_CONSTRUCTIONS,
    GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS,
    GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE,
    GET_BED_CONSTRUCTIONS_SUCCESS,
    GET_BED_CONSTRUCTIONS_FAILURE,
    GET_FUEL_TYPE_SELECT_LIST_SUCCESS,
    GET_FUEL_TYPE_SELECT_LIST_FAILURE,
    GET_ACTIVE_TRAILERS_SELECT_LIST,
    GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE,
    GET_ACTIVE_TRACTORS_SELECT_LIST,
    GET_ACTIVE_TRACTORS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_TRACTORS_SELECT_LIST_FAILURE,
    GET_MAKES_SELECT_LIST,
    GET_MAKES_SELECT_LIST_SUCCESS,
    GET_MAKES_SELECT_LIST_FAILURE,
    GET_MODELS_SELECT_LIST,
    GET_MODELS_SELECT_LIST_SUCCESS,
    GET_MODELS_SELECT_LIST_FAILURE,
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
    isLoadingBedConstructionsOpts: false,
    bedConstructions: null,
    fuelTypeSelectList: null,
    isLoadingActiveTrailersOpts: false,
    activeTrailersSelectList: null,
    isLoadingActiveTractorsOpts: false,
    activeTractorsSelectList: null,
    isLoadingMakesOpts: false,
    makesSelectList: null,
    isLoadingModelsOpts: false,
    modelsSelectList: null,
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
        case GET_BED_CONSTRUCTIONS:
            return {
                ...state,
                isLoadingBedConstructionsOpts: true
            };
        case GET_BED_CONSTRUCTIONS_SUCCESS:
            return {
                ...state,
                isLoadingBedConstructionsOpts: false,
                bedConstructions: action.payload
            };
        case GET_BED_CONSTRUCTIONS_FAILURE:
            return {
                ...state,
                isLoadingBedConstructionsOpts: false,
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
        case GET_ACTIVE_TRAILERS_SELECT_LIST:
            return {
                ...state,
                isLoadingActiveTrailersOpts: true
            };
        case GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingActiveTrailersOpts: false,
                activeTrailersSelectList: action.payload,  
            };
        case GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingActiveTrailersOpts: false,
                error: action.payload,
            };
        case GET_ACTIVE_TRACTORS_SELECT_LIST:
            return {
                ...state,
                isLoadingActiveTractorsOpts: true
            };
        case GET_ACTIVE_TRACTORS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingActiveTractorsOpts: false,
                activeTractorsSelectList: action.payload,
            };
        case GET_ACTIVE_TRACTORS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingActiveTractorsOpts: false,
                error: action.payload,
            };
        case GET_MAKES_SELECT_LIST:
            return {
                ...state,
                isLoadingMakesOpts: true
            };
        case GET_MAKES_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingMakesOpts: false,
                makesSelectList: action.payload,
            };
        case GET_MAKES_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingMakesOpts: false,
                error: action.payload,
            };
        case GET_MODELS_SELECT_LIST:
            return {
                ...state,
                isLoadingModelsOpts: true
            };
        case GET_MODELS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingModelsOpts: false,
                modelsSelectList: action.payload,
            };
        case GET_MODELS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingModelsOpts: false,
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
                editTruckSuccess: null,
                error: null
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