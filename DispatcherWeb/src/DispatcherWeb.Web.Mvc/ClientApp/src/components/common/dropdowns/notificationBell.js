import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link } from 'react-router-dom';
import {
    Badge,
    Box,
    Button,
    IconButton,
    Menu,
    MenuItem,
    Stack,
    Typography
} from '@mui/material';
import { grey } from '@mui/material/colors';
import { useSnackbar, closeSnackbar } from 'notistack';
import moment from 'moment';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { HeaderIconButton } from '../../DTComponents';
import { NotificationSettingsModal } from '../modals';
import { theme } from '../../../Theme';
import { 
    NotificationWrapper, 
    NotificationContent, 
    NotificationHeader, 
    NotificationItem,
    NotificationFooter,
    MarkAllAsReadButton
} from '../../styled';
import { 
    getUserSettingsByName,
    getUserNotifications, 
    getUserPriorityNotifications,
    setAllNotificationsAsRead as onSetAllNotificationsAsRead,
    setNotificationAsRead as onSetNotificationAsRead
} from '../../../store/actions';
import _, { isEmpty } from 'lodash';
import { 
    getFormattedMessageFromUserNotification, 
    getUrl,
    getUserNotificationStateAsString
} from '../../../helpers/notification_helper';
import { notificationState } from '../../../common/enums/notificationState';
import { baseUrl } from '../../../helpers/api_helper';

export const NotificationBell = ({
    isMobileView
}) => {
    const [anchorNotif, setAnchorNotif] = useState(null);
    const isNotification = Boolean(anchorNotif);
    const [isNotifSettings, setIsNotifSettings] = useState(false);
    const [notificationItemsList, setNotificationItemsList] = useState([]);
    const [unReadCount, setUnReadCount] = useState(0);
    const [initializedPrioNotif, setInitializedPrioNotif] = useState(false);
    const [soundEnabled, setSoundEnabled] = useState(null);
    const [isConnected, setIsConnected] = useState(false);

    const dispatch = useDispatch();
    const { 
        userSettings,
        notifications, 
        priorityNotifications 
    } = useSelector(state => ({
        userSettings: state.UserReducer.userSettings,
        notifications: state.NotificationReducer.notifications,
        priorityNotifications: state.NotificationReducer.priorityNotifications
    }));
    
    const { enqueueSnackbar } = useSnackbar();

    useEffect(() => {
        dispatch(getUserSettingsByName('App.UserOptions.PlaySoundForNotifications'));
    }, [dispatch]);

    useEffect(() => {
        if (!isEmpty(userSettings) && isEmpty(soundEnabled)) {
            setSoundEnabled(userSettings.result);
        }
    }, [userSettings, soundEnabled]);
    
    useEffect(() => {
        console.log('soundEnabled: ', soundEnabled)
        console.log('soundEnabled !== null: ', soundEnabled !== null)
        console.log('!isConnected: ', !isConnected)

        let connection;

        if (soundEnabled !== null && !isConnected) {
            connection = new HubConnectionBuilder()
                .withUrl(`${baseUrl}/signalr`)
                .withAutomaticReconnect()
                .build();

            connection.start()
                .then(() => {
                    setIsConnected(true);
                    console.log('SignalR connection established');

                    connection.on('getNotification', (notification) => {
                        console.log('notification: ', notification);
                        if (soundEnabled) {
                            const audioNotification = new Audio('/reactapp/sounds/notification.mp3');
                            audioNotification.play();
                        }
                        // const msg = getFormattedMessageFromUserNotification(notification);
                        // enqueueSnackbar(msg, {
                        //     variant: 'warning',
                        //     preventDuplicate: true,
                        //     action: (key) => (
                        //         <Button
                        //             onClick={() => { 
                        //                 closeSnackbar(key);
                        //                 handleNotificationClick(notification);
                        //             }}
                        //             style={{ color: theme.palette.common.white }}
                        //         >
                        //             View
                        //         </Button>
                        //     )
                        // });
                    }); 
                })
                .catch(error => console.log('SignalR connection error: ', error));
        }

        // Cleanup function for disconnecting the SignalR connection
        return () => {
            if (connection && connection.state === 'Connected') {
                connection.stop();
                console.log('SignalR connection closed');
            }
        };
    }, [soundEnabled, isConnected]);

    useEffect(() => {
        if (isEmpty(notifications)) {
            dispatch(getUserNotifications());
        } else {
            const { result } = notifications;
            if (!isEmpty(result)) {
                setUnReadCount(result.unreadCount);
                
                const { items } = result;
                if (!isEmpty(items)) {
                    let userNotifications = [];
                    
                    items.forEach((item) => {
                        const { id, notification, state } = item;
                        if (!isEmpty(notification)) {
                            const url = getUrl(item);

                            userNotifications.push({
                                userNotificationId: id,
                                text: getFormattedMessageFromUserNotification(notification),
                                time: moment(notification.creationTime).format('YYYY-MM-DD HH:mm:ss'),
                                state: getUserNotificationStateAsString(state),
                                data: notification.data,
                                url,
                                isUnread: item.state === notificationState.UNREAD,
                                timeAgo: moment(notification.creationTime).fromNow()
                            });
                        }
                    });
                
                    setNotificationItemsList(userNotifications);
                }
            }
        }
    }, [dispatch, notifications]);

    useEffect(() => {
        const init = async () => {
            if (isEmpty(priorityNotifications)) {
                dispatch(getUserPriorityNotifications());
            } else {
                const { result } = priorityNotifications;
                if (!isEmpty(result) && !initializedPrioNotif) {
                    let { items } = result;
                    if (!isEmpty(items)) {
                        items = _.uniqBy(items, 'id');

                        for (const item of items) {
                            await new Promise((resolve) => setTimeout(resolve, 1000));
                            
                            const { id, notification } = item;
                            if (!isEmpty(notification)) {
                                const msg = getFormattedMessageFromUserNotification(notification);
                                enqueueSnackbar(msg, {
                                    variant: 'warning',
                                    preventDuplicate: true, 
                                    persist: true,
                                    action: snackbarId => (
                                        <IconButton 
                                            onClick={(e) => handleDismissPriorityNotif(e, snackbarId, id)}
                                            size='small' 
                                        >
                                            <i className='fa-regular fa-close'></i>
                                        </IconButton>
                                    )
                                })
                            }
                        }

                        setInitializedPrioNotif(true);
                    }
                }
            }
        };

        init();
    }, [
        dispatch, 
        initializedPrioNotif,
        priorityNotifications,
        enqueueSnackbar
    ]);

    // Handles the closing of the priority notification
    const handleDismissPriorityNotif = (e, snackbarId, id) => {
        e.preventDefault();

        handleNotifRead({ userNotificationId: id });
        closeSnackbar(snackbarId);
    };
    
    // Handles the opening of the notification dropdown
    const handleNotifClick = (event) => setAnchorNotif(event.currentTarget);

    // Handles the closing of the notification dropdown
    const handleNotifClose = () => setAnchorNotif(null);

    // Handles the setting of notification to read
    const handleNotifRead = (notif) => {
        dispatch(onSetNotificationAsRead({
            id: notif.userNotificationId
        }));
    };

    // Handles the setting of all notifications to read all
    const handleReadAll = () => {
        dispatch(onSetAllNotificationsAsRead());
    };

    // Handles the opening of the notification settings modal
    const handleNotifSettingsOpen = () => setIsNotifSettings(true);

    // Handles the closing of the notification settings modal
    const handleNotifSettingsClose = () => setIsNotifSettings(false);

    const handleViewAllNotifications = () => window.location.href = `${baseUrl}/app/notifications`;

    if (isMobileView) {
        return (
            <React.Fragment>
                <MenuItem key='notification'>
                    <IconButton 
                        id='notification' 
                        aria-haspopup='true'
                        aria-expanded={isNotification ? 'true' : undefined}
                        onClick={handleNotifClick} 
                        p={0} 
                        aria-label='open drawer'
                    >
                        <i className='fa-regular fa-bell icon'></i>
                    </IconButton>
                </MenuItem>
                <Menu
                    id='notification-list' 
                    anchorEl={anchorNotif}
                    open={isNotification} 
                    onClose={handleNotifClose}
                >
                    <MenuItem to='/' onClick={handleNotifClose}>
                        Profile
                    </MenuItem>
                </Menu>
            </React.Fragment>
        );
    } else {
        return (
            <React.Fragment>
                <HeaderIconButton 
                    id='notification' 
                    aria-haspopup='true' 
                    aria-expanded={isNotification ? 'true' : undefined}
                    aria-label='notification'
                    onClick={handleNotifClick}
                >
                    <Badge color='error' variant='dot' invisible={unReadCount===0}>
                        <i className='fa-regular fa-bell icon'></i>
                    </Badge>
                </HeaderIconButton>
                
                <NotificationWrapper 
                    id='notification-list'
                    anchorEl={anchorNotif} 
                    open={isNotification} 
                >
                        <NotificationContent>
                            <NotificationHeader>
                                <Typography variant='subtitle1' color='white' fontWeight={700}>
                                    {`Notifications ${unReadCount > 0 ? `(${unReadCount})` : ''}`}
                                </Typography>
                                <Button 
                                    variant='text' 
                                    size='small' 
                                    sx={{ color: 'white', fontSize: 'caption' }}
                                    onClick={handleNotifClose}
                                >
                                    <i className='fa-regular fa-close' />
                                </Button>
                            </NotificationHeader>
    
                            { notificationItemsList.map((notification, index) => {
                                return (
                                    <NotificationItem key={index}>
                                        <Stack 
                                            sx={{ 
                                                width: 1,
                                                backgroundColor: !notification.isUnread ? 'transparent' : '#f1f5f8',
                                                padding: '6px 8px',
                                                borderRadius: '8px',
                                                '&:hover': {
                                                    backgroundColor: '#f1f5f8'
                                                }
                                            }}
                                        >
                                            <Box sx={{ display: 'flex', justifyContent: 'flex-start' }}>
                                                <Badge 
                                                    color='success'
                                                    variant='dot' 
                                                    invisible={!notification.isUnread} 
                                                    sx={{ mt: 1.5 }} 
                                                />

                                                <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                                                    <Typography
                                                        component={Link}
                                                        to={notification.url}
                                                        onClick={handleNotifClose}
                                                        sx={{
                                                            pl: 1,
                                                            overflow: 'hidden',
                                                            textOverflow: 'ellipsis',
                                                            display: '-webkit-box',
                                                            WebkitLineClamp: '2',
                                                            WebkitBoxOrient: 'vertical',
                                                            textDecoration: 'none',
                                                            color: theme.palette.text.primary,
                                                            fontWeight: notification.isUnread ? 500 : 400
                                                        }}
                                                    >
                                                        {notification.text}
                                                    </Typography>
                                                    
                                                    <Typography
                                                        variant='caption'
                                                        color='grey'
                                                        sx={{ pl: 1 }}
                                                    >
                                                        {notification.timeAgo}
                                                    </Typography>
                                                </Box>

                                                <IconButton
                                                    size='small'
                                                    onClick={() => handleNotifRead(notification)}
                                                    sx={{
                                                        p: 0,
                                                        width: '1.2rem',
                                                        height: '1.2rem',
                                                        display: notification.isUnread !== true ? 'none' : 'block',
                                                        marginLeft: 'auto',
                                                    }}
                                                >
                                                    <i className='fa-regular fa-eye secondary-icon' style={{ fontSize: 12 }}></i>
                                                </IconButton>
                                            </Box>
                                        </Stack>
                                    </NotificationItem>
                                )
                            })}
    
                            <NotificationFooter>
                                <IconButton onClick={handleNotifSettingsOpen}>
                                    <i className='fa-regular fa-gear' style={{ color: grey[500] }} />
                                </IconButton>

                                <NotificationSettingsModal 
                                    open={isNotifSettings}
                                    onClose={handleNotifSettingsClose} 
                                    labelledBy='notification-settings'
                                    title='Notification Settings'
                                />

                                <MarkAllAsReadButton 
                                    variant='text' 
                                    size='small' 
                                    sx={{ fontSize: 'caption' }}
                                    onClick={handleReadAll}
                                >
                                    <i class='fa-regular fa-check-double'></i> Set all as read
                                </MarkAllAsReadButton>

                                <Button variant='outlined' size='small' onClick={handleViewAllNotifications}>
                                    See more
                                </Button>
                            </NotificationFooter>
                        </NotificationContent>
                </NotificationWrapper>
            </React.Fragment>
        );
    }
};