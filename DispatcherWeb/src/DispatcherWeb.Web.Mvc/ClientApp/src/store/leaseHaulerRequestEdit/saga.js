import { call, put, takeEvery } from 'redux-saga/effects';
import {
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE
} from './actionTypes';
import {
    removeAvailableLeaseHaulerTruckFromScheduleSuccess,
    removeAvailableLeaseHaulerTruckFromScheduleFailure
} from './actions';
import {
    removeAvailableLeaseHaulerTruckFromSchedule
} from './service';

function* onRemoveAvailableLeaseHaulerTruckFromSchedule({ payload: { truckId, filter} }) {
    try {
        const response = yield call(removeAvailableLeaseHaulerTruckFromSchedule, filter);
        yield put(removeAvailableLeaseHaulerTruckFromScheduleSuccess({ 
            truckId, 
            response
        }));
    } catch (error) {
        yield put(removeAvailableLeaseHaulerTruckFromScheduleFailure(error));
    }
}

function* leaseHaulerRequestEditSaga() {
    yield takeEvery(REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE, onRemoveAvailableLeaseHaulerTruckFromSchedule);
}

export default leaseHaulerRequestEditSaga;