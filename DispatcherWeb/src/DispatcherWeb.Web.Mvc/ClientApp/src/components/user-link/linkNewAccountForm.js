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
import {
    getLinkedUsers,
    linkToUser as onLinkToUser 
} from '../../store/actions';

const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
};

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

    const dispatch = useDispatch();
    const { 
        userProfileMenu,
        linkSuccess
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu,
        linkSuccess: state.UserLinkReducer.linkSuccess
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

    useEffect(() => {
        if (linkSuccess) {
            dispatch(getLinkedUsers());
            closeModal();
        }
    }, [dispatch, linkSuccess, closeModal]);

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
        const invalidFields = Object.keys(fields).filter((field) => {
            if (fields[field].required && !fields[field].value) {
                return true; // Field is required but has no value
            }
        
            if (fields[field].type === 'email' && !isValidEmail(fields[field].value)) {
                fields[field].errorText = 'Please enter a valid username';
                return true; // Field is of type "email" but has an invalid email format
            }
        
            return false;
        });

        if (invalidFields.length > 0) {
            setInvalidFields(invalidFields);
            return;
        }

        // Perform custom actions based on the form data
        const convertedData = {};
        for (const key in fields) {
            convertedData[key] = fields[key].value;
        }
        dispatch(onLinkToUser(convertedData));

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