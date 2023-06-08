import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// get schedule trucks
export const getScheduleTrucks = filter => get(`${url.GET_SCHEDULE_TRUCKS}?officeId=${filter.officeId}&date=${filter.date}`);

// get schedule orders
export const getScheduleOrders = filter => get(`${url.GET_SCHEDULE_ORDERS}?officeId=${filter.officeId}&date=${filter.date}&hideCompletedOrders=${filter.hideCompletedOrders}&hideProgressBar=${filter.hideProgressBar}&sorting=${filter.sorting}`);