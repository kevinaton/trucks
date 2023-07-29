import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_PAGE_CONFIG,
    GET_SCHEDULE_TRUCKS, 
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST,
    GET_SCHEDULE_ORDERS 
} from './actionTypes';
import { 
    getSchedulePageConfigSuccess,
    getSchedulePageConfigFailure,
    getScheduleTrucksSuccess, 
    getScheduleTrucksFailure,
    getScheduleTruckBySyncRequestSuccess,
    getScheduleTruckBySyncRequestFailure, 
    getScheduleOrdersSuccess, 
    getScheduleOrdersFailure 
} from './actions';
import { 
    getPageConfig,
    getScheduleTrucks, 
    getScheduleOrders 
} from './service';

function* fetchPageConfig() {
    try {
        const response = yield call(getPageConfig);
        yield put(getSchedulePageConfigSuccess(response));
    } catch (error) {
        yield put(getSchedulePageConfigFailure(error));
    }
}

function* fetchScheduleTrucks({ payload: filter }) {
    try {
        const response = yield call(getScheduleTrucks, filter);
        yield put(getScheduleTrucksSuccess(response));
    } catch (error) {
        yield put(getScheduleTrucksFailure(error));
    }
}

function* fetchScheduleTruckBySyncRequest({ payload: filter }) {
    try {
        const response = yield call(getScheduleTrucks, filter);
        yield put(getScheduleTruckBySyncRequestSuccess(response));
    } catch (error) {
        yield put(getScheduleTruckBySyncRequestFailure(error));
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
    yield takeEvery(GET_PAGE_CONFIG, fetchPageConfig);
    yield takeEvery(GET_SCHEDULE_TRUCKS, fetchScheduleTrucks);
    yield takeEvery(GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST, fetchScheduleTruckBySyncRequest);
    yield takeEvery(GET_SCHEDULE_ORDERS, fetchScheduleOrders);
}

export default schedulingSaga;