import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get scheduled truck count partial view
export const getScheduledTruckCountPartialView = () => get(url.GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW);