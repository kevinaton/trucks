import {
    GET_LOCATIONS_SELECT_LIST,
    GET_LOCATIONS_SELECT_LIST_SUCCESS,
    GET_LOCATIONS_SELECT_LIST_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingLocationsOpts: false,
    locationsSelectList: null,
};

const LocationReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_LOCATIONS_SELECT_LIST:
            return {
                ...state,
                isLoadingLocationsOpts: true
            };
        case GET_LOCATIONS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingLocationsOpts: false,
                locationsSelectList: action.payload
            };
        case GET_LOCATIONS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingLocationsOpts: false,
                error: action.payload
            };
        default:
            return state;
    }
};

export default LocationReducer;