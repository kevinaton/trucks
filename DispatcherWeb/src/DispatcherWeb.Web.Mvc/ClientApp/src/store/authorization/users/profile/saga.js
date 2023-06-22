import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_USER_PROFILE_SETTINGS,
    CHANGE_PASSWORD,
    UPLOAD_PROFILE_PICTURE_FILE
} from './actionTypes';
import {
    getUserProfileSettingsSuccess,
    getUserProfileSettingsFailure,
    changePasswordSuccess,
    changePasswordFailure,
    uploadProfilePictureFileSuccess,
    uploadProfilePictureFileFailure
} from './actions';
import {
    getUserProfileSettings,
    changePassword,
    uploadProfilePictureFile
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

function* onUploadProfilePictureFile({ payload: file }) {
    try {
        const response = yield call(uploadProfilePictureFile, file);
        yield put(uploadProfilePictureFileSuccess(response));
    } catch (error) {
        yield put(uploadProfilePictureFileFailure(error));
    }
}

function* userProfileSaga() {
    yield takeEvery(GET_USER_PROFILE_SETTINGS, fetchUserProfileSettings);
    yield takeEvery(CHANGE_PASSWORD, onChangePassword);
    yield takeEvery(UPLOAD_PROFILE_PICTURE_FILE, onUploadProfilePictureFile);
}

export default userProfileSaga;