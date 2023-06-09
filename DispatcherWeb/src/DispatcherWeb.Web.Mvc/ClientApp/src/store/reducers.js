import { combineReducers } from 'redux';

import LayoutReducer from './layout/reducer';
import FeatureReducer from './features/reducer';
import DashboardReducer from './dashboard/reducer';
import UserReducer from './user/reducer';
import NotificationReducer from './notifications/reducer';
import OfficeReducer from './offices/reducer';
import SchedulingReducer from './scheduling/reducer';

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    FeatureReducer,
    DashboardReducer,
    UserReducer,
    NotificationReducer,
    OfficeReducer,
    SchedulingReducer
});

export default rootReducer;