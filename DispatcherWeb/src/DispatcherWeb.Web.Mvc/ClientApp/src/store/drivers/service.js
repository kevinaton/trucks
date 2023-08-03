import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get drivers select list
export const getDriversSelectList = filter => {
    return get(`${url.GET_DRIVERS_SELECT_LIST}?includeLeaseHaulerDrivers=${filter.includeLeaseHaulerDrivers}&maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`);
};

// get driver for edit
export const getDriverForEdit = () => get(url.GET_DRIVER_FOR_EDIT);