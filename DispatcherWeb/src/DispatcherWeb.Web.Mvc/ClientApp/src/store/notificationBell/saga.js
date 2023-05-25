import { call, put, takeEvery } from 'redux-saga/effects'

import { GET_USER_NOTIFICATIONS } from './actionTypes'
import {
    getUserNotificationsSuccess,
    getUserNotificationsFailure
} from './actions'

import { getUserNotifications } from './service'

function* fetchUserNotifications() {
    try {
        const response = yield call(getUserNotifications)
        yield put(getUserNotificationsSuccess(response))
    } catch (error) {
        yield put(getUserNotificationsFailure(error))
    }
}

function* notificationBellSaga() {
    yield takeEvery(GET_USER_NOTIFICATIONS, fetchUserNotifications)
}

export default notificationBellSaga