import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_USER_PROFILE_SETTINGS,
    UPDATE_USER_PROFILE,
    CHANGE_PASSWORD,
    UPLOAD_PROFILE_PICTURE_FILE,
    ENABLE_GOOGLE_AUTHENTICATOR,
    DISABLE_GOOGLE_AUTHENTICATOR,
    DOWNLOAD_COLLECTED_DATA
} from './actionTypes';
import {
    getUserProfileSettingsSuccess,
    getUserProfileSettingsFailure,
    updateUserProfileSuccess,
    updateUserProfileFailure,
    changePasswordSuccess,
    changePasswordFailure,
    uploadProfilePictureFileSuccess,
    uploadProfilePictureFileFailure,
    enableGoogleAuthenticatorSuccess,
    enableGoogleAuthenticatorFailure,
    disableGoogleAuthenticatorSuccess,
    disableGoogleAuthenticatorFailure,
    downloadCollectedDataSuccess,
    downloadCollectedDataFailure
} from './actions';
import {
    getUserProfileSettings,
    updateUserProfile,
    changePassword,
    uploadProfilePictureFile,
    enableGoogleAuthenticator,
    disableGoogleAuthenticator,
    downloadCollectedData
} from './service';

function* fetchUserProfileSettings() {
    try {
        const response = yield call(getUserProfileSettings);
        yield put(getUserProfileSettingsSuccess(response));
    } catch (error) {
        yield put(getUserProfileSettingsFailure(error));
    }
}

function* onUpdateUserProfile({ payload: userProfile }) {
    try {
        yield call(updateUserProfile, userProfile);
        yield put(updateUserProfileSuccess(userProfile));
    } catch (error) {
        yield put(updateUserProfileFailure(error));
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

function* onEnableGoogleAuthenticator() {
    try {
        const response = yield call(enableGoogleAuthenticator);
        yield put(enableGoogleAuthenticatorSuccess(response));
    } catch (error) {
        yield put(enableGoogleAuthenticatorFailure(error));
    }
}

function* onDisableGoogleAuthenticator() {
    try {
        const response = yield call(disableGoogleAuthenticator);
        yield put(disableGoogleAuthenticatorSuccess(response));
    } catch (error) {
        yield put(disableGoogleAuthenticatorFailure(error));
    }
}

function* onDownloadCollectedData() {
    try {
        yield call(downloadCollectedData);
        yield put(downloadCollectedDataSuccess());
    } catch (error) {
        yield put(downloadCollectedDataFailure(error));
    }
}

function* userProfileSaga() {
    yield takeEvery(GET_USER_PROFILE_SETTINGS, fetchUserProfileSettings);
    yield takeEvery(UPDATE_USER_PROFILE, onUpdateUserProfile);
    yield takeEvery(CHANGE_PASSWORD, onChangePassword);
    yield takeEvery(UPLOAD_PROFILE_PICTURE_FILE, onUploadProfilePictureFile);
    yield takeEvery(ENABLE_GOOGLE_AUTHENTICATOR, onEnableGoogleAuthenticator);
    yield takeEvery(DISABLE_GOOGLE_AUTHENTICATOR, onDisableGoogleAuthenticator);
    yield takeEvery(DOWNLOAD_COLLECTED_DATA, onDownloadCollectedData);
}

export default userProfileSaga;