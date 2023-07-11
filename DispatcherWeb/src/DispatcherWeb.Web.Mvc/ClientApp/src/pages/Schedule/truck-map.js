import React, { useEffect, useState, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Paper,
    Chip,
    Grid,
    Tooltip 
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import _, { isEmpty } from 'lodash';
import { getScheduleTrucks } from '../../store/actions';
import AddTruckForm from '../../components/trucks/addTruck';

const TruckMap = ({
    pageConfig,
    dataFilter,
    trucks,
    onSetTrucks,
    openModal,
    closeModal
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [validateUtilization, setValidateUtilization] = useState(null);
    const [leaseHaulers, setLeaseHaulers] = useState(null);

    const dispatch = useDispatch();
    const { 
        scheduleTrucks 
    } = useSelector((state) => ({
        scheduleTrucks: state.SchedulingReducer.scheduleTrucks,
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
            (leaseHaulers || truckCategoryNeedsDriver(truck))) {
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
            <AddTruckForm 
                pageConfig={pageConfig} 
                closeModal={closeModal} 
            />,
            500
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
                                    border: `${truckColors.border}`
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
                        {!isEmpty(trucks) && renderTrucks()}

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
                    </Grid>
                </Paper>
            </Box>
        </React.Fragment>
    );
}

export default TruckMap;