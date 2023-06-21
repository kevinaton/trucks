import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_LINKED_USERS,
    UNLINK_USER
} from './actionTypes';
import {
    getLinkedUsersSuccess,
    getLinkedUsersFailure,
    unlinkUserSuccess,
    unlinkUserFailure
} from './actions';
import {
    getLinkedUsers,
    unlinkUser
} from './service';

function* fetchLinkedUsers() {
    try {
        const response = yield call(getLinkedUsers);
        yield put(getLinkedUsersSuccess(response));
    } catch (error) {
        yield put(getLinkedUsersFailure(error));
    }
}

function* onUnlinkUser({ payload: linkedUser }) {
    try {
        yield call(unlinkUser, linkedUser);
        yield put(unlinkUserSuccess(linkedUser));
    } catch (error) {
        yield put(unlinkUserFailure(error));
    }
}

function* userLinkSaga() {
    yield takeEvery(GET_LINKED_USERS, fetchLinkedUsers);
    yield takeEvery(UNLINK_USER, onUnlinkUser);
}

export default userLinkSaga;