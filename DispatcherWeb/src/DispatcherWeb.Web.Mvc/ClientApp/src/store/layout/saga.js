import { call, put, takeEvery } from 'redux-saga/effects'

import {
    GET_MENU_ITEMS, 
    GET_SUPPORT_LINK_ADDRESS } from './actionTypes'
import { 
    getMenuItemsSuccess,
    getMenuItemsFailure,
    getSupportLinkAddressSuccess,
    getSupportLinkAddressFailure } from './actions'

import { 
    getMenuItems, 
    getSupportLinkAddress } from './service'

function* fetchMenuItems() {
    try {
        const response = yield call(getMenuItems)
        yield put(getMenuItemsSuccess(response))
    } catch (error) {
        yield put(getMenuItemsFailure(error))
    }
}

function* fetchSupportLinkAddress() {
    try {
        const response = yield call(getSupportLinkAddress)
        yield put(getSupportLinkAddressSuccess(response))
    } catch (error) {
        yield put(getSupportLinkAddressFailure(error))
    }
}

function* layoutSaga() {
    yield takeEvery(GET_MENU_ITEMS, fetchMenuItems)
    yield takeEvery(GET_SUPPORT_LINK_ADDRESS, fetchSupportLinkAddress)
}

export default layoutSaga