import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_USER_INFO, GET_USER_SETTINGS } from './actionTypes';
import { 
    getUserInfoSuccess, 
    getUserInfoFailure,
    getUserSettingsByNameSuccess,
    getUserSettingsByNameFailure } from './actions';
import { getUserInfo, getUserSettings } from './service';

function* fetchUserInfo() {
    try {
        const response = yield call(getUserInfo);
        yield put(getUserInfoSuccess(response));
    } catch (error) {
        yield put(getUserInfoFailure(error));
    }
}

function* fetchUserSettings({ payload: settingName }) {
    try {
        const response = yield call(getUserSettings, settingName);
        yield put(getUserSettingsByNameSuccess(response));
    } catch (error) {
        yield put(getUserSettingsByNameFailure(error));
    }
}

function* userSaga() {
    yield takeEvery(GET_USER_INFO, fetchUserInfo);
    yield takeEvery(GET_USER_SETTINGS, fetchUserSettings);
}

export default userSaga;