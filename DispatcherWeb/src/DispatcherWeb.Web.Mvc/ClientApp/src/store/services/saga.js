import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_SERVICES_WITH_TAX_INFO_SELECT_LIST
} from './actionTypes';
import {
    getServicesWithTaxInfoSelectListSuccess,
    getServicesWithTaxInfoSelectListFailure
} from './actions';
import {
    getServicesWithTaxInfoSelectList
} from './service';

function* fetchServicesWithTaxInfoSelectList({ payload: filter }) {
    try {
        const response = yield call(getServicesWithTaxInfoSelectList, filter);
        yield put(getServicesWithTaxInfoSelectListSuccess(response));
    } catch (error) {
        yield put(getServicesWithTaxInfoSelectListFailure(error));
    }
}

function* servicesSaga() {
    yield takeEvery(GET_SERVICES_WITH_TAX_INFO_SELECT_LIST, fetchServicesWithTaxInfoSelectList);
}

export default servicesSaga;