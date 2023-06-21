import { get, post } from '../../../../helpers/api_helper';
import * as url from '../../../../helpers/url_helper';

// get linked users
export const getLinkedUsers = () => get(url.GET_LINKED_USERS);

// unlink user
export const unlinkUser = linkedUser => post(url.UNLINK_USER, linkedUser);