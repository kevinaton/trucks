import { 
    GET_DESIGNATIONS_SELECT_LIST,
    GET_DESIGNATIONS_SELECT_LIST_SUCCESS,
    GET_DESIGNATIONS_SELECT_LIST_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    isLoadingDesignationsOpts: false,
    designationsSelectList: null,
};

const DesignationReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_DESIGNATIONS_SELECT_LIST:
            return {
                ...state,
                isLoadingDesignationsOpts: true
            };
        case GET_DESIGNATIONS_SELECT_LIST_SUCCESS:
            return {
                ...state,
                isLoadingDesignationsOpts: false,
                designationsSelectList: action.payload
            };
        case GET_DESIGNATIONS_SELECT_LIST_FAILURE:
            return {
                ...state,
                isLoadingDesignationsOpts: false,
                error: action.payload
            };
        default:
            return state;
    }
};

export default DesignationReducer;