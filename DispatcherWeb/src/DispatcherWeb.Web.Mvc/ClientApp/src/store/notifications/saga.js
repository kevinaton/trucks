import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_USER_NOTIFICATIONS,
    SET_ALL_NOTIFICATIONS_AS_READ,
    SET_NOTIFICATION_AS_READ } from './actionTypes';
import {
    getUserNotificationsSuccess,
    getUserNotificationsFailure,
    setAllNotificationsAsReadSuccess,
    setAllNotificationsAsReadFailure,
    setNotificationAsReadSuccess,
    setNotificationAsReadFailure
} from './actions';
import { 
    getUserNotifications, 
    setAllNotificationsAsRead,
    setNotificationAsRead } from './service';

function* fetchUserNotifications() {
    try {
        const response = yield call(getUserNotifications);
        yield put(getUserNotificationsSuccess(response));
    } catch (error) {
        yield put(getUserNotificationsFailure(error));
    }
}

function* onSetAllNotificationsAsRead() {
    try {
        const response = yield call(setAllNotificationsAsRead);
        yield put(setAllNotificationsAsReadSuccess(response));
    } catch (error) {
        yield put(setAllNotificationsAsReadFailure(error));
    }
}

function* onSetNotificationAsRead({ payload: notification }) {
    try {
        const response = yield call(setNotificationAsRead, notification);
        yield put(setNotificationAsReadSuccess(notification));
    } catch (error) {
        yield put(setNotificationAsReadFailure(error));
    }
}

function* notificationSaga() {
    yield takeEvery(GET_USER_NOTIFICATIONS, fetchUserNotifications);
    yield takeEvery(SET_ALL_NOTIFICATIONS_AS_READ, onSetAllNotificationsAsRead);
    yield takeEvery(SET_NOTIFICATION_AS_READ, onSetNotificationAsRead);
}

export default notificationSaga;