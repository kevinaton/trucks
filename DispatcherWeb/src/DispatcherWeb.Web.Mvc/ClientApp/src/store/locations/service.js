import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get locations select list
export const getLocationsSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.term) {
        queryString = `term=${filter.term}&${queryString}`;
    }

    return get(`${url.GET_LOCATIONS_SELECT_LIST}?${queryString}`);
};