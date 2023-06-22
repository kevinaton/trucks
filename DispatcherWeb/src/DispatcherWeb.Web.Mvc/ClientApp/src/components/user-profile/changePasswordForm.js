import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Button,
    Link,
    Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { DynamicForm } from '../common/forms';
import { baseUrl } from '../../helpers/api_helper';
import { isEmpty } from 'lodash';
import { useSnackbar } from 'notistack';
import { 
    changePassword as onChangePassword, 
    resetChangePasswordState as onResetChangePasswordState
} from '../../store/actions';
import { AlertDialog } from '../common/dialogs';

const ChangePasswordForm = ({
    closeModal,
    openDialog
}) => {
    const [fields, setFields] = useState({
        currentPassword: {
            label: 'Current Password',
            type: 'password',
            value: '',
            required: true,
            errorText: 'Please enter current password',
            helpText: (
                <Typography variant='caption' sx={{ display: 'block', marginBottom: '15px' }}>
                    If you can not remember your password, <Link href={`${baseUrl}/account/forgotpassword`} target='_blank'>click here</Link>.
                </Typography>
            )
        }, 
        newPassword: {
            label: 'New Password',
            type: 'password',
            value: '',
            required: true,
            errorText: 'Please enter new password'
        }, 
        newPasswordRepeat: {
            label: 'New Password (repeat)',
            type: 'password',
            value: '',
            required: true,
            errorText: 'Please repeat new password'
        }
    });
    const [invalidFields, setInvalidFields] = useState([]);

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const { 
        updateSuccess,
        error
    } = useSelector((state) => ({
        updateSuccess: state.UserProfileReducer.updateSuccess,
        error: state.UserProfileReducer.error
    }));

    useEffect(() => {
        if (updateSuccess) {
            closeModal();
            enqueueSnackbar('Your password has been successfully changed', { variant: 'success' });
            dispatch(onResetChangePasswordState());
        }
    }, [dispatch, updateSuccess, enqueueSnackbar, closeModal]);

    useEffect(() => {
        if (!isEmpty(error) && !isEmpty(error.response)) {
            const { data } = error.response;
            const { message } = data.error;

            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog message={message} />
                )
            });
        }
    }, [error, openDialog]);

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

            if (field === 'newPasswordRepeat' && fields.newPassword.value !== fields.newPasswordRepeat.value) {
                fields.newPasswordRepeat.errorText = 'Passwords do not match';
                return true; // newPassword does not match newPasswordRepeat
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
        dispatch(onChangePassword(convertedData));

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
                    Change password
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

export default ChangePasswordForm;