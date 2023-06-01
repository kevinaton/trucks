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
import { isEmpty, set } from 'lodash';
import { theme } from '../../../Theme'
import { getUserNotificationSettings } from '../../../store/actions';

export const NotificationSettingsModal = ({
    open,
    onClose,
    labelledBy,
    title
}) => {
    const [userNotificationSettings, setUserNotificationSettings] = useState({
        receiveNotifications: false,
        notifications: []
    });

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
                if (!isEmpty(result)) {
                    setUserNotificationSettings(result);
                }

                console.log('result: ', result)
            }
        }
    }, [dispatch, open, notificationSettings]);

    const handleToReceiveNotificationsChange = event => {
        setUserNotificationSettings({
            ...userNotificationSettings,
            receiveNotifications: event.target.checked
        });
    };

    const handleNotifChange = (event, notif) => {
        const updatedNotifications = userNotificationSettings.notifications.map((item) => {
            if (notif.name===item.name) {
                return {
                    ...item,
                    isSubscribed: event.target.checked
                }
            }
            return item
        });
        setUserNotificationSettings({
            ...userNotificationSettings,
            notifications: updatedNotifications
        });
    };

    const handleSave = () => {
        console.log('userNotificationSettings: ', userNotificationSettings);
    }

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
                            control={
                                <Switch 
                                    checked={userNotificationSettings.receiveNotifications} 
                                    onChange={handleToReceiveNotificationsChange}
                                />
                            }
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

                        <Typography
                            color={theme.palette.warning.main} 
                            variant='caption'
                        >
                            You completely disabled receiving notifications. You can enable it and select notification types you want to receive.
                        </Typography>

                        { userNotificationSettings.notifications.map((notif, index) => {
                            return (
                                <FormControlLabel
                                    control={
                                        <Checkbox 
                                            checked={notif.isSubscribed} 
                                            onChange={(e) => handleNotifChange(e, notif)}
                                        />
                                    }
                                    label={notif.displayName} 
                                />
                            )
                        })}
                    </FormGroup>
                </CardContent>

                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button onClick={onClose}>
                        Cancel
                    </Button>

                    <Button
                        variant='contained'
                        onClick={handleSave}
                        startIcon={<i className='fa-regular fa-save'></i>}
                    >
                        Save
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};