import { get } from '../../../../helpers/api_helper';
import * as url from '../../../../helpers/url_helper';

// get linked users
export const getLinkedUsers = () => get(url.GET_LINKED_USERS);