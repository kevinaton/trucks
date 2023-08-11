import {
    SET_TRAILER_FOR_TRACTOR,
    SET_TRAILER_FOR_TRACTOR_SUCCESS,
    SET_TRAILER_FOR_TRACTOR_FAILURE,
    SET_TRAILER_FOR_TRACTOR_RESET
} from './actionTypes';

export const setTrailerForTractor = (truckId, trailerAssignment) => ({
    type: SET_TRAILER_FOR_TRACTOR,
    payload: {
        truckId,
        trailerAssignment
    }
});

export const setTrailerForTractorSuccess = response => ({
    type: SET_TRAILER_FOR_TRACTOR_SUCCESS,
    payload: response
});

export const setTrailerForTractorFailure = error => ({
    type: SET_TRAILER_FOR_TRACTOR_FAILURE,
    payload: error,
});

export const setTrailerForTractorReset = () => ({
    type: SET_TRAILER_FOR_TRACTOR_RESET
});