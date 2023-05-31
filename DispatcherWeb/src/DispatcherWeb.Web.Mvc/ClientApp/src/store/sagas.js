import { all, fork } from 'redux-saga/effects';
import layoutSaga from './layout/saga';
import dashboardSaga from './dashboard/saga';
import userSaga from './user/saga';
import notificationSaga from './notifications/saga';

export default function* rootSaga() {
    yield all([
        fork(layoutSaga),
        fork(dashboardSaga),
        fork(userSaga),
        fork(notificationSaga)
    ]);
};