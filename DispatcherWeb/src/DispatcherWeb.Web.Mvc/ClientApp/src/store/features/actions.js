import { 
    CHECK_IF_ENABLED,
    CHECK_IF_ENABLED_SUCCESS,
    CHECK_IF_ENABLED_FAILURE
} from './actionTypes';

export const checkIfEnabled = featureName => ({
    type: CHECK_IF_ENABLED,
    payload: featureName
});

export const checkIfEnabledSuccess = feature => ({
    type: CHECK_IF_ENABLED_SUCCESS,
    payload: feature
});

export const checkIfEnabledFailure = error => ({
    type: CHECK_IF_ENABLED_FAILURE,
    payload: error
});