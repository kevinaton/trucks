import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get order for edit
export const getOrderForEdit = () => get(url.GET_ORDER_FOR_EDIT);