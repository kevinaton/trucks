import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_ORDER_PRIORITY_SELECT_LIST,
    GET_ORDER_FOR_EDIT
} from './actionTypes';
import {
    getOrderPrioritySelectListSuccess,
    getOrderPrioritySelectListFailure,
    getOrderForEditSuccess,
    getOrderForEditFailure
} from './actions';
import { 
    getOrderPrioritySelectList,
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
    yield takeEvery(GET_ORDER_FOR_EDIT, fetchOrderForEdit);
}

export default ordersSaga;