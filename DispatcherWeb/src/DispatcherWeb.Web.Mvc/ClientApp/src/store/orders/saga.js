import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_ORDER_PRIORITY_SELECT_LIST,
    GET_JOB_FOR_EDIT,
    GET_ORDER_FOR_EDIT
} from './actionTypes';
import {
    getOrderPrioritySelectListSuccess,
    getOrderPrioritySelectListFailure,
    getJobForEditSuccess,
    getJobForEditFailure,
    getOrderForEditSuccess,
    getOrderForEditFailure
} from './actions';
import { 
    getOrderPrioritySelectList,
    getJobForEdit,
    getOrderForEdit
} from './service';

function* fetchOrderPrioritySelectList() {
    try {
        const response = yield call(getOrderPrioritySelectList);
        yield put(getOrderPrioritySelectListSuccess(response));
    } catch (error) {
        yield put(getOrderPrioritySelectListFailure(error));
    }
}

function* fetchJobForEdit({ payload: input }) {
    try {
        const response = yield call(getJobForEdit, input);
        yield put(getJobForEditSuccess(response));
    } catch (error) {
        yield put(getJobForEditFailure(error));
    }
}

function* fetchOrderForEdit({ payload: input }) {
    try {
        const response = yield call(getOrderForEdit, input);
        yield put(getOrderForEditSuccess(response));
    } catch (error) {
        yield put(getOrderForEditFailure(error));
    }
}

function* ordersSaga() {
    yield takeEvery(GET_ORDER_PRIORITY_SELECT_LIST, fetchOrderPrioritySelectList);
    yield takeEvery(GET_JOB_FOR_EDIT, fetchJobForEdit);
    yield takeEvery(GET_ORDER_FOR_EDIT, fetchOrderForEdit);
}

export default ordersSaga;