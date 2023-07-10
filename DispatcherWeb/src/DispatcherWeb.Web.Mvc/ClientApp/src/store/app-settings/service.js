import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// get tenant settings
export const getTenantSettings = () => get(url.GET_TENANT_SETTINGS);