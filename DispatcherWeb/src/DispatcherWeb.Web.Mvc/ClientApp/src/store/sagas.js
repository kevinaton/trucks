import { all, fork } from 'redux-saga/effects';
import accountSaga from './account/saga';
import appSettingsSaga from './app-settings/saga';
import customerSaga from './customers/saga';
import dashboardSaga from './dashboard/saga';
import designationSaga from './designations/saga';
import driverSaga from './drivers/saga';
import driverAssignmentSaga from './driverAssignments/saga';
import featuresSaga from './features/saga';
import layoutSaga from './layout/saga';
import leaseHaulerSaga from './leaseHaulers/saga';
import leaseHaulerRequestEditSaga from './leaseHaulerRequestEdit/saga';
import locationSaga from './locations/saga';
import notificationSaga from './notifications/saga';
import officeSaga from './offices/saga';
import ordersSaga from './orders/saga';
import schedulingSaga from './scheduling/saga';
import serviceSaga from './services/saga';
import trailerAssignmentSaga from './trailerAssignments/saga';
import truckSaga from './trucks/saga';
import unitOfMeasureSaga from './unitsOfMeasure/saga';
import userSaga from './user/saga';
import userLinkSaga from './authorization/users/userLink/saga';
import userProfileSaga from './authorization/users/profile/saga';

export default function* rootSaga() {
    yield all([
        fork(accountSaga),
        fork(appSettingsSaga),
        fork(customerSaga),
        fork(dashboardSaga),
        fork(designationSaga),
        fork(driverSaga),
        fork(driverAssignmentSaga),
        fork(featuresSaga),
        fork(layoutSaga),
        fork(leaseHaulerSaga),
        fork(leaseHaulerRequestEditSaga),
        fork(locationSaga),
        fork(notificationSaga),
        fork(officeSaga),
        fork(ordersSaga),
        fork(schedulingSaga),
        fork(serviceSaga),
        fork(trailerAssignmentSaga),
        fork(truckSaga),
        fork(unitOfMeasureSaga),
        fork(userSaga),
        fork(userLinkSaga),
        fork(userProfileSaga)
    ]);
};