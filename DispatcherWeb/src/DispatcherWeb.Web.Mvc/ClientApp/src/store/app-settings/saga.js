import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_TENANT_SETTINGS } from './actionTypes';
import { getTenantSettingsSuccess, getTenantSettingsFailure } from './actions';
import { getTenantSettings } from './service';

function* fetchAppSettings() {
    try {
        const response = yield call(getTenantSettings);
        yield put(getTenantSettingsSuccess(response));
    } catch (error) {
        yield put(getTenantSettingsFailure(error));
    }
}

function* appSettingsSaga() {
    yield takeEvery(GET_TENANT_SETTINGS, fetchAppSettings);
}

export default appSettingsSaga;