import { get } from '../../helpers/api_helper'
import * as url from '../../helpers/url_helper'

// get user notifications
export const getUserNotifications = () => get(url.GET_USER_NOTIFICATIONS)