import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_USER_INFO, GET_USER_SETTING } from './actionTypes';
import { 
    getUserInfoSuccess, 
    getUserInfoFailure,
    getUserSettingByNameSuccess,
    getUserSettingByNameFailure } from './actions';
import { getUserInfo, getUserSetting } from './service';

function* fetchUserInfo() {
    try {
        const response = yield call(getUserInfo);
        yield put(getUserInfoSuccess(response));
    } catch (error) {
        yield put(getUserInfoFailure(error));
    }
}

function* fetchUserSetting({ payload: settingName }) {
    try {
        const response = yield call(getUserSetting, settingName);
        yield put(getUserSettingByNameSuccess(response));
    } catch (error) {
        yield put(getUserSettingByNameFailure(error));
    }
}

function* userSaga() {
    yield takeEvery(GET_USER_INFO, fetchUserInfo);
    yield takeEvery(GET_USER_SETTING, fetchUserSetting);
}

export default userSaga;