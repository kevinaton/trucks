import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get lease hauler drivers select list
export const getLeaseHaulerDriversSelectList = filter => get(`${url.GET_LEASE_HAULER_DRIVERS_SELECT_LIST}?leaseHaulerId=${filter.leaseHaulerId}`);