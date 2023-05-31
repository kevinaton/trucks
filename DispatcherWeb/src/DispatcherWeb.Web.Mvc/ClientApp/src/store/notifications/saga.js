import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_USER_NOTIFICATIONS,
    SET_ALL_NOTIFICATIONS_AS_READ,
    SET_NOTIFICATION_AS_READ,
    GET_USER_NOTIFICATION_SETTINGS } from './actionTypes';
import {
    getUserNotificationsSuccess,
    getUserNotificationsFailure,
    setAllNotificationsAsReadSuccess,
    setAllNotificationsAsReadFailure,
    setNotificationAsReadSuccess,
    setNotificationAsReadFailure,
    getUserNotificationSettingsSuccess,
    getUserNotificationSettingsFailure
} from './actions';
import { 
    getUserNotifications, 
    setAllNotificationsAsRead,
    setNotificationAsRead,
    getUserNotificationSettings } from './service';

function* fetchUserNotifications() {
    try {
        const response = yield call(getUserNotifications);
        yield put(getUserNotificationsSuccess(response));
    } catch (error) {
        yield put(getUserNotificationsFailure(error));
    }
}

function* fetchUserNotificationSettings() {
    try {
        const response = yield call(getUserNotificationSettings);
        yield put(getUserNotificationSettingsSuccess(response));
    } catch (error) {
        yield put(getUserNotificationSettingsFailure(error));
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
        yield call(setNotificationAsRead, notification);
        yield put(setNotificationAsReadSuccess(notification));
    } catch (error) {
        yield put(setNotificationAsReadFailure(error));
    }
}

function* notificationSaga() {
    yield takeEvery(GET_USER_NOTIFICATIONS, fetchUserNotifications);
    yield takeEvery(GET_USER_NOTIFICATION_SETTINGS, fetchUserNotificationSettings);
    yield takeEvery(SET_ALL_NOTIFICATIONS_AS_READ, onSetAllNotificationsAsRead);
    yield takeEvery(SET_NOTIFICATION_AS_READ, onSetNotificationAsRead);
}

export default notificationSaga;