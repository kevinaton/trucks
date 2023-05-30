import { call, put, takeEvery } from 'redux-saga/effects';
import { GET_USER_INFO } from './actionTypes';
import { getUserInfoSuccess, getUserInfoFailure } from './actions';
import { getUserInfo } from './service';

function* fetchUserInfo() {
    try {
        const response = yield call(getUserInfo);
        yield put(getUserInfoSuccess(response));
    } catch (error) {
        yield put(getUserInfoFailure(error));
    }
}

function* userSaga() {
    yield takeEvery(GET_USER_INFO, fetchUserInfo);
}

export default userSaga;