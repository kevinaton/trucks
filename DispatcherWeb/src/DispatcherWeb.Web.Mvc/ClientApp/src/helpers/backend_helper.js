import { get } from './api_helper'
import * as url from './url_helper'

export const getLoggedInUser = () => get(url.getLoggedInUser)