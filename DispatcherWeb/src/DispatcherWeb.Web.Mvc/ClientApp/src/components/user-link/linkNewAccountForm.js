import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Button, 
    Typography, 
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { DynamicForm } from '../common/forms';
import { isEmpty } from 'lodash';

const LinkNewAccountForm = ({
    closeModal
}) => {
    const [fields, setFields] = useState({
        tenancyName: {
            label: 'Tenancy Name',
            type: 'text',
            value: '',
            required: false,
            errorText: 'Please enter tenancy name'
        }, 
        usernameOrEmailAddress: {
            label: 'Username',
            type: 'email',
            value: '',
            required: true,
            errorText: 'Please enter username'
        }, 
        password: {
            label: 'Password',
            type: 'password',
            value: '',
            required: true,
            errorText: 'Please enter password'
        }
    });
    const [invalidFields, setInvalidFields] = useState([]);

    const { 
        userProfileMenu
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu
    }));
    
    useEffect(() => {
        if (!isEmpty(userProfileMenu) && !isEmpty(userProfileMenu.result)) {
            const { loginInformations } = userProfileMenu.result;
            if (!isEmpty(loginInformations)) {
                const { tenant } = loginInformations;
                if (!isEmpty(tenant)) {
                    const { tenancyName } = tenant;
                    if (!isEmpty(tenancyName)) {
                        setFields((prevFields) => ({
                            ...prevFields,
                            tenancyName: {
                                ...prevFields.tenancyName,
                                value: tenancyName,
                            },
                        }));
                    }
                }
            }
        }

    }, [userProfileMenu]);

    const handleInputChange = (field, value) => {
        setFields((prevFields) => ({
            ...prevFields,
            [field]: {
                ...prevFields[field],
                value: value
            },
        }));
  
        setInvalidFields((prevInvalidFields) => prevInvalidFields.filter((f) => f !== field));
    };

    const handleCancel = () => {
        // Reset the form
        setFields((prevFields) => {
            const resetFields = {};
            Object.keys(prevFields).forEach((field) => {
                resetFields[field] = {
                    ...prevFields[field],
                    value: '',
                };
            });
            return resetFields;
        });
        setInvalidFields([]);
        closeModal();
    };

    const handleSave = () => {
        // Perform form validation
        const invalidFields = Object.keys(fields).filter(
            (field) => fields[field].required && !fields[field].value
        );

        if (invalidFields.length > 0) {
            setInvalidFields(invalidFields);
            return;
        }

        // Perform custom actions based on the form data
        console.log('Form data: ', fields);

        // Reset form and field values
        setFields((prevFields) => {
            const resetFields = {};
            Object.keys(prevFields).forEach((field) => {
                resetFields[field] = {
                    ...prevFields[field],
                    value: '',
                };
            });
            return resetFields;
        });
        setInvalidFields([]);
    };
    
    return (
        <React.Fragment>
            <Box 
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ 
                    p: 2 
                }} 
            >
                <Typography variant='h6' component='h2'>
                    Link new account
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box 
                component='form' 
                noValidate
                autoComplete="off" 
                sx={{ p: 2 }}
            >
                <DynamicForm
                    fields={fields} 
                    invalidFields={invalidFields} 
                    onChange={(field, value) => handleInputChange(field, value)}
                    onSave={handleSave}
                    onCancel={handleCancel}
                />
            </Box>
        </React.Fragment>
    );
};

export default LinkNewAccountForm;