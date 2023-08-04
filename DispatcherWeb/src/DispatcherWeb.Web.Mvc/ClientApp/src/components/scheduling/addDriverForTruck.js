import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    Autocomplete,
    Box,
    Button,
    FormControl,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { isEmpty } from 'lodash';
import {
    getLeaseHaulerDriversSelectList,
    getDriversSelectList,
} from '../../store/actions';

const AddDriverForTruck = ({
    userAppConfiguration,
    data,
    closeModal
}) => {
    const [driverOptions, setDriverOptions] = useState(null);
    const [driverId, setDriverId] = useState('');
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
            if (data.leaseHaulerId !== null) {
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
            // setTruckId(data.truckId);
            // setTruckCode(data.truckCode);
            // setLeaseHaulerId(data.leaseHaulerId);
            // setDate(data.date);
            // setShift(data.shift);
            // setOfficeId(data.officeId);
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

        console.log('validating...')
        console.log('driverId: ', driverId)
        console.log('!driverId: ', !driverId)
        
        if (!driverId) {
            setError(true);
            setErrorText('Please select a driver.');
            return;
        }

        // const payload = {
        //     date: data.scheduleDate,
        //     isOutOfService: true,
        //     reason,
        //     truckId: data.truckId
        // };
        //dispatch(onSetTruckIsOutOfService(payload));
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
                {driverOptions && 
                    <Autocomplete
                        id='driverId'
                        options={driverOptions} 
                        getOptionLabel={(option) => option.name} 
                        defaultValue={driverOptions[driverId]}
                        sx={{ 
                            flex: 1, 
                            flexShrink: 0,
                            "& .Mui-error": {
                              // Define your error styles here, e.g., color, border, etc.
                              borderColor: 'red',
                            },
                        }}
                        renderInput={(params) => 
                            <TextField {...params} label={
                                    <>
                                        Driver {!allowSchedulingTrucksWithoutDrivers && <span style={{ marginLeft: '5px', color: 'red' }}>*</span> } 
                                    </>
                                } 
                            />
                        } 
                        onChange={(e, value) => handleDriverChange(e, value.id, value.name)} 
                        error={error} 
                        helperText={error ? errorText : ''} 
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

export default AddDriverForTruck;