import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get vehicle categories
export const getVehicleCategories = () => get(`${url.GET_VEHICLE_CATEGORIES}?maxResultCount=1000&skipCount=0`);
