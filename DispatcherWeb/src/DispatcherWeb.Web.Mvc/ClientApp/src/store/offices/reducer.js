import { 
    GET_OFFICES_SUCCESS,
    GET_OFFICES_FAILURE
} from './actionTypes';

const INIT_STATE = {
    offices: null
};

const OfficeReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_OFFICES_SUCCESS:
            return {
                ...state,
                offices: action.payload
            };
        case GET_OFFICES_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    }
};

export default OfficeReducer;