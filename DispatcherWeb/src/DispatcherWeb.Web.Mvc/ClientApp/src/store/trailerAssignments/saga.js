import { call, put, takeEvery } from 'redux-saga/effects';
import {
    SET_TRAILER_FOR_TRACTOR
} from './actionTypes';
import {
    setTrailerForTractorSuccess,
    setTrailerForTractorFailure
} from './actions';
import {
    setTrailerForTractor
} from './service';

function* onSetTrailerForTractor({ payload: {
    truckId,
    trailerAssignment
} }) {
    try {

        const response = yield call(setTrailerForTractor, trailerAssignment);
        yield put(setTrailerForTractorSuccess({
            truckId, 
            response
        }));
    } catch (error) {
        yield put(setTrailerForTractorFailure(error));
    }
}

function* trailerAssignmentSaga() {
    yield takeEvery(SET_TRAILER_FOR_TRACTOR, onSetTrailerForTractor);
}

export default trailerAssignmentSaga;