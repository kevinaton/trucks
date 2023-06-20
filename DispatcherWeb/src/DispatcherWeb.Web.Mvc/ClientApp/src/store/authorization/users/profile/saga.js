import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_USER_PROFILE_SETTINGS
} from './actionTypes';
import {
    getUserProfileSettingsSuccess,
    getUserProfileSettingsFailure
} from './actions';
import {
    getUserProfileSettings
} from './service';

function* fetchUserProfileSettings() {
    try {
        const response = yield call(getUserProfileSettings);
        yield put(getUserProfileSettingsSuccess(response));
    } catch (error) {
        yield put(getUserProfileSettingsFailure(error));
    }
}

function* userProfileSaga() {
    yield takeEvery(GET_USER_PROFILE_SETTINGS, fetchUserProfileSettings);
}

export default userProfileSaga;