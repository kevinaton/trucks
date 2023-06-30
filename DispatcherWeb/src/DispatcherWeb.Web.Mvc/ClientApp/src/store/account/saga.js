import { call, put, takeEvery } from 'redux-saga/effects';
import { 
    BACK_TO_IMPERSONATOR, 
    IMPERSONATE_SIGNIN,
    SWITCH_TO_USER
} from './actionTypes';
import { 
    backToImpersonatorSuccess, 
    backToImpersonatorFailure,
    impersonateSigninSuccess,
    impersonateSigninFailure,
    switchToUserSuccess,
    switchToUserFailure
} from './actions';
import { 
    backToImpersonator, 
    impersonateSignin,
    switchToUser
} from './service';

function* onBackToImpersonator() {
    try {
        const response = yield call(backToImpersonator);
        yield put(backToImpersonatorSuccess(response));
    } catch (error) {
        yield put(backToImpersonatorFailure(error));
    }
}

function* onImpersonateSignin({ payload: tokenId }) {
    try {
        const response = yield call(impersonateSignin, tokenId);
        yield put(impersonateSigninSuccess(response));
    } catch (error) {
        yield put(impersonateSigninFailure(error));
    }
}

function* onSwitchToUser({ payload: account }) {
    try {
        const response = yield call(switchToUser, account);
        yield put(switchToUserSuccess(response));
    } catch (error) {
        yield put(switchToUserFailure(error));
    }
}

function* accountSaga() {
    yield takeEvery(BACK_TO_IMPERSONATOR, onBackToImpersonator);
    yield takeEvery(IMPERSONATE_SIGNIN, onImpersonateSignin);
    yield takeEvery(SWITCH_TO_USER, onSwitchToUser);
}

export default accountSaga;