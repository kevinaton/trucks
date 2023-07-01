import { rootPost } from '../../helpers/root_api_helper';
import * as url from '../../helpers/app_url_helper';

// back to impersonator
export const backToImpersonator = () => rootPost(url.BACK_TO_IMPERSONATOR);

// impersonate signin
export const impersonateSignin = tokenId => rootPost(`${url.IMPERSONATE_SIGNIN}?tokenId=${tokenId.tokenId}`);

// switch to user
export const switchToUser = account => rootPost(url.SWITCH_TO_USER, account);