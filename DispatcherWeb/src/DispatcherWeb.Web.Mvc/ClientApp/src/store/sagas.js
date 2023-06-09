import { all, fork } from 'redux-saga/effects';
import layoutSaga from './layout/saga';
import featuresSaga from './features/saga';
import dashboardSaga from './dashboard/saga';
import userSaga from './user/saga';
import notificationSaga from './notifications/saga';
import officeSaga from './offices/saga';
import schedulingSaga from './scheduling/saga';

export default function* rootSaga() {
    yield all([
        fork(layoutSaga),
        fork(featuresSaga),
        fork(dashboardSaga),
        fork(userSaga),
        fork(notificationSaga),
        fork(officeSaga),
        fork(schedulingSaga),
    ]);
};