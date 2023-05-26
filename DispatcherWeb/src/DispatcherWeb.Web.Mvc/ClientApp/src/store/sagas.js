import { all, fork } from 'redux-saga/effects';

import layoutSaga from './layout/saga';
import dashboardSaga from './dashboard/saga';

export default function* rootSaga() {
    yield all([
        fork(layoutSaga),
        fork(dashboardSaga)
    ]);
};