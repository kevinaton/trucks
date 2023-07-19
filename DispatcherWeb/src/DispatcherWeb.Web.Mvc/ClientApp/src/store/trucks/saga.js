import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_VEHICLE_CATEGORIES, 
    GET_BED_CONSTRUCTION_SELECT_LIST, 
    GET_FUEL_TYPE_SELECT_LIST,
    GET_TRUCK_FOR_EDIT } from './actionTypes';
import { 
    getVehicleCategoriesSuccess, 
    getVehicleCategoriesFailure,
    getBedConstructionSelectListSuccess,
    getBedConstructionSelectListFailure,
    getFuelTypeSelectListSuccess,
    getFuelTypeSelectListFailure,
    getTruckForEditSuccess,
    getTruckForEditFailure,
} from './actions';
import { 
    getVehicleCategories, 
    getBedConstructionSelectList,
    getFuelTypeSelectList,
    getTruckForEdit 
} from './service';

function* fetchVehicleCategories() {
    try {
        const response = yield call(getVehicleCategories);
        yield put(getVehicleCategoriesSuccess(response));
    } catch (error) {
        yield put(getVehicleCategoriesFailure(error));
    }
}

function* fetchBedConstructionSelectList() {
    try {
        const response = yield call(getBedConstructionSelectList);
        yield put(getBedConstructionSelectListSuccess(response));
    } catch (error) {
        yield put(getBedConstructionSelectListFailure(error));
    }
}

function* fetchFuelTypeSelectList() {
    try {
        const response = yield call(getFuelTypeSelectList);
        yield put(getFuelTypeSelectListSuccess(response));
    } catch (error) {
        yield put(getFuelTypeSelectListFailure(error));
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
    yield takeEvery(GET_BED_CONSTRUCTION_SELECT_LIST, fetchBedConstructionSelectList);
    yield takeEvery(GET_FUEL_TYPE_SELECT_LIST, fetchFuelTypeSelectList);
    yield takeEvery(GET_TRUCK_FOR_EDIT, fetchTruckForEdit);
}

export default trucksSaga;