import axios from 'axios'
import { get } from '../../helpers/api_helper'
import * as url from '../../helpers/url_helper'

// get support link address
export const getSupportLinkAddress = () => get(false, url.GET_SUPPORT_LINK_ADDRESS)