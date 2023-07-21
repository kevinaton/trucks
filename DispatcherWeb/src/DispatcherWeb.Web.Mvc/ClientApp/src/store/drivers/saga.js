import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_DRIVERS_SELECT_LIST
} from './actionTypes';
import {
    getDriversSelectListSuccess,
    getDriversSelectListFailure
} from './actions';
import {
    getDriversSelectList
} from './service';

function* fetchDriversSelectList() {
    try {
        const response = yield call(getDriversSelectList);
        yield put(getDriversSelectListSuccess(response));
    } catch (error) {
        yield put(getDriversSelectListFailure(error));
    }
}

function* driversSaga() {
    yield takeEvery(GET_DRIVERS_SELECT_LIST, fetchDriversSelectList);
}

export default driversSaga;