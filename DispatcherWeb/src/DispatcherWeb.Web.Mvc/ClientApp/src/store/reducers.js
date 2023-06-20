import { combineReducers } from 'redux';

import LayoutReducer from './layout/reducer';
import DashboardReducer from './dashboard/reducer';
import UserReducer from './user/reducer';
import UserProfileReducer from './authorization/users/profile/reducer';
import UserLinkReducer from './authorization/users/userLink/reducer';
import NotificationReducer from './notifications/reducer';

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    DashboardReducer,
    UserReducer,
    UserProfileReducer,
    UserLinkReducer,
    NotificationReducer
});

export default rootReducer;