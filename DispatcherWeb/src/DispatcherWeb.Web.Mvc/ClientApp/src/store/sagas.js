import { all, fork } from 'redux-saga/effects';
import appSettingsSaga from './app-settings/saga';
import layoutSaga from './layout/saga';
import featuresSaga from './features/saga';
import dashboardSaga from './dashboard/saga';
import accountSaga from './account/saga';
import userSaga from './user/saga';
import userProfileSaga from './authorization/users/profile/saga';
import userLinkSaga from './authorization/users/userLink/saga';
import notificationSaga from './notifications/saga';
import officeSaga from './offices/saga';
import driverSaga from './drivers/saga';
import driverAssignmentSaga from './driverAssignments/saga';
import trailerAssignmentSaga from './trailerAssignments/saga';
import truckSaga from './trucks/saga';
import leaseHaulerSaga from './leaseHaulers/saga';
import schedulingSaga from './scheduling/saga';

export default function* rootSaga() {
    yield all([
        fork(appSettingsSaga),
        fork(layoutSaga),
        fork(featuresSaga),
        fork(dashboardSaga),
        fork(accountSaga),
        fork(userSaga),
        fork(notificationSaga),
        fork(officeSaga),
        fork(driverSaga),
        fork(driverAssignmentSaga),
        fork(trailerAssignmentSaga),
        fork(truckSaga),
        fork(leaseHaulerSaga),
        fork(schedulingSaga),
        fork(userProfileSaga),
        fork(userLinkSaga)
    ]);
};