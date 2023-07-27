import React, { useEffect, useState, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Paper,
    Chip,
    Grid, 
    Skeleton,
    Tooltip 
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import _, { isEmpty } from 'lodash';
import { baseUrl } from '../../helpers/api_helper';
import * as signalR from '@microsoft/signalr';
import { getScheduleTrucks } from '../../store/actions';
import AddOrEditTruckForm from '../../components/trucks/addOrEditTruck';

const TruckMap = ({
    pageConfig,
    dataFilter,
    trucks,
    onSetTrucks,
    openModal,
    closeModal,
    openDialog
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [validateUtilization, setValidateUtilization] = useState(null);
    const [leaseHaulers, setLeaseHaulers] = useState(null);
    const [isConnectedToSignalR, setIsConnectedToSignalR] = useState(false);

    const dispatch = useDispatch();
    const { 
        scheduleTrucks,
        editTruckSuccess
    } = useSelector((state) => ({
        scheduleTrucks: state.SchedulingReducer.scheduleTrucks,
        editTruckSuccess: state.TruckReducer.editTruckSuccess
    }));

    useEffect(() => {
        if (!isEmpty(pageConfig)) {
            if (validateUtilization === null) {
                setValidateUtilization(pageConfig.settings.validateUtilization);
            }
            
            if (leaseHaulers === null) {
                setLeaseHaulers(pageConfig.features.leaseHaulers);
            }
        }
    }, [pageConfig, validateUtilization, leaseHaulers]);
    
    useEffect(() => {
        if (!isEmpty(pageConfig) && isLoading && 
            !isEmpty(scheduleTrucks) && !isEmpty(scheduleTrucks.result)
        ) {
            const { items } = scheduleTrucks.result;
            if (!isEmpty(items) && (
                isEmpty(trucks) || (!isEmpty(trucks) && !_.isEqual(trucks, items))
            )) {
                onSetTrucks(items);
                setLoading(false);
            }
        }
    }, [pageConfig, isLoading, scheduleTrucks, trucks, onSetTrucks]);

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
        if (!isConnectedToSignalR && dataFilter.officeId !== null && dataFilter.date !== null) {
            const startConnection = (transport) => {
                const url = `${baseUrl}/signalr-dispatcher`;
                const connection = new signalR.HubConnectionBuilder()
                    .withUrl(url, transport)
                    .withAutomaticReconnect()
                    .build();

                connection.onclose((err) => {
                    if (err) {
                        console.log('Connection closed with error: ', err);
                    } else {
                        console.log('Disconnected');
                    }

                    setTimeout(() => {
                        connection.start();
                    }, 5000);
                });

                connection.on('syncRequest', payload => {
                    console.log('payload: ', payload)
                    fetchData();
                });

                connection
                    .start()
                    .then(() => {
                        setIsConnectedToSignalR(true);
                    })
                    .catch((err) => {
                        console.log(err);
                        if (transport !== signalR.HttpTransportType.LongPolling) {
                            return startConnection(transport + 1);
                        }
                    });
            };
            
            startConnection(signalR.HttpTransportType.WebSockets);
        }
    }, [dispatch, isConnectedToSignalR, dataFilter]);

    useEffect(() => {
        if (editTruckSuccess) {
            fetchData();
        }
    }, [editTruckSuccess]);

    const fetchData = () => {
        const { officeId, date } = dataFilter;
        if (officeId !== null && date !== null) {
            setLoading(true);
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

        if (truck.isOutOfService) {
            return {
                ...defaultColor,
                color: '#999'
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

    const getTruckTileTitle = (truck) => {
        let title = truck.truckCode;
        if (truckCategoryNeedsDriver(truck)) {
            title += ' - ' + truck.driverName;
        }
        return title;
    };

    const handleCreateNewTruck = (e) => {
        e.preventDefault();

        openModal(
            <AddOrEditTruckForm 
                pageConfig={pageConfig} 
                openModal={openModal}
                closeModal={closeModal} 
                openDialog={openDialog}
            />,
            560
        );
    };

    const renderTrucks = () => (
        <>
            { trucks.map((truck) => {
                const truckColors = getTruckColor(truck);
                const truckTitle = getTruckTileTitle(truck);

                return (
                    <Grid item key={truck.truckCode}>
                        <Tooltip title={truckTitle}>
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
                                    border: `${truckColors.border}`,
                                    '&:hover': {
                                        backgroundColor: `${truckColors.backgroundColor}`
                                    }
                                }}
                            />
                        </Tooltip>
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
                    <Grid id='TruckTiles' container rowSpacing={1} columnSpacing={1}>
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

                        {!isLoading && !isEmpty(trucks) && renderTrucks()}

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

export default TruckMap;