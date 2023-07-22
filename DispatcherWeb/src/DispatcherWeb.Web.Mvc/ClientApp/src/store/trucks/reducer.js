import { isEmpty } from 'lodash';
import {
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
    GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS,
    GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE,
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
} from './actionTypes';

const INIT_STATE = {
    vehicleCategories: null,
    bedConstructionSelectList: null,
    fuelTypeSelectList: null,
    activeTrailersSelectList: null,
    wialonDeviceTypesSelectList: null,
    truckForEdit: null,
    editSuccess: null
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
            }
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
        case EDIT_TRUCK_SUCCESS: {
            const { response, data } = action.payload;
            if (!isEmpty(response) && response.success && 
                !isEmpty(data)
            ) {
                let newData = data;
                const { result } = response;
                newData.id = result.id;

                console.log('newData: ', newData);

                return {
                    ...state,
                    editSuccess: true
                };
            }

            return {
                ...state,
                editSuccess: false
            };
        }
        default:
            return state;
    }
};

export default TruckReducer;