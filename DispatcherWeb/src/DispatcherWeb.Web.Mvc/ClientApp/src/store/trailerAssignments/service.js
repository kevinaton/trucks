import { post } from '../../helpers/api_helper';
import * as url from '../../helpers/api_url_helper';

// set trailer for tractor
export const setTrailerForTractor = trailerAssignment => post(url.SET_TRAILER_FOR_TRACTOR, trailerAssignment);