import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_SCHEDULE_TRUCKS, GET_SCHEDULE_ORDERS } from './actionTypes';
import { 
    getScheduleTrucksSuccess, 
    getScheduleTrucksFailure, 
    getScheduleOrdersSuccess, 
    getScheduleOrdersFailure 
} from './actions';
import { getScheduleTrucks, getScheduleOrders } from './service';

function* fetchScheduleTrucks({ payload: filter }) {
    try {
        const response = yield call(getScheduleTrucks, filter);
        yield put(getScheduleTrucksSuccess(response));
    } catch (error) {
        yield put(getScheduleTrucksFailure(error));
    }
}

function* fetchScheduleOrders({ payload: filter }) {
    try {
        const response = yield call(getScheduleOrders, filter);
        yield put(getScheduleOrdersSuccess(response));
    } catch (error) {
        yield put(getScheduleOrdersFailure(error));
    }
}

function* schedulingSaga() {
    yield takeEvery(GET_SCHEDULE_TRUCKS, fetchScheduleTrucks);
    yield takeEvery(GET_SCHEDULE_ORDERS, fetchScheduleOrders);
}

export default schedulingSaga;