import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    Checkbox,
    Divider,
    FormControlLabel,
    FormGroup,
    IconButton,
    Modal,
    Switch,
    Typography
} from '@mui/material';
import { isEmpty } from 'lodash';
import { theme } from '../../../Theme'
import { getUserNotificationSettings } from '../../../store/actions';

export const NotificationSettingsModal = ({
    open,
    onClose,
    labelledBy,
    title
}) => {
    const dispatch = useDispatch();
    const { notificationSettings } = useSelector(state => ({
        notificationSettings: state.NotificationReducer.notificationSettings
    }));

    useEffect(() => {
        if (open) {
            if (isEmpty(notificationSettings)) {
                dispatch(getUserNotificationSettings());
            } else {
                const { result } = notificationSettings;
                console.log('result: ', result)
            }
        }
    }, [dispatch, open, notificationSettings]);

    return (
        <Modal
            open={open}
            onClose={onClose}
            aria-labelledby={labelledBy}
        >
            <Card
                sx={{
                    minWidth: 500,
                    position: 'absolute',
                    top: '30%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}
            >
                <CardHeader
                    action={
                        <IconButton
                            aria-label='close'
                            onClick={onClose}
                        >
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title={title} 
                />

                <CardContent>
                    <FormGroup>
                        <FormControlLabel
                            control={<Switch defaultChecked />}
                            label='Receive notifications' 
                        />

                        <Typography
                            color={theme.palette.text.secondary}
                            variant='caption'
                        >
                            This option can be used to completely enable/disable
                            receiving notifications.
                        </Typography>

                        <Divider sx={{ my: 3 }} />

                        <FormControlLabel
                            control={<Checkbox defaultChecked />}
                            label='On a new user registered with the application.' 
                        />
                    </FormGroup>
                </CardContent>

                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button onClick={onClose}>
                        Cancel
                    </Button>

                    <Button
                        variant='contained'
                        onClick={onClose}
                        startIcon={<i className='fa-regular fa-save'></i>}
                    >
                        Save
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};