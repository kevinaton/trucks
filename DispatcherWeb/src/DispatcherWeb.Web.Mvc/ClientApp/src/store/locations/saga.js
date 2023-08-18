import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_LOCATIONS_SELECT_LIST
} from './actionTypes';
import {
    getLocationsSelectListSuccess,
    getLocationsSelectListFailure
} from './actions';
import {
    getLocationsSelectList
} from './service';

function* fetchLocationsSelectList({ payload: filter }) {
    try {
        const response = yield call(getLocationsSelectList, filter);
        yield put(getLocationsSelectListSuccess(response));
    } catch (error) {
        yield put(getLocationsSelectListFailure(error));
    }
}

function* locationsSaga() {
    yield takeEvery(GET_LOCATIONS_SELECT_LIST, fetchLocationsSelectList);
}

export default locationsSaga;