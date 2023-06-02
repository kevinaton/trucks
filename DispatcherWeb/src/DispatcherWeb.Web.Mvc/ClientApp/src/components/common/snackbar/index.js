import React, { useContext } from 'react';
import SnackbarContext from './snackbarContext';
import { Alert, Snackbar } from '@mui/material';

const SnackbarComponent = ({
    open, 
    message,
    severity
}) => {
    const { hideSnackbar } = useContext(SnackbarContext);
    
    const handleClose = (event, reason) => {
        if (reason === 'clickaway') {
            return;
        }

        hideSnackbar();
    };

    return (
        <Snackbar 
            anchorOrigin={{ 
                vertical: 'bottom', 
                horizontal: 'right' 
            }}
            open={open} 
            autoHideDuration={3000} 
            onClose={handleClose}
        >
            <Alert 
                elevation={6} 
                variant='filled' 
                severity={severity}
            >
                {message}
            </Alert>
        </Snackbar>
    );
};

export default SnackbarComponent;