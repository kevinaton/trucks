import {
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_SUCCESS,
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_FAILURE,
    REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_RESET
} from './actionTypes';

const INIT_STATE = {
    removeAvailableLeaseHaulerTruckFromScheduleResponse: null
};

const LeaseHaulerRequestEditReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_SUCCESS:
            {
                const { truckId } = action.payload;
                return {
                    ...state,
                    removeAvailableLeaseHaulerTruckFromScheduleResponse: {
                        truckId,
                        success: true
                    }
                };
            }
        case REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_FAILURE:
            return {
                ...state,
                removeAvailableLeaseHaulerTruckFromScheduleResponse: {
                    success: false
                },
                error: action.payload
            };
        case REMOVE_AVAILABLE_LEASE_HAULER_TRUCK_FROM_SCHEDULE_RESET:
            return {
                ...state,
                removeAvailableLeaseHaulerTruckFromScheduleResponse: null,
            };
        default:
            return state;
    }
};

export default LeaseHaulerRequestEditReducer;