import {
    GET_TENANT_SETTINGS_SUCCESS,
    GET_TENANT_SETTINGS_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    tenantSettings: null
};

const AppSettingsReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_TENANT_SETTINGS_SUCCESS:
            return {
                ...state,
                tenantSettings: action.payload
            };
        case GET_TENANT_SETTINGS_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    };  
};

export default AppSettingsReducer;