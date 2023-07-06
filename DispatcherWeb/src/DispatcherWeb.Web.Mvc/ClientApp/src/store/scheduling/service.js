import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get page config
export const getPageConfig = () => get(url.GET_PAGE_CONFIG);

// get schedule trucks
export const getScheduleTrucks = filter => get(`${url.GET_SCHEDULE_TRUCKS}?officeId=${filter.officeId}&date=${filter.date}`);

// get schedule orders
export const getScheduleOrders = filter => get(`${url.GET_SCHEDULE_ORDERS}?officeId=${filter.officeId}&date=${filter.date}&hideCompletedOrders=${filter.hideCompletedOrders}&hideProgressBar=${filter.hideProgressBar}&sorting=${filter.sorting}`);