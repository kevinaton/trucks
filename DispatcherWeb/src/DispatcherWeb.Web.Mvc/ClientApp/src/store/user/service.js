import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// get user info
export const getUserInfo = () => get(url.GET_CURRENT_LOGIN_INFO);

// get user setting
export const getUserSetting = settingName => get(`${url.GET_USER_SETTING}?settingName=${settingName}`);