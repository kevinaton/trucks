import axios from 'axios'
import { get } from '../../helpers/api_helper'
import * as url from '../../helpers/url_helper'

// get menu items
export const getMenuItems = () => get(url.GET_MENU_ITEMS)

// get support link address
export const getSupportLinkAddress = () => get(url.GET_SUPPORT_LINK_ADDRESS)