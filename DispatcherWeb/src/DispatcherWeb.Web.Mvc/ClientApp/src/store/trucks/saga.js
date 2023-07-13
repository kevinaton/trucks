import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_VEHICLE_CATEGORIES, GET_TRUCK_FOR_EDIT } from './actionTypes';
import { 
    getVehicleCategoriesSuccess, 
    getVehicleCategoriesFailure,
    getTruckForEditSuccess,
    getTruckForEditFailure,
} from './actions';
import { getVehicleCategories, getTruckForEdit } from './service';

function* fetchVehicleCategories() {
    try {
        const response = yield call(getVehicleCategories);
        yield put(getVehicleCategoriesSuccess(response));
    } catch (error) {
        yield put(getVehicleCategoriesFailure(error));
    }
}

function* fetchTruckForEdit(action) {
    try {
        const response = yield call(getTruckForEdit, action.payload);
        yield put(getTruckForEditSuccess(response));
    } catch (error) {
        yield put(getTruckForEditFailure(error));
    }
};

function* trucksSaga() {
    yield takeEvery(GET_VEHICLE_CATEGORIES, fetchVehicleCategories);
    yield takeEvery(GET_TRUCK_FOR_EDIT, fetchTruckForEdit);
}

export default trucksSaga;