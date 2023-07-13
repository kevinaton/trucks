import {
    GET_VEHICLE_CATEGORIES,
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
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