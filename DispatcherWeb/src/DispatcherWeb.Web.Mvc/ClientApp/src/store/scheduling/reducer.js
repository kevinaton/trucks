import _ from 'lodash';
import {
    GET_PAGE_CONFIG_SUCCESS,
    GET_PAGE_CONFIG_FAILURE,
    GET_SCHEDULE_TRUCKS,
    GET_SCHEDULE_TRUCKS_SUCCESS,
    GET_SCHEDULE_TRUCKS_FAILURE,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_SUCCESS,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_FAILURE,
    GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_RESET,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
} from './actionTypes';

const INIT_STATE = {
    schedulePageConfig: null,
    isLoadingScheduleTrucks: false,
    scheduleTrucks: null,
    isModifiedScheduleTrucks: false,
    scheduleOrders: null,
};

const SchedulingReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_PAGE_CONFIG_SUCCESS:
            return {
                ...state,
                schedulePageConfig: action.payload,
            };
        case GET_PAGE_CONFIG_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_SCHEDULE_TRUCKS:
            return {
                ...state,
                isLoadingScheduleTrucks: true
            };
        case GET_SCHEDULE_TRUCKS_SUCCESS:
            return {
                ...state,
                scheduleTrucks: action.payload,
                isLoadingScheduleTrucks: false
            };
        case GET_SCHEDULE_TRUCKS_FAILURE:
            return {
                ...state,
                scheduleTrucks: null,
                isLoadingScheduleTrucks: false,
                error: action.payload,
            };
        case GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_SUCCESS: {
            const payload = action.payload.result;
            if (payload === null) break;

            let result = {
                items: []
            };
            if (state.scheduleTrucks !== null && state.scheduleTrucks.result !== null) {
                let { items } = state.scheduleTrucks.result;
                if (items !== null) {
                    const index = _.findIndex(items, { id: parseInt(payload.id) });
                    if (index !== -1) {
                        items[index] = payload;
                    } else {
                        items.push(payload);
                    }

                    if (items.length > 1) {
                        // order by isExternal in ascending order (true first)
                        // then by isPowered in descending order (false first)
                        // then by truckCode in ascending order
                        items = _.orderBy(items, [
                            'isExternal', 
                            'vehicleCategory.isPowered',
                            'truckCode'
                        ], [
                            'asc',
                            'desc',
                            'asc'
                        ]);
                    }
                } else {
                    items = [payload];
                }
                
                result = {
                    ...result,
                    items
                }
            } else {
                result = {
                    ...result,
                    items: [payload]
                }
            }

            return {
                ...state,
                scheduleTrucks: {
                    ...state.scheduleTrucks,
                    result
                },
                isModifiedScheduleTrucks: true,
            };
        }
        case GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_FAILURE:
            return {
                ...state,
                error: action.payload,
            };
        case GET_SCHEDULE_TRUCK_BY_SYNC_REQUEST_RESET:
            return {
                ...state,
                isModifiedScheduleTrucks: false,
            };
        case GET_SCHEDULE_ORDERS_SUCCESS:
            return {
                ...state,
                scheduleOrders: action.payload,
            };
        case GET_SCHEDULE_ORDERS_FAILURE:
            return {
                ...state,
                scheduleOrders: [],
                error: action.payload,
            };
        default:
            return state;
    }
};

export default SchedulingReducer;