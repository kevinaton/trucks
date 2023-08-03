import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_DRIVERS_SELECT_LIST,
    GET_DRIVER_FOR_EDIT
} from './actionTypes';
import {
    getDriversSelectListSuccess,
    getDriversSelectListFailure,
    getDriverForEditSuccess,
    getDriverForEditFailure
} from './actions';
import {
    getDriversSelectList,
    getDriverForEdit
} from './service';

function* fetchDriversSelectList({ payload: filter }) {
    try {
        const response = yield call(getDriversSelectList, filter);
        yield put(getDriversSelectListSuccess(response));
    } catch (error) {
        yield put(getDriversSelectListFailure(error));
    }
}

function* fetchDriverForEdit() {
    try {
        const response = yield call(getDriverForEdit);
        yield put(getDriverForEditSuccess(response));
    } catch (error) {
        yield put(getDriverForEditFailure(error));
    }
}

function* driversSaga() {
    yield takeEvery(GET_DRIVERS_SELECT_LIST, fetchDriversSelectList);
    yield takeEvery(GET_DRIVER_FOR_EDIT, fetchDriverForEdit);
}

export default driversSaga;