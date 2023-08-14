import { call, put, takeEvery } from 'redux-saga/effects';
import {
    SET_TRAILER_FOR_TRACTOR,
    SET_TRACTOR_FOR_TRAILER
} from './actionTypes';
import {
    setTrailerForTractorSuccess,
    setTrailerForTractorFailure,
    setTractorForTrailerSuccess,
    setTractorForTrailerFailure
} from './actions';
import {
    setTrailerForTractor,
    setTractorForTrailer
} from './service';

function* onSetTrailerForTractor({ payload: {
    truckId,
    trailerAssignment
}}) {
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

function* onSetTractorForTrailer({ payload: {
    trailerId,
    tractorAssignment
}}) {
    try {

        const response = yield call(setTractorForTrailer, tractorAssignment);
        yield put(setTractorForTrailerSuccess({
            trailerId, 
            response
        }));
    } catch (error) {
        yield put(setTractorForTrailerFailure(error));
    }
}

function* trailerAssignmentSaga() {
    yield takeEvery(SET_TRAILER_FOR_TRACTOR, onSetTrailerForTractor);
    yield takeEvery(SET_TRACTOR_FOR_TRAILER, onSetTractorForTrailer);
}

export default trailerAssignmentSaga;