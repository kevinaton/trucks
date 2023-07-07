import {
    GET_TENANT_SETTINGS,
    GET_TENANT_SETTINGS_SUCCESS,
    GET_TENANT_SETTINGS_FAILURE,
} from './actionTypes';

export const getTenantSettings = () => ({
    type: GET_TENANT_SETTINGS
});

export const getTenantSettingsSuccess = tenantSettings => ({
    type: GET_TENANT_SETTINGS_SUCCESS,
    payload: tenantSettings
});

export const getTenantSettingsFailure = error => ({
    type: GET_TENANT_SETTINGS_FAILURE,
    payload: error
});