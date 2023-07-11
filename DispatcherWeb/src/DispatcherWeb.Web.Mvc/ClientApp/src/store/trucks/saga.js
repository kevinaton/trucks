import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_VEHICLE_CATEGORIES } from './actionTypes';
import { getVehicleCategoriesSuccess, getVehicleCategoriesFailure } from './actions';
import { getVehicleCategories } from './service';

function* fetchVehicleCategories() {
    try {
        const response = yield call(getVehicleCategories);
        yield put(getVehicleCategoriesSuccess(response));
    } catch (error) {
        yield put(getVehicleCategoriesFailure(error));
    }
}

function* trucksSaga() {
    yield takeEvery(GET_VEHICLE_CATEGORIES, fetchVehicleCategories);
}

export default trucksSaga;