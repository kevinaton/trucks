import { combineReducers } from 'redux';
import AccountReducer from './account/reducer';
import AppSettingsReducer from './app-settings/reducer';
import CustomerReducer from './customers/reducer';
import DashboardReducer from './dashboard/reducer';
import DesignationReducer from './designations/reducer';
import DriverReducer from './drivers/reducer';
import DriverAssignmentReducer from './driverAssignments/reducer';
import FeatureReducer from './features/reducer';
import LayoutReducer from './layout/reducer';
import LeaseHaulerReducer from './leaseHaulers/reducer';
import LeaseHaulerRequestEditReducer from './leaseHaulerRequestEdit/reducer';
import LocationReducer from './locations/reducer';
import NotificationReducer from './notifications/reducer';
import OfficeReducer from './offices/reducer';
import OrderReducer from './orders/reducer';
import SchedulingReducer from './scheduling/reducer';
import ServiceReducer from './services/reducer';
import TrailerAssignmentReducer from './trailerAssignments/reducer';
import TruckReducer from './trucks/reducer';
import UnitOfMeasureReducer from './unitsOfMeasure/reducer';
import UserLinkReducer from './authorization/users/userLink/reducer';
import UserProfileReducer from './authorization/users/profile/reducer';
import UserReducer from './user/reducer';

const rootReducer = combineReducers({
    AccountReducer,
    AppSettingsReducer,
    CustomerReducer,
    DashboardReducer,
    DesignationReducer,
    DriverReducer,
    DriverAssignmentReducer,
    FeatureReducer,
    LayoutReducer,
    LeaseHaulerReducer,
    LeaseHaulerRequestEditReducer,
    LocationReducer,
    NotificationReducer,
    OfficeReducer,
    OrderReducer,
    SchedulingReducer,
    ServiceReducer,
    TrailerAssignmentReducer,
    TruckReducer,
    UnitOfMeasureReducer,
    UserLinkReducer,
    UserProfileReducer,
    UserReducer
});

export default rootReducer;