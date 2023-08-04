import { combineReducers } from 'redux';

import AppSettingsReducer from './app-settings/reducer';
import LayoutReducer from './layout/reducer';
import FeatureReducer from './features/reducer';
import DashboardReducer from './dashboard/reducer';
import AccountReducer from './account/reducer';
import UserReducer from './user/reducer';
import UserProfileReducer from './authorization/users/profile/reducer';
import UserLinkReducer from './authorization/users/userLink/reducer';
import NotificationReducer from './notifications/reducer';
import OfficeReducer from './offices/reducer';
import DriverReducer from './drivers/reducer';
import TruckReducer from './trucks/reducer';
import LeaseHaulerReducer from './leasehaulers/reducer';
import SchedulingReducer from './scheduling/reducer';

const rootReducer = combineReducers({
    // public
    AppSettingsReducer,
    LayoutReducer,
    FeatureReducer,
    DashboardReducer,
    AccountReducer,
    UserReducer,
    NotificationReducer,
    OfficeReducer,
    DriverReducer,
    TruckReducer,
    LeaseHaulerReducer,
    SchedulingReducer,
    UserProfileReducer,
    UserLinkReducer
});

export default rootReducer;