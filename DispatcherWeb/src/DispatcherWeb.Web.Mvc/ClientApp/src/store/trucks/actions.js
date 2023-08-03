import {
    GET_VEHICLE_CATEGORIES,
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
    GET_BED_CONSTRUCTION_SELECT_LIST,
    GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS,
    GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE,
    GET_FUEL_TYPE_SELECT_LIST,
    GET_FUEL_TYPE_SELECT_LIST_SUCCESS,
    GET_FUEL_TYPE_SELECT_LIST_FAILURE,
    GET_ACTIVE_TRAILERS_SELECT_LIST,
    GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS,
    GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE,
    GET_TRUCK_FOR_EDIT,
    GET_TRUCK_FOR_EDIT_SUCCESS,
    GET_TRUCK_FOR_EDIT_FAILURE,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST_SUCCESS,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST_FAILURE,
    EDIT_TRUCK,
    EDIT_TRUCK_SUCCESS,
    EDIT_TRUCK_FAILURE,
    EDIT_TRUCK_RESET,
    SET_TRUCK_IS_OUT_OF_SERVICE,
    SET_TRUCK_IS_OUT_OF_SERVICE_SUCCESS,
    SET_TRUCK_IS_OUT_OF_SERVICE_FAILURE,
    SET_TRUCK_IS_OUT_OF_SERVICE_RESET,
} from './actionTypes';

export const getVehicleCategories = () => ({
    type: GET_VEHICLE_CATEGORIES,
});

export const getVehicleCategoriesSuccess = vehicleCategories => ({
    type: GET_VEHICLE_CATEGORIES_SUCCESS,
    payload: vehicleCategories,
});

export const getVehicleCategoriesFailure = error => ({
    type: GET_VEHICLE_CATEGORIES_FAILURE,
    payload: error,
});

export const getBedConstructionSelectList = () => ({
    type: GET_BED_CONSTRUCTION_SELECT_LIST
});

export const getBedConstructionSelectListSuccess = bedConstructionSelectList => ({
    type: GET_BED_CONSTRUCTION_SELECT_LIST_SUCCESS,
    payload: bedConstructionSelectList,
});

export const getBedConstructionSelectListFailure = error => ({
    type: GET_BED_CONSTRUCTION_SELECT_LIST_FAILURE,
    payload: error,
});

export const getFuelTypeSelectList = () => ({
    type: GET_FUEL_TYPE_SELECT_LIST,
});

export const getFuelTypeSelectListSuccess = fuelTypeSelectList => ({
    type: GET_FUEL_TYPE_SELECT_LIST_SUCCESS,
    payload: fuelTypeSelectList,
});

export const getFuelTypeSelectListFailure = error => ({
    type: GET_FUEL_TYPE_SELECT_LIST_FAILURE,
    payload: error,
});

export const getActiveTrailersSelectList = () => ({
    type: GET_ACTIVE_TRAILERS_SELECT_LIST,
});

export const getActiveTrailersSelectListSuccess = activeTrailersSelectList => ({
    type: GET_ACTIVE_TRAILERS_SELECT_LIST_SUCCESS,
    payload: activeTrailersSelectList,
});

export const getActiveTrailersSelectListFailure = error => ({
    type: GET_ACTIVE_TRAILERS_SELECT_LIST_FAILURE,
    payload: error,
});

export const getTruckForEdit = input => ({
    type: GET_TRUCK_FOR_EDIT,
    payload: input,
});

export const getTruckForEditSuccess = truckForEdit => ({
    type: GET_TRUCK_FOR_EDIT_SUCCESS,
    payload: truckForEdit,
});

export const getTruckForEditFailure = error => ({
    type: GET_TRUCK_FOR_EDIT_FAILURE,
    payload: error,
});

export const getWialonDeviceTypesSelectList = () => ({
    type: GET_WIALON_DEVICE_TYPES_SELECT_LIST,
});

export const getWialonDeviceTypesSelectListSuccess = wialonDeviceTypesSelectList => ({
    type: GET_WIALON_DEVICE_TYPES_SELECT_LIST_SUCCESS,
    payload: wialonDeviceTypesSelectList,
});

export const getWialonDeviceTypesSelectListFailure = error => ({
    type: GET_WIALON_DEVICE_TYPES_SELECT_LIST_FAILURE,
    payload: error,
});

export const editTruck = truck => ({
    type: EDIT_TRUCK,
    payload: truck
});

export const editTruckSuccess = response => ({
    type: EDIT_TRUCK_SUCCESS,
    payload: response
});

export const editTruckFailure = error => ({
    type: EDIT_TRUCK_FAILURE,
    payload: error,
});

export const resetEditTruck = () => ({
    type: EDIT_TRUCK_RESET
});

export const setTruckIsOutOfService = truck => ({
    type: SET_TRUCK_IS_OUT_OF_SERVICE,
    payload: truck
});

export const setTruckIsOutOfServiceSuccess = (response, isOutOfService) => ({
    type: SET_TRUCK_IS_OUT_OF_SERVICE_SUCCESS,
    payload: {
        response, 
        isOutOfService
    }
});

export const setTruckIsOutOfServiceFailure = error => ({
    type: SET_TRUCK_IS_OUT_OF_SERVICE_FAILURE,
    payload: error,
});

export const resetSetTruckIsOutOfService = () => ({
    type: SET_TRUCK_IS_OUT_OF_SERVICE_RESET
});