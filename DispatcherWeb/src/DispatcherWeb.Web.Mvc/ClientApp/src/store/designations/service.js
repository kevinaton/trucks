import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get designations select list
export const getDesignationsSelectList = () => {
    return get(`${url.GET_DESIGNATIONS_SELECT_LIST}`);
};