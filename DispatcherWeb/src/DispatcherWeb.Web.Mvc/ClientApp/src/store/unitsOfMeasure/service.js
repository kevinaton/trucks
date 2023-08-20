import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get unit of measures select list
export const getUnitsOfMeasureSelectList = filter => {
    let queryString = `maxResultCount=${filter.maxResultCount}&skipCount=${filter.skipCount}`;

    if (filter.term) {
        queryString = `term=${filter.term}&${queryString}`;
    }

    return get(`${url.GET_UNITS_OF_MEASURE_SELECT_LIST}?${queryString}`);
};