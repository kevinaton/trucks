import { get, post, put } from '../../../../helpers/api_helper';
import { rootPost } from '../../../../helpers/root_api_helper';
import * as url from '../../../../helpers/url_helper';

// get user profile settings
export const getUserProfileSettings = () => get(url.GET_USER_PROFILE_SETTINGS);

// update user profile
export const updateUserProfile = userProfile => put(url.UPDATE_USER_PROFILE, userProfile);

// update profile picture
export const updateProfilePicture = profilePicture => put(url.UPDATE_PROFILE_PICTURE, profilePicture);

// update signature picture
export const updateSignaturePicture = signaturePicture => put(url.UPDATE_SIGNATURE_PICTURE, signaturePicture);

// change password
export const changePassword = password => post(url.CHANGE_PASSWORD, password);

// upload profile picture file
export const uploadProfilePictureFile = file => rootPost(url.UPLOAD_PROFILE_PICTURE_FILE, file);
export const uploadSignaturePictureFile = file => rootPost(url.UPLOAD_SIGNATURE_PICTURE_FILE, file);

// enable google authenticator
export const enableGoogleAuthenticator = () => put(url.ENABLE_GOOGLE_AUTHENTICATOR);
export const disableGoogleAuthenticator = () => post(url.DISABLE_GOOGLE_AUTHENTICATOR);

// download collected data
export const downloadCollectedData = () => post(url.DOWNLOAD_COLLECTED_DATA);