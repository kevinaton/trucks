import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import moment from 'moment';
import {
    Box,
    Button,
    Typography
} from '@mui/material';
// import { DateRangePicker } from '@mui/x-date-pickers-pro';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import CloseIcon from '@mui/icons-material/Close';
import { isEmpty } from 'lodash';

const ShareTruck = ({
    closeModal,
    userAppConfiguration
}) => {
    return (
        <React.Fragment>
            { !isEmpty(userAppConfiguration) && 
                <React.Fragment>
                    <Box
                        display='flex'
                        justifyContent='space-between'
                        alignItems='center'
                        sx={{ p: 2 }} 
                    >
                        <Typography variant='h6' component='h2'>Share Truck</Typography>
                        <Button 
                            onClick={closeModal} 
                            sx={{ minWidth: '32px' }}
                        >
                            <CloseIcon />
                        </Button>
                    </Box>

                    <Box sx={{ width: '100%' }}>
                        <LocalizationProvider 
                            dateAdapter={AdapterMoment} 
                            adapterLocale={moment.locale()}
                            fullWidth
                        >
                            {/* <DateRangePicker localeText={{ start: 'Beginning', end: 'End date' }} /> */}
                        </LocalizationProvider>
                    </Box>
                </React.Fragment>
            }
        </React.Fragment>
    );
};

export default ShareTruck;