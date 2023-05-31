import { combineReducers } from 'redux';

import LayoutReducer from './layout/reducer';
import DashboardReducer from './dashboard/reducer';
import UserReducer from './user/reducer';
import NotificationBellReducer from './notificationBell/reducer';

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    DashboardReducer,
    UserReducer,
    NotificationBellReducer
});

export default rootReducer;