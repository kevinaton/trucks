import React, { useEffect, useState, useContext } from 'react';
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
import { theme } from '../../../Theme';
import { 
    getUserNotificationSettings, 
    updateUserNotificationSettings as onUpdateUserNotificationSettings,
    updateUserNotificationSettingsReset as onResetSaveState
} from '../../../store/actions';
import SnackbarContext from '../snackbar/snackbarContext';

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
    const [hasNoSelectedTypes, setHasNoSelectedTypes] = useState(null);
    const [showNoSelectionError, setShowNoSelectionError] = useState(false);

    const dispatch = useDispatch();
    const { 
        notificationSettings,
        updateSuccess 
    } = useSelector(state => ({
        notificationSettings: state.NotificationReducer.notificationSettings,
        updateSuccess: state.NotificationReducer.updateSuccess
    }));

    const { showSnackbar } = useContext(SnackbarContext);

    useEffect(() => {
        if (open) {
            if (isEmpty(notificationSettings)) {
                dispatch(getUserNotificationSettings());
            } else {
                const { result } = notificationSettings;
                if (!isEmpty(result)) {
                    setUserNotificationSettings(result);
                }
            }
        }
    }, [dispatch, open, notificationSettings]);

    useEffect(() => {
        if (updateSuccess) {
            showSnackbar('Saved successfully', 'success');
            onClose();
            dispatch(onResetSaveState());
        }
    }, [dispatch, showSnackbar, updateSuccess, onClose]);

    useEffect(() => {
        if (!isEmpty(userNotificationSettings.notifications)) {
            const { notifications } = userNotificationSettings;
            if (!isEmpty(notifications)) {
                const hasNoSelection = notifications.every((notif) => {
                    return !notif.isSubscribed;
                });
                setHasNoSelectedTypes(hasNoSelection);
            }
        }
    }, [userNotificationSettings]);

    useEffect(() => {
        if (hasNoSelectedTypes && 
            !userNotificationSettings.receiveNotifications) {
            if (!showNoSelectionError) setShowNoSelectionError(true);
        } else {
            if (showNoSelectionError) setShowNoSelectionError(false);
        }
    }, [showNoSelectionError, hasNoSelectedTypes, userNotificationSettings]);

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
        dispatch(onUpdateUserNotificationSettings(userNotificationSettings));
    }

    return (
        <Modal
            open={!isEmpty(notificationSettings) && open}
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

                        { showNoSelectionError && 
                            <Typography
                                color={theme.palette.error.main} 
                                variant='caption'
                            >
                                You completely disabled receiving notifications. You can enable it and select notification types you want to receive.
                            </Typography>
                        }

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