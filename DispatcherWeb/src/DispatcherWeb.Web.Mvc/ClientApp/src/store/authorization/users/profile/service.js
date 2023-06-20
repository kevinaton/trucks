import { get } from '../../../../helpers/api_helper';
import * as url from '../../../../helpers/url_helper';

// get user profile settings
export const getUserProfileSettings = () => get(url.GET_USER_PROFILE_SETTINGS);