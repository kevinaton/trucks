import {
    GET_VEHICLE_CATEGORIES,
    GET_VEHICLE_CATEGORIES_SUCCESS,
    GET_VEHICLE_CATEGORIES_FAILURE,
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