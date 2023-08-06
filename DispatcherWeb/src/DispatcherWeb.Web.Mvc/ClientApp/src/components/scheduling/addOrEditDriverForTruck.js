import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    Autocomplete,
    Box,
    Button,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import _, { isEmpty } from 'lodash';
import {
    getLeaseHaulerDriversSelectList,
    getDriversSelectList,
    setDriverForTruck as onSetDriverForTruck
} from '../../store/actions';
import { isPastDate } from '../../helpers/misc_helper';

const AddOrEditDriverForTruck = ({
    userAppConfiguration,
    data,
    closeModal
}) => {
    const [isLoading, setIsLoading] = useState(false);
    const [driverOptions, setDriverOptions] = useState(null);
    const [driverId, setDriverId] = useState('');
    const [defaultDriverId, setDefaultDriverId] = useState(null);
    const [leaseHaulerId, setLeaseHaulerId] = useState(null);
    const [allowSchedulingTrucksWithoutDrivers, setAllowSchedulingTrucksWithoutDrivers] = useState(null);
    const [allowSubcontractorsToDriveCompanyOwnedTrucks, setAllowSubcontractorsToDriveCompanyOwnedTrucks] = useState(null);
    const [error, setError] = useState(false);
    const [errorText, setErrorText] = useState('');

    const dispatch = useDispatch();
    const {
        leaseHaulerDriversSelectList,
        driversSelectList
    } = useSelector(state => ({
        leaseHaulerDriversSelectList: state.LeaseHaulerReducer.leaseHaulerDriversSelectList,
        driversSelectList: state.DriverReducer.driversSelectList
    }));

    useEffect(() => {
        if (!isEmpty(userAppConfiguration)) {
            if (allowSchedulingTrucksWithoutDrivers === null) {
                setAllowSchedulingTrucksWithoutDrivers(userAppConfiguration.settings.allowSchedulingTrucksWithoutDrivers);
            }

            if (allowSubcontractorsToDriveCompanyOwnedTrucks === null) {
                setAllowSubcontractorsToDriveCompanyOwnedTrucks(userAppConfiguration.settings.allowSubcontractorsToDriveCompanyOwnedTrucks);
            }
        }
    }, [])

    useEffect(() => {
        if (!isEmpty(data)) {
            setIsLoading(true);

            if (data.leaseHaulerId !== null) {
                setLeaseHaulerId(data.leaseHaulerId);
                dispatch(getLeaseHaulerDriversSelectList({
                    leaseHaulerId: data.leaseHaulerId,
                }));
            } else {
                if (allowSubcontractorsToDriveCompanyOwnedTrucks !== null) {
                    dispatch(getDriversSelectList({
                        officeId: allowSubcontractorsToDriveCompanyOwnedTrucks ? null : data.officeId,
                        includeLeaseHaulerDrivers: allowSubcontractorsToDriveCompanyOwnedTrucks,
                        maxResultCount: 1000,
                        skipCount: 0
                    }));
                }
            }
        }
    }, [dispatch, data, allowSubcontractorsToDriveCompanyOwnedTrucks]);

    useEffect(() => {
        console.log('leaseHaulerDriversSelectList: ', leaseHaulerDriversSelectList)
    }, [leaseHaulerDriversSelectList]);

    useEffect(() => {
        if (!isEmpty(driversSelectList) && !isEmpty(driversSelectList.result)) {
            const { result } = driversSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setDriverOptions(result.items);
            }
        }
    }, [driversSelectList]);

    useEffect(() => {
        if (!isEmpty(driverOptions)) {
            if (data.driverId !== null && data.driverId !== undefined && driverId === '') {
                setDefaultDriverId(_.findIndex(driverOptions, { id: data.driverId.toString() }));
                setDriverId(data.driverId);
            }

            setIsLoading(false);
        }
    }, [driverOptions, data, driverId]);

    const handleDriverChange = (e, newValue) => {
        e.preventDefault();
        setDriverId(newValue);
    };

    const handleCancel = () => {
        // Reset the form
        setDriverId('');
        setError(false);
        setErrorText('');
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
        
        if (!driverId) {
            setError(true);
            setErrorText('Please select a driver.');
            return;
        }
        
        if (leaseHaulerId) {
            console.log('leaseHaulerId: ', leaseHaulerId)
        } else {
            dispatch(onSetDriverForTruck({
                date: data.date,
                driverId,
                leaseHaulerId: "",
                officeId: data.officeId,
                shift: data.shift,
                truckCode: data.truckCode,
                truckId: data.truckId
            }));
        }

        closeModal();
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ p: 2 }} 
            >
                <Typography variant='h6' component='h2'>Assign driver</Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>
            
            <Box sx={{ 
                p: 2, 
                width: '100%' 
            }}>
                {!isLoading && driverOptions && 
                    <Autocomplete
                        id='driverId'
                        options={driverOptions} 
                        getOptionLabel={(option) => option.name} 
                        defaultValue={driverOptions[defaultDriverId]}
                        sx={{ 
                            flex: 1, 
                            flexShrink: 0,
                            "& .Mui-error": {
                                borderColor: 'red',
                            },
                        }}
                        renderInput={(params) => 
                            <TextField 
                                {...params} 
                                label={
                                    <>
                                        Driver {!allowSchedulingTrucksWithoutDrivers && <span style={{ marginLeft: '5px', color: 'red' }}>*</span> } 
                                    </>
                                } 
                                error={error} 
                                helperText={error ? errorText : ''} 
                            />
                        } 
                        onChange={(e, value) => handleDriverChange(e, value.id, value.name)} 
                        fullWidth
                    />
                }
            </Box>

            <Box sx={{ p: 2 }}>
                <Stack 
                    spacing={2}
                    direction='row' 
                    justifyContent='flex-end'
                >
                    <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
                    <Button variant='contained' color='primary' onClick={(e) => handleSave(e)}>
                        <i className='fa-regular fa-save' style={{ marginRight: '6px' }}></i>
                        <Typography>Save</Typography>
                    </Button>
                </Stack>
            </Box>
        </React.Fragment>
    );
};

export default AddOrEditDriverForTruck;