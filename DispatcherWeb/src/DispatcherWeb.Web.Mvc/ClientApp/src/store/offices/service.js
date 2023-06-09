import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// get offices
export const getOffices = () => get(`${url.GET_OFFICES}?maxResultCount=20&skipCount=0`);