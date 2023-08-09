import {
    SET_TRAILER_FOR_TRACTOR_SUCCESS,
    SET_TRAILER_FOR_TRACTOR_FAILURE,
    SET_TRAILER_FOR_TRACTOR_RESET
} from './actionTypes';

const INIT_STATE = {
    setTrailerForTractorSuccess: null
};

const TrailerAssignmentReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case SET_TRAILER_FOR_TRACTOR_SUCCESS:
            return {
                ...state,
                setTrailerForTractorSuccess: true,
            };
        case SET_TRAILER_FOR_TRACTOR_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case SET_TRAILER_FOR_TRACTOR_RESET:
            return {
                ...state,
                setTrailerForTractorSuccess: null,
            };
        default:
            return state;
    }
};

export default TrailerAssignmentReducer;