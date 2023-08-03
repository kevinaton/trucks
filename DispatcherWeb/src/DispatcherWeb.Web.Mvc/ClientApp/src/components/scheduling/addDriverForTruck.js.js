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

const AddDriverForTruck = ({
    data,
    closeModal
}) => {
    const [driverOptions, setDriverOptions] = useState(null);
    const [truckId, setTruckId] = useState(null);
    const [truckCode, setTruckCode] = useState(null);
    const [leaseHaulerId, setLeaseHaulerId] = useState(null);
    const [date, setDate] = useState(null);
    const [shift, setShift] = useState(null);
    const [officeId, setOfficeId] = useState(null);
    const [driverId, setDriverId] = useState('');
    const [error, setError] = useState(false);
    const [errorText, setErrorText] = useState('');

    const dispatch = useDispatch();

    useEffect(() => {
        if (!isEmpty(data)) {
            if (data.leaseHaulerId !== null) {
                
            } else {

            }
            // setTruckId(data.truckId);
            // setTruckCode(data.truckCode);
            // setLeaseHaulerId(data.leaseHaulerId);
            // setDate(data.date);
            // setShift(data.shift);
            // setOfficeId(data.officeId);
        }
    }, []);

    const handleDriverChange = (e, newValue) => {
        e.preventDefault();
        
        setDriverId(newValue);
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
        
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
                <Autocomplete
                    id='driverId'
                    options={driverOptions} 
                    getOptionLabel={(option) => option.name} 
                    defaultValue={driverOptions[driverId]}
                    sx={{ flex: 1, flexShrink: 0 }}
                    renderInput={(params) => <TextField {...params} label='Driver' />} 
                    onChange={(e, value) => handleDriverChange(e, value.id, value.name)} 
                    error={error} 
                    helperText={error ? errorText : ''} 
                    fullWidth
                />
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