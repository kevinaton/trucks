import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get order priority select list
export const getOrderPrioritySelectList = () => get(url.GET_ORDER_PRIORITY_SELECT_LIST);

// get order for edit
export const getOrderForEdit = () => get(url.GET_ORDER_FOR_EDIT);