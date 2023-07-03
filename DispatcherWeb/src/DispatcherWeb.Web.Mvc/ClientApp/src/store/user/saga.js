import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_USER_INFO, 
    GET_USER_PROFILE_MENU,
    GET_USER_SETTING } from './actionTypes';
import { 
    getUserInfoSuccess, 
    getUserInfoFailure,
    getUserProfileMenuSuccess,
    getUserProfileMenuFailure,
    getUserSettingByNameSuccess,
    getUserSettingByNameFailure } from './actions';
import { getUserInfo, getUserProfileMenu, getUserSetting } from './service';

function* fetchUserInfo() {
    try {
        const response = yield call(getUserInfo);
        yield put(getUserInfoSuccess(response));
    } catch (error) {
        yield put(getUserInfoFailure(error));
    }
}

function* fetchUserProfileMenu() {
    try {
        const response = yield call(getUserProfileMenu);
        yield put(getUserProfileMenuSuccess(response));
    } catch (error) {
        yield put(getUserProfileMenuFailure(error));
    }
}

function* fetchUserSetting({ payload: settingName }) {
    try {
        const response = yield call(getUserSetting, settingName);
        const result = {
            name: settingName,
            value: response.result
        };
        yield put(getUserSettingByNameSuccess(result));
    } catch (error) {
        yield put(getUserSettingByNameFailure(error));
    }
}

function* userSaga() {
    yield takeEvery(GET_USER_INFO, fetchUserInfo);
    yield takeEvery(GET_USER_PROFILE_MENU, fetchUserProfileMenu);
    yield takeEvery(GET_USER_SETTING, fetchUserSetting);
}

export default userSaga;