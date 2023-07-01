import { get, post } from '../../../../helpers/api_helper';
import * as url from '../../../../helpers/api_url_helper';

// get linked users
export const getLinkedUsers = () => get(url.GET_LINKED_USERS);

// link to user
export const linkToUser = user => post(url.LINK_TO_USER, user);

// unlink user
export const unlinkUser = linkedUser => post(url.UNLINK_USER, linkedUser);