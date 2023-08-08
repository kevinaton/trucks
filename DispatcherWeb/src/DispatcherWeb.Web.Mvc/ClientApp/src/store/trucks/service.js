import { get, post } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get vehicle categories
export const getVehicleCategories = filter => {
    let queryString = `maxResultCount=1000&skipCount=0`;

    if (filter.assetType !== null && filter.assetType !== undefined) {
        queryString = `assetType=${filter.assetType}&${queryString}`;
    }

    return get(`${url.GET_VEHICLE_CATEGORIES}?${queryString}`);
};

// get bed construction select list
export const getBedConstructionSelectList = () => get(url.GET_BED_CONSTRUCTION_SELECT_LIST);

// get bed constructions
export const getBedConstructions = filter => {
    return get(`${url.GET_BED_CONSTRUCTIONS}?vehicleCategoryId=${filter.vehicleCategoryId}`)
};

// get fuel type select list
export const getFuelTypeSelectList = () => get(url.GET_FUEL_TYPE_SELECT_LIST);

// get active trailers select list
export const getActiveTrailersSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;
    
    if (filter.id !== null && filter.id !== undefined) {
        queryString = `id=${filter.id}&${queryString}`;
    }

    if (filter.model !== null && filter.model !== undefined) {
        queryString = `model=${filter.model}&${queryString}`;
    }

    if (filter.make !== null && filter.make !== undefined) {
        queryString = `make=${filter.make}&${queryString}`;
    }

    if (filter.bedConstruction !== null && filter.bedConstruction !== undefined) {
        queryString = `bedConstruction=${filter.bedConstruction}&${queryString}`;
    }

    if (filter.vehicleCategoryId !== null && filter.vehicleCategoryId !== undefined) {
        queryString = `vehicleCategoryId=${filter.vehicleCategoryId}&${queryString}`;
    }

    return get(`${url.GET_ACTIVE_TRAILERS_SELECT_LIST}?${queryString}`);
};

// get makes select list
export const getMakesSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.vehicleCategoryId !== null && filter.vehicleCategoryId !== undefined) {
        queryString = `vehicleCategoryId=${filter.vehicleCategoryId}&${queryString}`;
    }

    return get(`${url.GET_MAKES_SELECT_LIST}?${queryString}`);
};

// get models select list
export const getModelsSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.make !== null && filter.make !== undefined) {
        queryString = `make=${filter.make}&${queryString}`;
    }

    if (filter.vehicleCategoryId !== null && filter.vehicleCategoryId !== undefined) {
        queryString = `vehicleCategoryId=${filter.vehicleCategoryId}&${queryString}`;
    }

    return get(`${url.GET_MODELS_SELECT_LIST}?${queryString}`);
};

// get truck for edit
export const getTruckForEdit = () => get(url.GET_TRUCK_FOR_EDIT);

// get wialon device types select list
export const getWialonDeviceTypesSelectList = () => get(`${url.GET_WIALON_DEVICE_TYPES_SELECT_LIST}?maxResultCount=1000&skipCount=0`);

// edit truck
export const editTruck = truck => post(url.EDIT_TRUCK, truck);

// set truck is out of service
export const setTruckIsOutOfService = truck => post(url.SET_TRUCK_IS_OUT_OF_SERVICE, truck);