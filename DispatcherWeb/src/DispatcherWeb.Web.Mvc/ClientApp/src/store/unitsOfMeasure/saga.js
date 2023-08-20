import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_UNITS_OF_MEASURE_SELECT_LIST
} from './actionTypes';
import {
    getUnitsOfMeasureSelectListSuccess,
    getUnitsOfMeasureSelectListFailure
} from './actions';
import {
    getUnitsOfMeasureSelectList
} from './service';

function* fetchUnitsOfMeasureSelectList({ payload: filter }) {
    try {
        const response = yield call(getUnitsOfMeasureSelectList, filter);
        yield put(getUnitsOfMeasureSelectListSuccess(response));
    } catch (error) {
        yield put(getUnitsOfMeasureSelectListFailure(error));
    }
}

function* unitOfMeasureSaga() {
    yield takeEvery(GET_UNITS_OF_MEASURE_SELECT_LIST, fetchUnitsOfMeasureSelectList);
}

export default unitOfMeasureSaga;