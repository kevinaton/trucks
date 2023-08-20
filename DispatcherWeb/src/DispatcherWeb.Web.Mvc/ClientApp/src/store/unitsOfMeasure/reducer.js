import {
    GET_UNITS_OF_MEASURE_SELECT_LIST,
    GET_UNITS_OF_MEASURE_SELECT_LIST_SUCCESS,
    GET_UNITS_OF_MEASURE_SELECT_LIST_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingUnitOfMeasureOpts: false,
    unitsOfMeasureSelectList: null,
};

const UnitOfMeasureReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_UNITS_OF_MEASURE_SELECT_LIST:
            return {
                ...state,
                isLoadingUnitOfMeasureOpts: true
            };
        case GET_UNITS_OF_MEASURE_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingUnitOfMeasureOpts: false,
                unitsOfMeasureSelectList: action.payload
            };
        case GET_UNITS_OF_MEASURE_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingUnitOfMeasureOpts: false,
                error: action.payload
            };
        default:
            return state;
    }
};

export default UnitOfMeasureReducer;