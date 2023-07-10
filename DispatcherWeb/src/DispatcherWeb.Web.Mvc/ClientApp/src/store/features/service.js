import { get, post } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// check if feature is enabled
export const checkIfEnabled = featureName => post(`${url.IS_FEATURE_ENABLED}?featureName=${featureName}`);