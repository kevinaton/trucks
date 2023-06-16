import { combineReducers } from 'redux';

import LayoutReducer from './layout/reducer';
import DashboardReducer from './dashboard/reducer';
import UserLinkReducer from './authorization/users/userLink/reducer';
import UserReducer from './user/reducer';
import NotificationReducer from './notifications/reducer';

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    DashboardReducer,
    UserLinkReducer,
    UserReducer,
    NotificationReducer
});

export default rootReducer;