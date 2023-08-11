import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get schedule trucks
export const getScheduleTrucks = filter => {
    let queryString = `officeId=${filter.officeId}&date=${encodeURIComponent(filter.date)}`;

    if (filter.truckIds) {
        queryString += `&truckIds=${filter.truckIds.join('&truckIds=')}`;
    }

    return get(`${url.GET_SCHEDULE_TRUCKS}?${queryString}`);
};

// get schedule orders
export const getScheduleOrders = filter => get(`${url.GET_SCHEDULE_ORDERS}?officeId=${filter.officeId}&date=${filter.date}&hideCompletedOrders=${filter.hideCompletedOrders}&hideProgressBar=${filter.hideProgressBar}&sorting=${filter.sorting}`);