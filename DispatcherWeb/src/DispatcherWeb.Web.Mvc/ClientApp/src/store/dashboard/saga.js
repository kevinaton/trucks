import { call, put, takeEvery } from 'redux-saga/effects'

import { GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW } from './actionTypes'
import {
    getScheduledTruckCountPartialViewSuccess,
    getScheduledTruckCountPartialViewFailure 
} from './actions'

import { getScheduledTruckCountPartialView } from './service'

function* fetchScheduledTruckCountPartialView() {
    try {
        const response = yield call(getScheduledTruckCountPartialView)
        yield put(getScheduledTruckCountPartialViewSuccess(response))
    } catch (error) {
        yield put(getScheduledTruckCountPartialViewFailure(error))
    }
}

function* dashboardSaga() {
    yield takeEvery(GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW, fetchScheduledTruckCountPartialView)
}

export default dashboardSaga