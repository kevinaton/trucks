import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_LEASE_HAULER_DRIVERS_SELECT_LIST
} from './actionTypes';
import {
    getLeaseHaulerDriversSelectListSuccess,
    getLeaseHaulerDriversSelectListFailure
} from './actions';
import {
    getLeaseHaulerDriversSelectList
} from './service';

function* fetchLeaseHaulerDriversSelectList({ payload: filter }) {
    try {
        const response = yield call(getLeaseHaulerDriversSelectList, filter);
        yield put(getLeaseHaulerDriversSelectListSuccess(response));
    } catch (error) {
        yield put(getLeaseHaulerDriversSelectListFailure(error));
    }
}

function* leaseHaulerSaga() {
    yield takeEvery(GET_LEASE_HAULER_DRIVERS_SELECT_LIST, fetchLeaseHaulerDriversSelectList);
}

export default leaseHaulerSaga;