import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_UNIT_OF_MEASURES_SELECT_LIST
} from './actionTypes';
import {
    getUnitOfMeasuresSelectListSuccess,
    getUnitOfMeasuresSelectListFailure
} from './actions';
import {
    getUnitOfMeasuresSelectList
} from './service';

function* fetchUnitOfMeasuresSelectList({ payload: filter }) {
    try {
        const response = yield call(getUnitOfMeasuresSelectList, filter);
        yield put(getUnitOfMeasuresSelectListSuccess(response));
    } catch (error) {
        yield put(getUnitOfMeasuresSelectListFailure(error));
    }
}

function* unitOfMeasuresSaga() {
    yield takeEvery(GET_UNIT_OF_MEASURES_SELECT_LIST, fetchUnitOfMeasuresSelectList);
}

export default unitOfMeasuresSaga;