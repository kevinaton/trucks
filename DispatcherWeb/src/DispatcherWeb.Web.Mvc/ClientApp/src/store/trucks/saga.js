import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    GET_VEHICLE_CATEGORIES, 
    GET_BED_CONSTRUCTION_SELECT_LIST, 
    GET_BED_CONSTRUCTIONS,
    GET_FUEL_TYPE_SELECT_LIST,
    GET_ACTIVE_TRAILERS_SELECT_LIST,
    GET_MAKES_SELECT_LIST,
    GET_MODELS_SELECT_LIST,
    GET_WIALON_DEVICE_TYPES_SELECT_LIST,
    GET_TRUCK_FOR_EDIT,
    EDIT_TRUCK,
    SET_TRUCK_IS_OUT_OF_SERVICE
} from './actionTypes';
import { 
    getVehicleCategoriesSuccess, 
    getVehicleCategoriesFailure,
    getBedConstructionSelectListSuccess,
    getBedConstructionSelectListFailure,
    getBedConstructionsSuccess,
    getBedConstructionsFailure,
    getFuelTypeSelectListSuccess,
    getFuelTypeSelectListFailure,
    getActiveTrailersSelectListSuccess,
    getActiveTrailersSelectListFailure,
    getMakesSelectListSuccess,
    getMakesSelectListFailure,
    getModelsSelectListSuccess,
    getModelsSelectListFailure,
    getWialonDeviceTypesSelectListSuccess,
    getWialonDeviceTypesSelectListFailure,
    getTruckForEditSuccess,
    getTruckForEditFailure,
    editTruckSuccess,
    editTruckFailure,
    setTruckIsOutOfServiceSuccess,
    setTruckIsOutOfServiceFailure,
} from './actions';
import { 
    getVehicleCategories, 
    getBedConstructionSelectList,
    getBedConstructions,
    getFuelTypeSelectList,
    getActiveTrailersSelectList,
    getMakesSelectList,
    getModelsSelectList,
    getWialonDeviceTypesSelectList,
    getTruckForEdit,
    editTruck,
    setTruckIsOutOfService
} from './service';

function* fetchVehicleCategories({ payload: filter }) {
    try {
        const response = yield call(getVehicleCategories, filter);
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

function* fetchBedConstructions({ payload: filter }) {
    try {
        const response = yield call(getBedConstructions, filter);
        yield put(getBedConstructionsSuccess(response));
    } catch (error) {
        yield put(getBedConstructionsFailure(error));
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

function* fetchActiveTrailersSelectList({ payload: filter }) {
    try {
        const response = yield call(getActiveTrailersSelectList, filter);
        yield put(getActiveTrailersSelectListSuccess(response));
    } catch (error) {
        yield put(getActiveTrailersSelectListFailure(error));
    }
}

function* fetchMakesSelectList({ payload: filter }) {
    try {
        const response = yield call(getMakesSelectList, filter);
        yield put(getMakesSelectListSuccess(response));
    } catch (error) {
        yield put(getMakesSelectListFailure(error));
    }
}

function* fetchModelsSelectList({ payload: filter }) {
    try {
        const response = yield call(getModelsSelectList, filter);
        yield put(getModelsSelectListSuccess(response));
    } catch (error) {
        yield put(getModelsSelectListFailure(error));
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

function* fetchTruckForEdit({ payload: input }) {
    try {
        const response = yield call(getTruckForEdit, input);
        yield put(getTruckForEditSuccess(response));
    } catch (error) {
        yield put(getTruckForEditFailure(error));
    }
};

function* onEditTruck({ payload: truck }) {
    try {
        const response = yield call(editTruck, truck);
        yield put(editTruckSuccess(response));
    } catch (error) {
        yield put(editTruckFailure(error));
    }
}

function* onSetTruckIsOutOfService({ payload: truck }) {
    try {
        const response = yield call(setTruckIsOutOfService, truck);
        yield put(setTruckIsOutOfServiceSuccess(response, truck.isOutOfService));
    } catch (error) {
        yield put(setTruckIsOutOfServiceFailure(error));
    }
}

function* trucksSaga() {
    yield takeEvery(GET_VEHICLE_CATEGORIES, fetchVehicleCategories);
    yield takeEvery(GET_BED_CONSTRUCTION_SELECT_LIST, fetchBedConstructionSelectList);
    yield takeEvery(GET_BED_CONSTRUCTIONS, fetchBedConstructions);
    yield takeEvery(GET_FUEL_TYPE_SELECT_LIST, fetchFuelTypeSelectList);
    yield takeEvery(GET_ACTIVE_TRAILERS_SELECT_LIST, fetchActiveTrailersSelectList);
    yield takeEvery(GET_MAKES_SELECT_LIST, fetchMakesSelectList);
    yield takeEvery(GET_MODELS_SELECT_LIST, fetchModelsSelectList);
    yield takeEvery(GET_WIALON_DEVICE_TYPES_SELECT_LIST, fetchWialonDeviceTypesSelectList);
    yield takeEvery(GET_TRUCK_FOR_EDIT, fetchTruckForEdit);
    yield takeEvery(EDIT_TRUCK, onEditTruck);
    yield takeEvery(SET_TRUCK_IS_OUT_OF_SERVICE, onSetTruckIsOutOfService);
}

export default trucksSaga;