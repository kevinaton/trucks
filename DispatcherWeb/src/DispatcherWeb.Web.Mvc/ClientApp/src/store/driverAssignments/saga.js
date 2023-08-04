import { call, put, takeEvery } from 'redux-saga/effects';
import {
    SET_DRIVER_FOR_TRUCK
} from './actionTypes';
import {
    setDriverForTruckSuccess,
    setDriverForTruckFailure
} from './actions';
import {
    setDriverForTruck
} from './service';

function* onSetDriverForTruck({ payload: driverAssignment }) {
    try {
        const response = yield call(setDriverForTruck, driverAssignment);
        yield put(setDriverForTruckSuccess(response));
    } catch (error) {
        yield put(setDriverForTruckFailure(error));
    }
}

function* driverAssignmentSaga() {
    yield takeEvery(SET_DRIVER_FOR_TRUCK, onSetDriverForTruck);
}

export default driverAssignmentSaga;