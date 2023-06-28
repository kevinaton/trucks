import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_LINKED_USERS,
    LINK_TO_USER,
    UNLINK_USER
} from './actionTypes';
import {
    getLinkedUsersSuccess,
    getLinkedUsersFailure,
    linkToUserSuccess,
    linkToUserFailure,
    unlinkUserSuccess,
    unlinkUserFailure
} from './actions';
import {
    getLinkedUsers,
    linkToUser,
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

function* onLinkToUser({ payload: user }) {
    try { 
        const response = yield call(linkToUser, user);
        yield put(linkToUserSuccess(response));
    } catch (error) {
        yield put(linkToUserFailure(error));
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
    yield takeEvery(LINK_TO_USER, onLinkToUser);
    yield takeEvery(UNLINK_USER, onUnlinkUser);
}

export default userLinkSaga;