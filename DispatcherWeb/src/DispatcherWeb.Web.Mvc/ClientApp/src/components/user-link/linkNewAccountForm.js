import React, { useState } from 'react';
import {
    Box,
    Button,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { DynamicForm } from '../common/forms';

const LinkNewAccountForm = ({
    closeModal
}) => {
    const [tenancyName, setTenancyName] = useState('');
    const [usernameOrEmailAddress, setUsernameOrEmailAddress] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleTenancyNameChange = (newValue) => {
        setTenancyName(newValue);
    };

    const handleUsernameOrEmailAddressChange = (newValue) => {
        setUsernameOrEmailAddress(newValue);
    };

    const handlePasswordChange = (newValue) => {
        setPassword(newValue);
    };

    const handleCancel = () => {
        // Reset the form
        setTenancyName('');
        setUsernameOrEmailAddress('');
        setPassword('');
    };

    const handleSave = () => {
        // Perform form validation
        if (!tenancyName) {
            setError('Tenancy name is required');
        }

        if (!usernameOrEmailAddress) {
            setError('Username is required');
        }

        if (!password) {
            setError('Password is required');
        }

        // Perform custom actions based on the form data
        console.log('Form data: ', {tenancyName, usernameOrEmailAddress, password});

        // Reset the form
        setTenancyName('');
        setUsernameOrEmailAddress('');
        setPassword('');
    };

    const fields = [
        {
            name: 'tenancyName',
            label: 'Tenancy Name',
            value: tenancyName,
            onChange: handleTenancyNameChange
        }, {
            name: 'usernameOrEmailAddress',
            label: 'Username',
            value: usernameOrEmailAddress,
            onChange: handleUsernameOrEmailAddressChange
        }, {
            name: 'password',
            label: 'Password',
            value: password,
            onChange: handlePasswordChange
        }
    ];
    
    return (
        <React.Fragment>
            <Box 
                sx={{ 
                    display: 'flex', 
                    p: 2 
                }} 
                justifyContent='space-between'
                alignItems='center'
            >
                <Typography variant='h6' component='h2'>
                    Link new account
                </Typography>
                <Button onClick={closeModal}>
                    <CloseIcon />
                </Button>
            </Box>

            <div>
                <DynamicForm
                    fields={fields}
                    onSave={handleSave}
                    onCancel={handleCancel}
                />
                {error && <p>{error}</p>}
            </div>
        </React.Fragment>
    );
};

export default LinkNewAccountForm;