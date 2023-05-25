import { 
    GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_SUCCESS,
    GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    htmlView: null
};

const DashboardReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_SUCCESS:
            return {
                ...state,
                htmlView: action.payload
            };
        case GET_SCHEDULED_TRUCK_COUNT_PARTIAL_VIEW_FAILURE:
            return {
                ...state,
                error: action.payload
            };
        default:
            return state;
    };
};

export default DashboardReducer;