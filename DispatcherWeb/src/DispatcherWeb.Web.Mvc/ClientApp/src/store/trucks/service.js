import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get vehicle categories
export const getVehicleCategories = () => get(`${url.GET_VEHICLE_CATEGORIES}?maxResultCount=1000&skipCount=0`);

// get bed construction select list
export const getBedConstructionSelectList = () => get(url.GET_BED_CONSTRUCTION_SELECT_LIST);

// get fuel type select list
export const getFuelTypeSelectList = () => get(url.GET_FUEL_TYPE_SELECT_LIST);

// get truck for edit
export const getTruckForEdit = () => get(url.GET_TRUCK_FOR_EDIT);
