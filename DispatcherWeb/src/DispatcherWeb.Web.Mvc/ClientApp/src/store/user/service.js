import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get user info
export const getUserInfo = () => get(url.GET_CURRENT_LOGIN_INFO);

// get user profile menu
export const getUserProfileMenu = () => get(url.GET_CURRENT_USER_LOGIN_INFO);

// get user setting
export const getUserSetting = settingName => get(`${url.GET_USER_SETTING}?settingName=${settingName}`);