import { call, put, takeEvery } from 'redux-saga/effects';
import { CHECK_IF_ENABLED } from './actionTypes';
import { checkIfEnabledSuccess, checkIfEnabledFailure } from './actions';
import { checkIfEnabled } from './service';

function* checkIfEnabledSaga({ payload: featureName }) {
    try {
        const response = yield call(checkIfEnabled, featureName);
        yield put(checkIfEnabledSuccess(response));
    } catch (error) {
        yield put(checkIfEnabledFailure(error));
    }
}

export function* featuresSaga() {
    yield takeEvery(CHECK_IF_ENABLED, checkIfEnabledSaga);
}

export default featuresSaga;