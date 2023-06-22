import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_USER_PROFILE_SETTINGS,
    CHANGE_PASSWORD
} from './actionTypes';
import {
    getUserProfileSettingsSuccess,
    getUserProfileSettingsFailure,
    changePasswordSuccess,
    changePasswordFailure
} from './actions';
import {
    getUserProfileSettings,
    changePassword
} from './service';

function* fetchUserProfileSettings() {
    try {
        const response = yield call(getUserProfileSettings);
        yield put(getUserProfileSettingsSuccess(response));
    } catch (error) {
        yield put(getUserProfileSettingsFailure(error));
    }
}

function* onChangePassword({ payload: password }) {
    try {
        yield call(changePassword, password);
        yield put(changePasswordSuccess());
    } catch (error) {
        yield put(changePasswordFailure(error));
    }
}

function* userProfileSaga() {
    yield takeEvery(GET_USER_PROFILE_SETTINGS, fetchUserProfileSettings);
    yield takeEvery(CHANGE_PASSWORD, onChangePassword);
}

export default userProfileSaga;