import {
    SET_TRAILER_FOR_TRACTOR_SUCCESS,
    SET_TRAILER_FOR_TRACTOR_FAILURE,
    SET_TRAILER_FOR_TRACTOR_RESET
} from './actionTypes';

const INIT_STATE = {
    setTrailerForTractorResponse: null
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
        default:
            return state;
    }
};

export default TrailerAssignmentReducer;