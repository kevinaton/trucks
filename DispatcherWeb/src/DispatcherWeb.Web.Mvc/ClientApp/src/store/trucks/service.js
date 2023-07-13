import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get vehicle categories
export const getVehicleCategories = () => get(`${url.GET_VEHICLE_CATEGORIES}?maxResultCount=1000&skipCount=0`);

// get truck for edit
export const getTruckForEdit = () => get(url.GET_TRUCK_FOR_EDIT);
