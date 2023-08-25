import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get order priority select list
export const getOrderPrioritySelectList = () => get(url.GET_ORDER_PRIORITY_SELECT_LIST);

// get job for edit
export const getJobForEdit = input => get(url.GET_JOB_FOR_EDIT, input);

// get order for edit
export const getOrderForEdit = () => get(url.GET_ORDER_FOR_EDIT);