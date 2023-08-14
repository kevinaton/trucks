import {
    SET_TRAILER_FOR_TRACTOR,
    SET_TRAILER_FOR_TRACTOR_SUCCESS,
    SET_TRAILER_FOR_TRACTOR_FAILURE,
    SET_TRAILER_FOR_TRACTOR_RESET,
    SET_TRACTOR_FOR_TRAILER,
    SET_TRACTOR_FOR_TRAILER_SUCCESS,
    SET_TRACTOR_FOR_TRAILER_FAILURE,
    SET_TRACTOR_FOR_TRAILER_RESET
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

export const setTractorForTrailer = (trailerId, tractorAssignment) => ({
    type: SET_TRACTOR_FOR_TRAILER,
    payload: {
        trailerId,
        tractorAssignment
    }
});

export const setTractorForTrailerSuccess = response => ({
    type: SET_TRACTOR_FOR_TRAILER_SUCCESS,
    payload: response
});

export const setTractorForTrailerFailure = error => ({
    type: SET_TRACTOR_FOR_TRAILER_FAILURE,
    payload: error,
});

export const setTractorForTrailerReset = () => ({
    type: SET_TRACTOR_FOR_TRAILER_RESET
});