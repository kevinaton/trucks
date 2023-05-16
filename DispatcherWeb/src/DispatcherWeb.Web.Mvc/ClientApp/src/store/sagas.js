import { all, fork } from 'redux-saga/effects'

import layoutSaga from './layout/saga'

export default function* rootSaga() {
    yield all([
        fork(layoutSaga)
    ])
}