import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get active customers select list
export const getActiveCustomersSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.term) {
        queryString = `term=${filter.term}${queryString}`;
    }

    return get(`${url.GET_ACTIVE_CUSTOMERS_SELECT_LIST}?${queryString}`);
};