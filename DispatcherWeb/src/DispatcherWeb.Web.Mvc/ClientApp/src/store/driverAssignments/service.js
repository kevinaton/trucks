import { post } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// set driver for truck
export const setDriverForTruck = driverAssignment => post(url.SET_DRIVER_FOR_TRUCK, driverAssignment);

// has order line trucks
export const hasOrderLineTrucks = filter => post(url.HAS_ORDER_LINE_TRUCKS, filter);