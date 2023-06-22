import { get, post } from '../../../../helpers/api_helper';
import { rootPost } from '../../../../helpers/root_api_helper';
import * as url from '../../../../helpers/url_helper';

// get user profile settings
export const getUserProfileSettings = () => get(url.GET_USER_PROFILE_SETTINGS);

// change password
export const changePassword = password => post(url.CHANGE_PASSWORD, password);

// upload profile picture file
export const uploadProfilePictureFile = file => rootPost(url.UPLOAD_PROFILE_PICTURE_FILE, file);