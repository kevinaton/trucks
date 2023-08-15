import { del } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// remove available lease hauler truck from schedule
export const removeAvailableLeaseHaulerTruckFromSchedule = filter => {
    let queryString = `truckId=${filter.truckId}&date=${encodeURIComponent(filter.date)}&officeId=${filter.officeId}`;
    return del(`${url.REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE}?${queryString}`);
};