import {
    GET_UNIT_OF_MEASURES_SELECT_LIST,
    GET_UNIT_OF_MEASURES_SELECT_LIST_SUCCESS,
    GET_UNIT_OF_MEASURES_SELECT_LIST_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingUnitOfMeasuresOpts: false,
    unitOfMeasuresSelectList: null,
};

const UnitOfMeasureReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_UNIT_OF_MEASURES_SELECT_LIST:
            return {
                ...state,
                isLoadingUnitOfMeasuresOpts: true
            };
        case GET_UNIT_OF_MEASURES_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingUnitOfMeasuresOpts: false,
                unitOfMeasuresSelectList: action.payload
            };
        case GET_UNIT_OF_MEASURES_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingUnitOfMeasuresOpts: false,
                error: action.payload
            };
        default:
            return state;
    }
};

export default UnitOfMeasureReducer;