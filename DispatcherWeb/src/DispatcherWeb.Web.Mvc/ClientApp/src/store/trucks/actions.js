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
    GET_TRUCK_FOR_EDIT,
    GET_TRUCK_FOR_EDIT_SUCCESS,
    GET_TRUCK_FOR_EDIT_FAILURE,
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