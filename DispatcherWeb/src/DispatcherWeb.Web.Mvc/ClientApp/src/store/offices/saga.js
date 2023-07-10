import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_OFFICES } from './actionTypes';
import { getOfficesSuccess, getOfficesFailure } from './actions';
import { getOffices } from './service';

function* fetchOffices() {
    try {
        const response = yield call(getOffices);
        yield put(getOfficesSuccess(response));
    } catch (error) {
        yield put(getOfficesFailure(error));
    }
}

function* officeSaga() {
    yield takeEvery(GET_OFFICES, fetchOffices);
}

export default officeSaga;