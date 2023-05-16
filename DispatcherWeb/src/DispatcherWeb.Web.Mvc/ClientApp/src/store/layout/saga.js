import { call, put, takeEvery } from 'redux-saga/effects'

import { GET_SUPPORT_LINK_ADDRESS } from './actionTypes'
import { 
    getSupportLinkAddressSuccess,
    getSupportLinkAddressFailure } from './actions'

import { getSupportLinkAddress } from './service'

function* fetchSupportLinkAddress() {
    try {
        const response = yield call(getSupportLinkAddress)
        yield put(getSupportLinkAddressSuccess(response))
    } catch (error) {
        yield put(getSupportLinkAddressFailure(error))
    }
}

function* layoutSaga() {
    yield takeEvery(GET_SUPPORT_LINK_ADDRESS, fetchSupportLinkAddress)
}

export default layoutSaga