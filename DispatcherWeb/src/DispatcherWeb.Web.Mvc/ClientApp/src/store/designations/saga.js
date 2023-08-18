import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_DESIGNATIONS_SELECT_LIST
} from './actionTypes';
import {
    getDesignationsSelectListSuccess,
    getDesignationsSelectListFailure
} from './actions';
import {
    getDesignationsSelectList
} from './service';

function* fetchDesignationsSelectList() {
    try {
        const response = yield call(getDesignationsSelectList);
        yield put(getDesignationsSelectListSuccess(response));
    } catch (error) {
        yield put(getDesignationsSelectListFailure(error));
    }
}

function* designationsSaga() {
    yield takeEvery(GET_DESIGNATIONS_SELECT_LIST, fetchDesignationsSelectList);
}

export default designationsSaga;