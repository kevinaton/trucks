import { get, post, put } from '../../../../helpers/api_helper';
import { rootPost } from '../../../../helpers/root_api_helper';
import * as url from '../../../../helpers/url_helper';

// get user profile settings
export const getUserProfileSettings = () => get(url.GET_USER_PROFILE_SETTINGS);

// update user profile
export const updateUserProfile = userProfile => put(url.UPDATE_USER_PROFILE, userProfile);

// change password
export const changePassword = password => post(url.CHANGE_PASSWORD, password);

// upload profile picture file
export const uploadProfilePictureFile = file => rootPost(url.UPLOAD_PROFILE_PICTURE_FILE, file);

// enable google authenticator
export const enableGoogleAuthenticator = () => put(url.ENABLE_GOOGLE_AUTHENTICATOR);
export const disableGoogleAuthenticator = () => post(url.DISABLE_GOOGLE_AUTHENTICATOR);