import { all, fork } from 'redux-saga/effects';
import layoutSaga from './layout/saga';
import dashboardSaga from './dashboard/saga';
import accountSaga from './account/saga';
import userSaga from './user/saga';
import userProfileSaga from './authorization/users/profile/saga';
import userLinkSaga from './authorization/users/userLink/saga';
import notificationSaga from './notifications/saga';

export default function* rootSaga() {
    yield all([
        fork(layoutSaga),
        fork(dashboardSaga),
        fork(accountSaga),
        fork(userSaga),
        fork(userProfileSaga),
        fork(userLinkSaga),
        fork(notificationSaga)
    ]);
};