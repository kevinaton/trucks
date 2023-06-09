import { get } from '../../helpers/api_helper';
import * as url from '../../helpers/url_helper';

// check if feature is enabled
export const checkIfEnabled = featureName => get(`${url.IS_FEATURE_ENABLED}?featureName=${featureName}`);