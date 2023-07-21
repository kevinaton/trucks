import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_VEHICLE_CATEGORIES, 
    GET_BED_CONSTRUCTION_SELECT_LIST, 
    GET_FUEL_TYPE_SELECT_LIST,
    GET_ACTIVE_TRAILERS_SELECT_LIST,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST,
    GET_TRUCK_FOR_EDIT 
} from './actionTypes';
import { 
    getVehicleCategoriesSuccess, 
    getVehicleCategoriesFailure,
    getBedConstructionSelectListSuccess,
    getBedConstructionSelectListFailure,
    getFuelTypeSelectListSuccess,
    getFuelTypeSelectListFailure,
    getActiveTrailersSelectListSuccess,
    getActiveTrailersSelectListFailure,
    getWialonDeviceTypesSelectListSuccess,
    getWialonDeviceTypesSelectListFailure,
    getTruckForEditSuccess,
    getTruckForEditFailure,
} from './actions';
import { 
    getVehicleCategories, 
    getBedConstructionSelectList,
    getFuelTypeSelectList,
    getActiveTrailersSelectList,
    getWialonDeviceTypesSelectList,
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

function* fetchActiveTrailersSelectList() {
    try {
        const response = yield call(getActiveTrailersSelectList);
        yield put(getActiveTrailersSelectListSuccess(response));
    } catch (error) {
        yield put(getActiveTrailersSelectListFailure(error));
    }
}

function* fetchWialonDeviceTypesSelectList() {
    try {
        const response = yield call(getWialonDeviceTypesSelectList);
        yield put(getWialonDeviceTypesSelectListSuccess(response));
    } catch (error) {
        yield put(getWialonDeviceTypesSelectListFailure(error));
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
    yield takeEvery(GET_ACTIVE_TRAILERS_SELECT_LIST, fetchActiveTrailersSelectList);
    yield takeEvery(GET_WIALON_DEVICE_TYPES_SELECT_LIST, fetchWialonDeviceTypesSelectList);
    yield takeEvery(GET_TRUCK_FOR_EDIT, fetchTruckForEdit);
}

export default trucksSaga;