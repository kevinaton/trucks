import React, { useState } from 'react';
import { isEmpty } from 'lodash';
import SnackbarContext from './snackbarContext';
import SnackbarComponent from './index';

const SnackbarProvider = ({ children }) => {
    const [open, setOpen] = useState(false);
    const [message, setMessage] = useState('');
    const [severity, setSeverity] = useState('success'); // ['success', 'info', 'warning', 'error']

    const showSnackbar = (msg, severityType) => {
        if (!isEmpty(severityType)) {
            setSeverity(severityType)
        }

        setMessage(msg);
        setOpen(true);
    };

    const hideSnackbar = () => {
        setOpen(false);
    };

    return (
        <SnackbarContext.Provider value={{ showSnackbar, hideSnackbar }}>
            {children}
            { open && 
                <SnackbarComponent 
                    open={open} 
                    message={message} 
                    severity={severity}
                />
            }
        </SnackbarContext.Provider>
    );
};

export default SnackbarProvider;