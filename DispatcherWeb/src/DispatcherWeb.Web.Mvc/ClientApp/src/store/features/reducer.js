import {
    CHECK_IF_ENABLED_SUCCESS,
    CHECK_IF_ENABLED_FAILURE
} from './actionTypes';

const INIT_STATE = {
    isEnabled: null
};

const FeatureReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case CHECK_IF_ENABLED_SUCCESS:
            return {
                ...state,
                isEnabled: action.payload
            };
        case CHECK_IF_ENABLED_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    }
};

export default FeatureReducer;