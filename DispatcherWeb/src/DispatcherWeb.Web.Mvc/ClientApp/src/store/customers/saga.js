import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_ACTIVE_CUSTOMERS_SELECT_LIST
} from './actionTypes';
import {
    getActiveCustomersSelectListSuccess,
    getActiveCustomersSelectListFailure
} from './actions';
import {
    getActiveCustomersSelectList
} from './service';

function* fetchActiveCustomersSelectList({ payload: filter }) {
    try {
        const response = yield call(getActiveCustomersSelectList, filter);
        yield put(getActiveCustomersSelectListSuccess(response));
    } catch (error) {
        yield put(getActiveCustomersSelectListFailure(error));
    }
}

function* customersSaga() {
    yield takeEvery(GET_ACTIVE_CUSTOMERS_SELECT_LIST, fetchActiveCustomersSelectList);
}

export default customersSaga;