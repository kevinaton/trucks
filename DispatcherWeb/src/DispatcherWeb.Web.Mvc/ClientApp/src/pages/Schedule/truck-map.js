import React, { useEffect, useState, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Paper,
    Chip,
    Grid
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import _, { isEmpty } from 'lodash';
import { 
    checkIfEnabled,
    getUserSettingByName, 
    getScheduleTrucks 
} from '../../store/actions';

const TruckMap = ({
    dataFilter
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [validateUtilization, setValidateUtilization] = useState({
        settingsKey: 'App.DispatchingAndMessaging.ValidateUtilization',
        isEnabled: null
    });
    const [leaseHaulers, setLeaseHaulers] = useState({
        feature: 'App.AllowLeaseHaulersFeature',
        isEnabled: null
    });
    const [trucks, setTrucks] = useState([]);

    const dispatch = useDispatch();
    const { 
        feature,
        userSettings, 
        scheduleTrucks 
    } = useSelector((state) => ({
        feature: state.FeatureReducer.feature,
        userSettings: state.UserReducer.userSettings,
        scheduleTrucks: state.SchedulingReducer.scheduleTrucks,
    }));

    useEffect(() => {
        if (validateUtilization.isEnabled === null) {
            dispatch(getUserSettingByName(validateUtilization.settingsKey));
        }
    }, [dispatch, validateUtilization]);

    useEffect(() => {
        if (leaseHaulers.isEnabled === null) {
            dispatch(checkIfEnabled(leaseHaulers.feature));
        }
    }, [dispatch, leaseHaulers]);
    
    useEffect(() => {
        if (!isEmpty(userSettings) && !isEmpty(userSettings.result)) {
            const { name, value } = userSettings.result;
            if (validateUtilization.isEnabled === null && name === validateUtilization.settingsKey) {
                setValidateUtilization({
                    ...validateUtilization,
                    isEnabled: Boolean(value)
                });
            }
        }
    }, [userSettings, validateUtilization]);

    useEffect(() => {
        if (!isEmpty(feature) && !isEmpty(feature.result)) {
            const { name, value } = feature.result;
            if (leaseHaulers.isEnabled === null && name === leaseHaulers.feature) {
                setLeaseHaulers({
                    ...leaseHaulers,
                    isEnabled: value
                });
            }
        }
    }, [feature, leaseHaulers]);

    // useEffect(() => {
    //     if (!isEmpty(dataFilter) && isEmpty(trucks) && !isLoading) {
    //         const { officeId, date } = dataFilter;
    //         if (officeId !== null && date !== null) {
    //             setLoading(true);
    //             dispatch(getScheduleTrucks({
    //                 officeId,
    //                 date: date
    //             }));
    //         }
    //     }
    // }, [dispatch, isLoading, dataFilter, trucks]);
    
    useEffect(() => {
        if (isLoading && 
            !isEmpty(scheduleTrucks) && 
            !isEmpty(scheduleTrucks.result)
        ) {
            const { items } = scheduleTrucks.result;
            if (!isEmpty(items) && (
                isEmpty(trucks) || (!isEmpty(trucks) && !_.isEqual(trucks, items))
            )) {
                setTrucks(items);
                setLoading(false);
            }
        }
    }, [isLoading, scheduleTrucks, trucks]);

    useEffect(() => {
        // check if dataFilter has changed from its previous state
        if (
            prevDataFilterRef.current.officeId !== dataFilter.officeId ||
            prevDataFilterRef.current.date !== dataFilter.date
            // prevDataFilterRef.current.hideCompletedOrders !== dataFilter.hideCompletedOrders ||
            // prevDataFilterRef.current.hideProgressBar !== dataFilter.hideProgressBar ||
            // prevDataFilterRef.current.sorting !== dataFilter.sorting
        ) {
            const fetchData = async () => {
                const { officeId, date } = dataFilter;
                if (officeId !== null && date !== null) {
                    setLoading(true);
                    dispatch(getScheduleTrucks({
                        officeId,
                        date: date
                    }));
                }
            };

            fetchData();

            // update the previous dataFilter value
            prevDataFilterRef.current = dataFilter;
        }
    }, [dispatch, dataFilter]);

    useEffect(() => {
        // cleanup logic
        return () => {
            // reset the previous dataFilter value
            prevDataFilterRef.current = null;
        };
    }, []);

    const truckHasNoDriver = truck => !truck.isExternal && 
        (truck.hasNoDriver || (!truck.hasDefaultDriver && !truck.hasDriverAssignment));

    const truckCategoryNeedsDriver = truck => 
        truck.vehicleCategory.isPowered && 
        ((!truck.alwaysShowOnSchedule && !truck.isExternal));

    const getTruckColor = (truck) => {
        const defaultColor = {
            backgroundColor: '#f8f9fa', // grey
            color: '#fff',
            border: '1px solid transparent'
        };

        if (truck.isOutOfService) {
            return {
                ...defaultColor,
                color: '#999'
            };
        }

        if (truckHasNoDriver(truck) && 
            (leaseHaulers.isEnabled || truckCategoryNeedsDriver(truck))) {
            return {
                ...defaultColor,
                backgroundColor: '#0288d1' // blue
            };
        }

        if (validateUtilization.isEnabled) {
            if (truck.utilization >= 1) {
                return {
                    ...defaultColor,
                    backgroundColor: '#dd4b39' // red
                };
            }
            if (truck.utilization > 0) {
                return {
                    ...defaultColor,
                    backgroundColor: '#ed6c02' // orange
                }
            }
            return {
                ...defaultColor,
                backgroundColor: '#00a65a' // green
            };
        } else {
            if (truck.utilization > 1) {
                return {
                    ...defaultColor,
                    backgroundColor: '#dd4b39' // red
                };
            }
            if (truck.utilization === 1) {
                return {
                    ...defaultColor,
                    backgroundColor: '#ed6c02' // orange
                }
            }
            if (truck.utilization > 0) {
                return {
                    ...defaultColor,
                    backgroundColor: '#00a65a' // green
                };
            }

            // empty
            return {
                backgroundColor: '#fff',
                color: '#999',
                border: '1px solid #999'
            };
        }
    };

    const renderTrucks = () => (
        <>
            { trucks.map((truck) => {
                const truckColors = getTruckColor(truck);
                return (
                    <Grid item key={truck.truckCode}>
                        <Chip
                            label={truck.truckCode}
                            onClick={() => {}}
                            sx={{
                                borderRadius: 0,
                                fontSize: 18,
                                fontWeight: 600,
                                py: 3,
                                backgroundColor: `${truckColors.backgroundColor}`,
                                color: `${truckColors.color}`,
                                border: `${truckColors.border}`
                            }}
                        />
                    </Grid>
                );
            })}
        </>
    );

    return (
        <React.Fragment>
            {/* Truck Map */}
            <Box sx={{ p: 3 }}>
                <Paper variant='outlined' sx={{ p: 1 }}>
                    <Grid container rowSpacing={1} columnSpacing={1}>
                        {!isEmpty(trucks) && renderTrucks()}

                        <Grid item>
                            <Chip
                                label={
                                    <>
                                        <AddIcon sx={{ mt: '5px' }} />
                                    </>
                                }
                                onClick={() => {}}
                                sx={{
                                    backgroundColor: '#fff',
                                    border: '1px solid #ebedf2',
                                    color: '#6f727d',
                                    borderRadius: 0,
                                    fontSize: 18,
                                    fontWeight: 600,
                                    py: 3,
                                    px: 1
                                }}
                            />
                        </Grid>

                        {/* {TruckCode.map((truck) => {
                            return (
                                <Grid item key={truck.label}>
                                    <Chip
                                        label={truck.label}
                                        color={truck.color}
                                        onClick={() => {}}
                                        sx={{
                                            borderRadius: 0,
                                            fontSize: 18,
                                            fontWeight: 600,
                                            py: 3,
                                        }}
                                    />
                                </Grid>
                            );
                        })} */}
                    </Grid>
                </Paper>
            </Box>
        </React.Fragment>
    );
}

export default TruckMap;