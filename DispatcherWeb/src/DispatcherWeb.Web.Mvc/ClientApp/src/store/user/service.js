import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// get user info
export const getUserInfo = () => get(url.GET_CURRENT_LOGIN_INFO);

// get user settings
export const getUserSettings = settingName => get(`${url.GET_USER_SETTINGS}?settingName=${settingName}`);