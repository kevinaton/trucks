import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_USER_INFO,
    GET_USER_GENERAL_SETTINGS,
    GET_USER_SETTING, 
    GET_USER_PROFILE_MENU } from './actionTypes';
import { 
    getUserInfoSuccess, 
    getUserInfoFailure,
    getUserGeneralSettingsSuccess,
    getUserGeneralSettingsFailure,
    getUserSettingByNameSuccess,
    getUserSettingByNameFailure,
    getUserProfileMenuSuccess,
    getUserProfileMenuFailure, } from './actions';
import { 
    getUserInfo, 
    getUserGeneralSettings,
    getUserSetting, 
    getUserProfileMenu } from './service';

function* fetchUserInfo() {
    try {
        const response = yield call(getUserInfo);
        yield put(getUserInfoSuccess(response));
    } catch (error) {
        yield put(getUserInfoFailure(error));
    }
}

function* fetchUserGeneralSettings() {
    try {
        const response = yield call(getUserGeneralSettings);
        yield put(getUserGeneralSettingsSuccess(response));
    } catch (error) {
        yield put(getUserGeneralSettingsFailure(error));
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

function* fetchUserProfileMenu() {
    try {
        const response = yield call(getUserProfileMenu);
        yield put(getUserProfileMenuSuccess(response));
    } catch (error) {
        yield put(getUserProfileMenuFailure(error));
    }
}

function* userSaga() {
    yield takeEvery(GET_USER_INFO, fetchUserInfo);
    yield takeEvery(GET_USER_GENERAL_SETTINGS, fetchUserGeneralSettings);
    yield takeEvery(GET_USER_SETTING, fetchUserSetting);
    yield takeEvery(GET_USER_PROFILE_MENU, fetchUserProfileMenu);
}

export default userSaga;