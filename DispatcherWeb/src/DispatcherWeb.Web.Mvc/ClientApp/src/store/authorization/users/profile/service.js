import { get, post, put } from '../../../../helpers/api_helper';
import { rootPost } from '../../../../helpers/root_api_helper';
import * as apiUrl from '../../../../helpers/api_url_helper';
import * as appUrl from '../../../../helpers/app_url_helper';

// get user profile settings
export const getUserProfileSettings = () => get(apiUrl.GET_USER_PROFILE_SETTINGS);

// update user profile
export const updateUserProfile = userProfile => put(apiUrl.UPDATE_USER_PROFILE, userProfile);

// update profile picture
export const updateProfilePicture = profilePicture => put(url.UPDATE_PROFILE_PICTURE, profilePicture);

// update signature picture
export const updateSignaturePicture = signaturePicture => put(apiUrl.UPDATE_SIGNATURE_PICTURE, signaturePicture);

// change password
export const changePassword = password => post(apiUrl.CHANGE_PASSWORD, password);

// upload profile picture file
export const uploadProfilePictureFile = file => rootPost(appUrl.UPLOAD_PROFILE_PICTURE_FILE, file);
export const uploadSignaturePictureFile = file => rootPost(appUrl.UPLOAD_SIGNATURE_PICTURE_FILE, file);

// enable google authenticator
export const enableGoogleAuthenticator = () => put(apiUrl.ENABLE_GOOGLE_AUTHENTICATOR);
export const disableGoogleAuthenticator = () => post(apiUrl.DISABLE_GOOGLE_AUTHENTICATOR);

// download collected data
export const downloadCollectedData = () => post(apiUrl.DOWNLOAD_COLLECTED_DATA);