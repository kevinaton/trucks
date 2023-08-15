import React, { useContext, useEffect, useState, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Chip,
    Grid, 
    Paper,
    Skeleton,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import _, { isEmpty } from 'lodash';
import { 
    getScheduleTrucks, 
    getScheduleTruckBySyncRequest, 
    removeTruckFromSchedule as onRemoveTruckFromSchedule,
    getScheduleTruckBySyncRequestReset as onResetGetScheduleTruckBySyncRequest } from '../../store/actions';
import TruckBlockItem from './truck-block-item';
import AddOrEditTruckForm from '../../components/trucks/addOrEditTruck';
import { assetType } from '../../common/enums/assetType';
import { entityType } from '../../common/enums/entityType';
import { changeType } from '../../common/enums/changeType';
import SyncRequestContext from '../../components/common/signalr/syncRequestContext';

const TruckBlock = ({
    userAppConfiguration,
    dataFilter,
    trucks, 
    orders,
    onSetTrucks,
    openModal,
    closeModal,
    openDialog, 
    setIsUIBusy
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [validateUtilization, setValidateUtilization] = useState(null);
    const [leaseHaulers, setLeaseHaulers] = useState(null);
    const [isConnectedToSignalR, setIsConnectedToSignalR] = useState(false);

    const syncRequestConnection = useContext(SyncRequestContext);
    const dispatch = useDispatch();
    const { 
        isLoadingScheduleTrucks,
        scheduleTrucks,
        isModifiedScheduleTrucks
    } = useSelector((state) => ({
        isLoadingScheduleTrucks: state.SchedulingReducer.isLoadingScheduleTrucks,
        scheduleTrucks: state.SchedulingReducer.scheduleTrucks,
        isModifiedScheduleTrucks: state.SchedulingReducer.isModifiedScheduleTrucks
    }));

    useEffect(() => {
        if (!isEmpty(userAppConfiguration)) {
            if (validateUtilization === null) {
                setValidateUtilization(userAppConfiguration.settings.validateUtilization);
            }
            
            if (leaseHaulers === null) {
                setLeaseHaulers(userAppConfiguration.features.leaseHaulers);
            }
        }
    }, [userAppConfiguration, validateUtilization, leaseHaulers]);
    
    useEffect(() => {
        if (isLoading && 
            !isEmpty(userAppConfiguration) && 
            !isEmpty(scheduleTrucks) && 
            !isEmpty(scheduleTrucks.result)
        ) {
            const { items } = scheduleTrucks.result;
            if (!isEmpty(items) && (
                isEmpty(trucks) || (!isEmpty(trucks) && !_.isEqual(trucks, items))
            )) {
                onSetTrucks(items);
            }
        }
    }, [isLoading, scheduleTrucks]);

    useEffect(() => {
        // check if dataFilter has changed from its previous state
        if (
            prevDataFilterRef.current.officeId !== dataFilter.officeId ||
            prevDataFilterRef.current.date !== dataFilter.date
        ) {
            fetchData();

            // update the previous dataFilter value
            prevDataFilterRef.current = dataFilter;
        }
    }, [dataFilter]);

    useEffect(() => {
        // cleanup logic
        return () => {
            // reset the previous dataFilter value
            prevDataFilterRef.current = null;
        };
    }, []);

    useEffect(() => {
        if (!isConnectedToSignalR && 
            syncRequestConnection !== null &&
            dataFilter.officeId !== null && 
            dataFilter.date !== null
        ) {
            syncRequestConnection.on('syncRequest', payload => {
                const { changes } = payload;
                if (!isEmpty(changes)) {
                    let changedTrucks = _.filter(changes, i => i.entityType === entityType.TRUCK);
                    const modifiedTrucks = _.map(
                        _.filter(changedTrucks, change => change.changeType === changeType.MODIFIED), 
                        item => item.entity.id
                    );

                    const removedTrucks = _.map(
                        _.filter(changedTrucks, change => change.changeType === changeType.REMOVED),
                        item => item.entity.id
                    );

                    if (modifiedTrucks.length > 0) {
                        dispatch(getScheduleTruckBySyncRequest({
                            officeId: dataFilter.officeId,
                            date: dataFilter.date,
                            truckIds: modifiedTrucks
                        }));
                    }

                    if (removedTrucks.length > 0) {
                        dispatch(onRemoveTruckFromSchedule(removedTrucks));
                    }
                }
            });

            setIsConnectedToSignalR(true);
        }
    }, [dispatch, isConnectedToSignalR, syncRequestConnection, dataFilter]);

    useEffect(() => {
        if (isModifiedScheduleTrucks) {
            const { items } = scheduleTrucks.result;
            if (!isEmpty(items)) {
                onSetTrucks(items);
                dispatch(onResetGetScheduleTruckBySyncRequest());
            }
        }
    }, [isModifiedScheduleTrucks]);

    useEffect(() => {
        if (isLoading !== isLoadingScheduleTrucks) {
            setLoading(isLoadingScheduleTrucks);
        }
    }, [isLoading, isLoadingScheduleTrucks]);

    const fetchData = () => {
        const { officeId, date } = dataFilter;
        if (officeId !== null && date !== null) {
            dispatch(getScheduleTrucks({
                officeId,
                date: date
            }));
        }
    };

    const truckHasNoDriver = truck => !truck.isExternal && 
        (truck.hasNoDriver || (!truck.hasDefaultDriver && !truck.hasDriverAssignment));

    const truckCategoryNeedsDriver = truck => 
        truck.vehicleCategory.isPowered && 
        (leaseHaulers || (!truck.alwaysShowOnSchedule && !truck.isExternal));

    const getTruckColor = (truck) => {
        const defaultColor = {
            backgroundColor: '#f8f9fa', // gray
            color: '#fff',
            border: '1px solid transparent'
        };

        if (truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor) {
            const tractor = _.find(trucks, t => t.id === truck.tractor.id);
            if (tractor) {
                return getTruckColor(tractor);
            }
        }

        if (truck.isOutOfService) {
            return {
                ...defaultColor,
                backgroundColor: '#999999',
                color: '#fff'
            };
        }

        if (truckHasNoDriver(truck) && truckCategoryNeedsDriver(truck)) {
            return {
                ...defaultColor,
                backgroundColor: '#0288d1' // blue
            };
        }

        if (validateUtilization) {
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

    const handleCreateNewTruck = (e) => {
        e.preventDefault();

        openModal(
            <AddOrEditTruckForm 
                userAppConfiguration={userAppConfiguration} 
                openModal={openModal}
                closeModal={closeModal} 
                openDialog={openDialog}
            />,
            560
        );
    };

    const renderTrucks = (index, truck) => (
        <Grid item key={index}>
            <TruckBlockItem 
                truck={truck} 
                truckColors={getTruckColor(truck)}
                userAppConfiguration={userAppConfiguration} 
                dataFilter={dataFilter} 
                truckHasNoDriver={truckHasNoDriver(truck)} 
                truckCategoryNeedsDriver={truckCategoryNeedsDriver(truck)} 
                orders={orders}
                openModal={openModal} 
                closeModal={closeModal} 
                openDialog={openDialog} 
                setIsUIBusy={setIsUIBusy}
            />
        </Grid>
    );

    return (
        <React.Fragment>
            {/* Truck Map */}
            <Box sx={{ p: 3 }}>
                <Paper variant='outlined' sx={{ p: 1 }}>
                    <Grid 
                        id='TruckTiles' 
                        container 
                        rowSpacing={1} 
                        columnSpacing={1}
                    >
                        { isLoading && 
                            <React.Fragment>
                                <Grid item>
                                    <Skeleton variant="rectangular" width={82} height={50} />
                                </Grid>
                                <Grid item>
                                    <Skeleton variant="rectangular" width={82} height={50} />
                                </Grid>
                                <Grid item>
                                    <Skeleton variant="rectangular" width={82} height={50} />
                                </Grid>
                                <Grid item>
                                    <Skeleton variant="rectangular" width={82} height={50} />
                                </Grid>
                                <Grid item>
                                    <Skeleton variant="rectangular" width={82} height={50} />
                                </Grid>
                            </React.Fragment>
                        }

                        { trucks && trucks.map((truck, index) => renderTrucks(index, truck))}

                        {!isLoading && 
                            <Grid item>
                                <Chip
                                    label={
                                        <>
                                            <AddIcon sx={{ mt: '5px' }} />
                                        </>
                                    }
                                    onClick={(e) => handleCreateNewTruck(e)}
                                    sx={{
                                        width: '80px',
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
                        }
                    </Grid>
                </Paper>
            </Box>
        </React.Fragment>
    );
}

export default TruckBlock;