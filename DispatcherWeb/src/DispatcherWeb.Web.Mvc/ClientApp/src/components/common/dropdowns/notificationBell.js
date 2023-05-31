import React, { useEffect, useState } from 'react'
import { useSelector, useDispatch } from 'react-redux'
import { Link } from 'react-router-dom'
import {
    Badge,
    Box,
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
    Menu,
    MenuItem,
    Modal,
    Stack,
    Switch,
    Typography
} from '@mui/material'
import { grey } from '@mui/material/colors'
import moment from 'moment'
import { HeaderIconButton } from '../../DTComponents'
import { theme } from '../../../Theme'
import { 
    NotificationWrapper, 
    NotificationContent, 
    NotificationHeader, 
    NotificationItem,
    NotificationFooter,
    MarkAllAsReadButton,
    ViewAllNotificationsButton } from '../../styled'
import { notificationItems } from '../../../common/data/notifications'
import { getUserNotifications } from '../../../store/actions'
import { isEmpty } from 'lodash'
import { getFormattedMessageFromUserNotification, getUrl } from '../../../helpers/notification_helper'
import { notificationState } from '../../../common/enums/notificationState'

export const NotificationBell = ({
    isMobileView
}) => {
    const [anchorNotif, setAnchorNotif] = useState(null)
    const isNotification = Boolean(anchorNotif)
    const [notificationsList, setNotificationsList] = useState(notificationItems)
    const [isNotifSettings, setIsNotifSettings] = useState(false)
    const [notificationItemsList, setNotificationItemsList] = useState([])
    const [unReadCount, setUnReadCount] = useState(0)

    const dispatch = useDispatch()
    const { notifications } = useSelector(state => ({
        notifications: state.NotificationBellReducer.notifications
    }))

    useEffect(() => {
        if (isEmpty(notifications)) {
            dispatch(getUserNotifications())
        } else {
            const { result } = notifications
            if (!isEmpty(result)) {
                setUnReadCount(result.unreadCount)
                
                const { items } = result
                if (!isEmpty(items)) {
                    let notifications = []
                    
                    items.forEach((item) => {
                        const { id, notification, state } = item
                        if (!isEmpty(notification)) {
                            const url = getUrl(item)

                            notifications.push({
                                userNotificationId: id,
                                text: getFormattedMessageFromUserNotification(notification),
                                time: moment(notification.creationTime).format('YYYY-MM-DD HH:mm:ss'),
                                state,
                                data: notification.data,
                                url,
                                isUnread: item.state === notificationState.UNREAD
                            })
                        }
                    })
                
                    setNotificationItemsList(notifications)
                }
            }
        }
    }, [dispatch, notifications])
    
    // Handles the opening of the notification dropdown
    const handleNotifClick = (event) => setAnchorNotif(event.currentTarget)

    // Handles the closing of the notification dropdown
    const handleNotifClose = () => setAnchorNotif(null)

    // Handles the setting of notification to read
    const handleNotifRead = (notif) => {
        const updatedNotif = notificationsList.map((item) => {
            if (notif.content===item.content) {
                return {
                    ...item,
                    isRead: true
                }
            }
            return item
        })
        setNotificationsList(updatedNotif)
    }

    // Handles the setting of all notifications to read all
    const handleReadAll = () => {
        const allRead = notificationsList.map((item) => ({
            ...item,
            isRead: true
        }))
        setNotificationsList(allRead)
    }

    // Handles the opening of the notification settings modal
    const handleNotifSettingsOpen = () => setIsNotifSettings(true)

    // Handles the closing of the notification settings modal
    const handleNotifSettingsClose = () => setIsNotifSettings(false)

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
        )
    } else {
        return (
            <React.Fragment>
                <HeaderIconButton 
                    id="notification" 
                    aria-haspopup="true" 
                    aria-expanded={isNotification ? "true" : undefined}
                    aria-label="notification"
                    onClick={handleNotifClick}
                >
                    <Badge color="error" variant="dot" invisible={false}>
                        <i className="fa-regular fa-bell icon"></i>
                    </Badge>
                </HeaderIconButton>
                
                <NotificationWrapper 
                    id="notification-list"
                    anchorEl={anchorNotif} 
                    open={isNotification} 
                >
                        <NotificationContent>
                            <NotificationHeader>
                                <Typography variant="subtitle1" color="white" fontWeight={700}>
                                    Notifications {/* {`Notifications (${notificationItems.length})`} */}
                                </Typography>
                                <Button 
                                    variant="text" 
                                    size="small" 
                                    sx={{ color: "white", fontSize: "caption" }}
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
                                                padding: '10px 8px',
                                                borderRadius: '8px',
                                                '&:hover': {
                                                    backgroundColor: '#f1f5f8'
                                                }
                                            }}
                                        >
                                            <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                                <Box sx={{ display: "flex" }}>
                                                    <Badge 
                                                        color="success"
                                                        variant="dot" 
                                                        invisible={!notification.isUnread} 
                                                        sx={{ mt: 1.5 }} 
                                                    />

                                                    <Typography
                                                        component={Link}
                                                        to={notification.url}
                                                        onClick={handleNotifClose}
                                                        sx={{
                                                            pl: 1,
                                                            overflow: "hidden",
                                                            textOverflow: "ellipsis",
                                                            display: "-webkit-box",
                                                            WebkitLineClamp: "2",
                                                            WebkitBoxOrient: "vertical",
                                                            textDecoration: "none",
                                                            color: theme.palette.text.primary,
                                                        }}
                                                        fontWeight={notification.isUnread ? 500 : 400}
                                                    >
                                                        {notification.text}
                                                    </Typography>
                                                </Box>

                                                <IconButton
                                                    size="small"
                                                    onClick={() => handleNotifRead(notification)}
                                                    sx={{
                                                        p: 0,
                                                        width: "1.2rem",
                                                        height: "1.2rem",
                                                        display: notification.isUnread !== true ? "none" : "block",
                                                    }}
                                                >
                                                    <i className="fa-regular fa-eye secondary-icon" style={{ fontSize: 12 }}></i>
                                                </IconButton>
                                            </Box>
                                            <Box>
                                                <Typography
                                                    variant="caption"
                                                    color="grey"
                                                    sx={{ pl: 1 }}
                                                >
                                                    {notification.time}
                                                </Typography>
                                            </Box>
                                        </Stack>
                                    </NotificationItem>
                                )
                            })}
    
                            <NotificationFooter>
                                <IconButton onClick={handleNotifSettingsOpen}>
                                    <i className="fa-regular fa-gear" style={{ color: grey[500] }} />
                                </IconButton>

                                <Modal
                                    open={isNotifSettings}
                                    onClose={handleNotifSettingsClose}
                                    aria-labelledby="notification-settings"
                                >
                                    <Card
                                        sx={{
                                            minWidth: 500,
                                            position: "absolute",
                                            top: "30%",
                                            left: "50%",
                                            transform: "translate(-50%, -50%)",
                                        }}
                                    >
                                        <CardHeader
                                            action={
                                                <IconButton
                                                    aria-label="close"
                                                    onClick={handleNotifSettingsClose}
                                                >
                                                    <i className="fa-regular fa-close"></i>
                                                </IconButton>
                                            }
                                            title="Notification Settings" 
                                        />

                                        <CardContent>
                                            <FormGroup>
                                                <FormControlLabel
                                                    control={<Switch defaultChecked />}
                                                    label="Receive notifications" 
                                                />

                                                <Typography
                                                    color={theme.palette.text.secondary}
                                                    variant="caption"
                                                >
                                                    This option can be used to completely enable/disable
                                                    receiving notifications.
                                                </Typography>

                                                <Divider sx={{ my: 3 }} />

                                                <FormControlLabel
                                                    control={<Checkbox defaultChecked />}
                                                    label="On a new user registered with the application." 
                                                />
                                            </FormGroup>
                                        </CardContent>

                                        <CardActions sx={{ justifyContent: "end" }}>
                                            <Button onClick={handleNotifSettingsClose}>
                                                Cancel
                                            </Button>

                                            <Button
                                                variant="contained"
                                                onClick={handleNotifSettingsClose}
                                                startIcon={<i className="fa-regular fa-save"></i>}
                                            >
                                                Save
                                            </Button>
                                        </CardActions>
                                    </Card>
                                </Modal>

                                <MarkAllAsReadButton 
                                    variant="text" 
                                    size="small" 
                                    sx={{ fontSize: "caption" }}
                                    onClick={handleReadAll}
                                >
                                    <i class="fa-regular fa-check-double"></i> Mark all as read
                                </MarkAllAsReadButton>

                                <ViewAllNotificationsButton>
                                    View all notifications
                                </ViewAllNotificationsButton>
                            </NotificationFooter>
                        </NotificationContent>
                </NotificationWrapper>
            </React.Fragment>
        )
    }
}