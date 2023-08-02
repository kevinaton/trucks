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
    GET_SCHEDULE_ORDERS,
    GET_SCHEDULE_ORDERS_SUCCESS,
    GET_SCHEDULE_ORDERS_FAILURE,
    REMOVE_TRUCK_FROM_SCHEDULE,
} from './actionTypes';

const INIT_STATE = {
    schedulePageConfig: null,
    isLoadingScheduleTrucks: false,
    scheduleTrucks: null,
    isModifiedScheduleTrucks: false,
    isLoadingScheduleOrders: false,
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
                    const newItems = payload.items;
                    _.forEach(newItems, (newItem) => {
                        const index = _.findIndex(items, { id: parseInt(newItem.id) });
                        if (index !== -1) {
                            items[index] = newItem;
                        } else {
                            items.push(newItem);
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
                    });
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
        case GET_SCHEDULE_ORDERS:
            return {
                ...state,
                isLoadingScheduleOrders: true
            };
        case GET_SCHEDULE_ORDERS_SUCCESS:
            return {
                ...state,
                scheduleOrders: action.payload,
                isLoadingScheduleOrders: false
            };
        case GET_SCHEDULE_ORDERS_FAILURE:
            return {
                ...state,
                scheduleOrders: null,
                isLoadingScheduleOrders: false,
                error: action.payload,
            };
        case REMOVE_TRUCK_FROM_SCHEDULE: {
            const payload = action.payload;
            if (payload === null) break;

            let result = {
                items: []
            };

            if (state.scheduleTrucks !== null && state.scheduleTrucks.result !== null) {
                let { items } = state.scheduleTrucks.result;
                if (items !== null) {
                    _.forEach(payload, (newItem) => {
                        console.log('newItem: ', newItem)
                        const index = _.findIndex(items, { id: parseInt(newItem) });
                        if (index !== -1) {
                            items.splice(index, 1);
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
                    });
                }
                    
                result = {
                    ...result,
                    items
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
        default: return state;
    }
};

export default SchedulingReducer;