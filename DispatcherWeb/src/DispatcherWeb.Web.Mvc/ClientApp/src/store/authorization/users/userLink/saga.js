import { call, put, takeEvery } from 'redux-saga/effects';
import {
    GET_LINKED_USERS
} from './actionTypes';
import {
    getLinkedUsersSuccess,
    getLinkedUsersFailure
} from './actions';
import {
    getLinkedUsers
} from './service';

function* fetchLinkedUsers() {
    try {
        const response = yield call(getLinkedUsers);
        yield put(getLinkedUsersSuccess(response));
    } catch (error) {
        yield put(getLinkedUsersFailure(error));
    }
}

function* userLinkSaga() {
    yield takeEvery(GET_LINKED_USERS, fetchLinkedUsers);
}

export default userLinkSaga;