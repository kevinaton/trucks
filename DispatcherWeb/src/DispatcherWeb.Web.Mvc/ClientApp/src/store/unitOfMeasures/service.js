import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get unit of measures select list
export const getUnitOfMeasuresSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.term) {
        queryString = `term=${filter.term}&${queryString}`;
    }

    return get(`${url.GET_UNIT_OF_MEASURES_SELECT_LIST}?${queryString}`);
};