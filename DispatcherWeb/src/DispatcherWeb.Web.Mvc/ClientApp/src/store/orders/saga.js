import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_ORDER_FOR_EDIT
} from './actionTypes';
import {
    getOrderForEditSuccess,
    getOrderForEditFailure
} from './actions';
import { 
    getOrderForEdit
} from './service';

function* fetchOrderForEdit({ payload: input }) {
    try {
        const response = yield call(getOrderForEdit, input);
        yield put(getOrderForEditSuccess(response));
    } catch (error) {
        yield put(getOrderForEditFailure(error));
    }
}

function* ordersSaga() {
    yield takeEvery(GET_ORDER_FOR_EDIT, fetchOrderForEdit);
}

export default ordersSaga;