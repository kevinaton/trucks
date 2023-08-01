import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    Box,
    Button,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import moment from 'moment';

const AddOutOfServiceReason = ({
    data,
    closeModal
}) => {
    const [reason, setReason] = useState('');
    const [error, setError] = useState(false);
    const [errorText, setErrorText] = useState('');

    const dispatch = useDispatch();

    const handleReasonInputChange = () => {

    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
        const payload = {
            date: moment().format('MM/DD/YYYY'),
            isOutOfService: true,
            reason,
            truckId: data
        };
        console.log('payload: ', payload)
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ p: 2 }} 
            >
                <Typography variant='h6' component='h2'>Take out of service</Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>
            
            <Box sx={{ width: '100%' }}>
                <TextField
                    id="reason"
                    label={
                        <>
                            Reason <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                        </>
                    }
                    value={reason} 
                    defaultValue=''
                    onChange={handleReasonInputChange} 
                    multiline
                    rows={2} 
                    error={error} 
                    helperText={error ? errorText : ''} 
                    fullWidth 
                    maxLength={500}
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

export default AddOutOfServiceReason;