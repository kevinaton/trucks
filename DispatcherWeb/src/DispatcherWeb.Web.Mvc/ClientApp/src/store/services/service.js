import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get services with tax info select list
export const getServicesWithTaxInfoSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.term) {
        queryString = `term=${filter.term}&${queryString}`;
    }

    return get(`${url.GET_SERVICES_WITH_TAX_INFO_SELECT_LIST}?${queryString}`);
};