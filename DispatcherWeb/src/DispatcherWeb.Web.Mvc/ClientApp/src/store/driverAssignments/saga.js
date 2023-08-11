import { call, put, takeEvery } from 'redux-saga/effects';
import {
    SET_DRIVER_FOR_TRUCK,
    HAS_ORDER_LINE_TRUCKS
} from './actionTypes';
import {
    setDriverForTruckSuccess,
    setDriverForTruckFailure,
    hasOrderLineTrucksSuccess,
    hasOrderLineTrucksFailure
} from './actions';
import {
    setDriverForTruck,
    hasOrderLineTrucks
} from './service';

function* onSetDriverForTruck({ payload: driverAssignment }) {
    try {
        const response = yield call(setDriverForTruck, driverAssignment);
        yield put(setDriverForTruckSuccess(response));
    } catch (error) {
        yield put(setDriverForTruckFailure(error));
    }
}

function* onHasOrderLineTrucks({ payload: { truckId, filter} }) {
    try {
        const response = yield call(hasOrderLineTrucks, filter);
        yield put(hasOrderLineTrucksSuccess({
            truckId,
            response
        }));
    } catch (error) {
        yield put(hasOrderLineTrucksFailure(error));
    }
}

function* driverAssignmentSaga() {
    yield takeEvery(SET_DRIVER_FOR_TRUCK, onSetDriverForTruck);
    yield takeEvery(HAS_ORDER_LINE_TRUCKS, onHasOrderLineTrucks);
}

export default driverAssignmentSaga;