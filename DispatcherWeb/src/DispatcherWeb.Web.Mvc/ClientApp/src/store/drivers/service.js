import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get drivers select list
export const getDriversSelectList = () => get(`${url.GET_DRIVERS_SELECT_LIST}?includeLeaseHaulerDrivers=false&maxResultCount=1000&skipCount=0`);