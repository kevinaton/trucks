import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Paper,
    Chip,
    Grid
} from '@mui/material';
import moment from 'moment';
import { isEmpty } from 'lodash';
import { getUserSettingByName, getScheduleTrucks } from '../../store/actions';
import data from '../../common/data/data.json';
const { TruckCode } = data;

const TruckMap = ({
    officeId
}) => {
    const [validateUtilization, setValidateUtilization] = useState(null);
    const [trucks, setTrucks] = useState([]);
    const [filter, setFilter] = useState({
        officeId: '2778',
        date: moment().format('MM/DD/YYYY')
    });
    const dispatch = useDispatch();

    const { userSettings, scheduleTrucks } = useSelector((state) => ({
        userSettings: state.UserReducer.userSettings,
        scheduleTrucks: state.SchedulingReducer.scheduleTrucks,
    }));

    useEffect(() => {
        dispatch(getUserSettingByName('App.DispatchingAndMessaging.ValidateUtilization'));
        dispatch(getScheduleTrucks(filter));
    }, [dispatch, filter]);
    
    useEffect(() => {
        if (!isEmpty(userSettings) && !isEmpty(userSettings.result) && isEmpty(validateUtilization)) {
            setValidateUtilization(Boolean(userSettings.result));
        }
        
        if (!isEmpty(scheduleTrucks) && !isEmpty(scheduleTrucks.result) && isEmpty(trucks)) {
            const { items } = scheduleTrucks.result;
            setTrucks(items);
        }
    }, [validateUtilization, userSettings, scheduleTrucks, trucks]);

    const truckHasNoDriver = truck => 
        !truck.isExternal && 
        (truck.hasNoDriver || (!truck.hasDefaultDriver && !truck.hasDriverAssignment));

    // const truckCategoryNeedsDriver = truck => 
    //     truck.vehicleCategory.isPowered && ()

    const getTruckColor = (truck) => {
        if (truck.isOutOfService) {
            return 'error';
        }

        if (truck.isAvailable) {
            return 'success';
        }

        if (truck.isOnBreak) {
            return 'warning';
        }

        return 'primary';
    }

    const renderTrucks = () => {
        if (!isEmpty(trucks)) {
            const mappedTrucks = trucks.map((truck) => {
                return (
                    <Grid item key={truck.truckCode}>
                        <Chip
                            label={truck.truckCode}
                            color={getTruckColor(truck)}
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
            });
        }
    }

    return (
        <React.Fragment>
            {renderTrucks()}

            {/* Truck Map */}
            <Box sx={{ p: 3 }}>
                <Paper variant='outlined' sx={{ p: 1 }}>
                    <Grid container rowSpacing={1} columnSpacing={1}>
                        {TruckCode.map((truck) => {
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
                        })}
                    </Grid>
                </Paper>
            </Box>
        </React.Fragment>
    );
}

export default TruckMap;