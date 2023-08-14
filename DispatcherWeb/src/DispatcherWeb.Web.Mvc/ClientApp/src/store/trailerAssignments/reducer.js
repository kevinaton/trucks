import {
    SET_TRAILER_FOR_TRACTOR_SUCCESS,
    SET_TRAILER_FOR_TRACTOR_FAILURE,
    SET_TRAILER_FOR_TRACTOR_RESET,
    SET_TRACTOR_FOR_TRAILER_SUCCESS,
    SET_TRACTOR_FOR_TRAILER_FAILURE,
    SET_TRACTOR_FOR_TRAILER_RESET
} from './actionTypes';

const INIT_STATE = {
    setTrailerForTractorResponse: null,
    setTractorForTrailerResponse: null,
};

const TrailerAssignmentReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case SET_TRAILER_FOR_TRACTOR_SUCCESS:
            {
                const { truckId } = action.payload;
                return {
                    ...state,
                    setTrailerForTractorResponse: {
                        truckId,
                        success: true
                    },
                };
            }
        case SET_TRAILER_FOR_TRACTOR_FAILURE:
            return {
                ...state,
                setTrailerForTractorResponse: {
                    success: false
                },
                error: action.payload,
            };
        case SET_TRAILER_FOR_TRACTOR_RESET:
            return {
                ...state,
                setTrailerForTractorResponse: null,
            };
        case SET_TRACTOR_FOR_TRAILER_SUCCESS:
            {
                const { trailerId } = action.payload;
                return {
                    ...state,
                    setTractorForTrailerResponse: {
                        trailerId,
                        success: true
                    }
                };
            }
        case SET_TRACTOR_FOR_TRAILER_FAILURE:
            return {
                ...state,
                setTractorForTrailerResponse: {
                    success: false
                },
                error: action.payload,
            }
        case SET_TRACTOR_FOR_TRAILER_RESET:
            return {
                ...state,
                setTractorForTrailerResponse: null,
            };
        default:
            return state;
    }
};

export default TrailerAssignmentReducer;