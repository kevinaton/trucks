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
import { useSnackbar } from 'notistack';
import { 
    setTruckIsOutOfService as onSetTruckIsOutOfService,
    resetSetTruckIsOutOfService as onResetTruckIsOutOfService
} from '../../store/actions';
import { getText } from '../../helpers/localization_helper';
import { AlertDialog } from '../../components/common/dialogs';
import { isEmpty } from 'lodash';

const AddOutOfServiceReason = ({
    data,
    closeModal,
    openDialog
}) => {
    const [reason, setReason] = useState('');
    const [error, setError] = useState(false);
    const [errorText, setErrorText] = useState('');

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const {
        setTruckIsOutOfServiceSuccess,
        setOutOfServiceResponse
    } = useSelector(state => ({
        setTruckIsOutOfServiceSuccess: state.TruckReducer.setTruckIsOutOfServiceSuccess,
        setOutOfServiceResponse: state.TruckReducer.setOutOfServiceResponse
    }));

    useEffect(() => {
        if (setTruckIsOutOfServiceSuccess) {
            closeModal();
            enqueueSnackbar('Saved successfully', { variant: 'success' });

            if (!isEmpty(setOutOfServiceResponse)) {
                const { result } = setOutOfServiceResponse;

                const infoMessages = [];
                if (result.thereWereAssociatedOrders) {
                    infoMessages.push(getText('ThereWereOrdersAssociatedWithThisTruck'));
                }
                if (result.thereWereCanceledDispatches) {
                    infoMessages.push(getText('ThereWereCanceledDispatches'));
                }
                if (result.thereWereNotCanceledDispatches) {
                    infoMessages.push(getText('ThereWereNotCanceledDispatches'));
                }
                if (result.thereWereAssociatedTractors) {
                    infoMessages.push(getText('ThereWasTractorAssociatedWithThisTrailer'));
                }
                
                if (infoMessages.length > 0) {
                    const mappedMsg = infoMessages.map((message, index) => (
                        <Typography key={index} display='block'>
                            {message}
                        </Typography>
                    ));
                    openDialog({
                        type: 'alert', 
                        content: (
                            <AlertDialog 
                                variant='info' 
                                title='Message'
                                message={mappedMsg} 
                            />
                        ),
                    });
                }
            }

            dispatch(onResetTruckIsOutOfService());
        }
    }, [dispatch, enqueueSnackbar, openDialog, setTruckIsOutOfServiceSuccess, setOutOfServiceResponse, closeModal]);

    const handleReasonInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 500) {
            setReason(inputValue);
            setError(false);
            setErrorText('');
        }
    };

    const handleCancel = () => {
        // Reset the form
        setReason('');
        setError(false);
        setErrorText('');
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
        
        if (!reason) {
            setError(true);
            setErrorText('Please enter a reason.');
            return;
        }

        const payload = {
            date: data.scheduleDate,
            isOutOfService: true,
            reason,
            truckId: data.truckId
        };
        dispatch(onSetTruckIsOutOfService(payload));
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
            
            <Box sx={{ 
                p: 2, 
                width: '100%' 
            }}>
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