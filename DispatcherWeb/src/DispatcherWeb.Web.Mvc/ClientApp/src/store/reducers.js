import { combineReducers } from 'redux';

import LayoutReducer from './layout/reducer';
import DashboardReducer from './dashboard/reducer';
import UserReducer from './user/reducer';
import NotificationReducer from './notifications/reducer';
import SchedulingReducer from './scheduling/reducer';

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    DashboardReducer,
    UserReducer,
    NotificationReducer,
    SchedulingReducer
});

export default rootReducer;