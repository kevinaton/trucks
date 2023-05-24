import React, { useEffect, useState } from 'react'
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
    Paper,
    Stack,
    Switch,
    Typography
} from '@mui/material'
import { grey } from '@mui/material/colors'
import { HeaderIconButton } from '../../DTComponents'
import { theme } from '../../../Theme'
import { notificationItems } from '../../../common/data/notifications'

export const NotificationBell = () => {
    const [anchorNotif, setAnchorNotif] = useState(null)
    const isNotification = Boolean(anchorNotif)
    const [notificationsList, setNotificationsList] = useState(notificationItems)
    const [isNotifSettings, setIsNotifSettings] = useState(false)
    
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
            
            <Menu 
                id="notification-list"
                anchorEl={anchorNotif} 
                open={isNotification} 
                onClose={handleNotifClose} 
            >
                    <Paper sx={{ width: 380 }}>
                        <Box 
                            sx={{
                                backgroundColor: (theme) => theme.palette.primary.main,
                                px: 2,
                                py: 1,
                                display: "flex",
                                justifyContent: "space-between"
                            }}
                        >
                                <Typography variant="subtitle1" color="white" fontWeight={700}>
                                    {notificationItems.length} Notifications
                                </Typography>
                                <Box>
                                    <Button 
                                        variant="text" 
                                        size="small" 
                                        sx={{ color: "white", fontSize: "caption" }}
                                        onClick={handleReadAll}>
                                        Set all as read
                                    </Button>

                                    <IconButton onClick={handleNotifSettingsOpen}>
                                        <i className="fa-regular fa-gear" style={{ color: '#fff' }} />
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
                                                        onClick={handleNotifSettingsClose}>
                                                        <i className="fa-regular fa-close"></i>
                                                    </IconButton>
                                                }
                                                title="Notification Settings" />

                                            <CardContent>
                                                <FormGroup>
                                                    <FormControlLabel
                                                        control={<Switch defaultChecked />}
                                                        label="Receive notifications" />

                                                    <Typography
                                                        color={theme.palette.text.secondary}
                                                        variant="caption">
                                                        This option can be used to completely enable/disable
                                                        receiving notifications.
                                                    </Typography>

                                                    <Divider sx={{ my: 3 }} />

                                                    <FormControlLabel
                                                        control={<Checkbox defaultChecked />}
                                                        label="On a new user registered with the application." />
                                                </FormGroup>
                                            </CardContent>

                                            <CardActions sx={{ justifyContent: "end" }}>
                                                <Button onClick={handleNotifSettingsClose}>
                                                    Cancel
                                                </Button>

                                                <Button
                                                    variant="contained"
                                                    onClick={handleNotifSettingsClose}
                                                    startIcon={<i className="fa-regular fa-save"></i>}>
                                                    Save
                                                </Button>
                                            </CardActions>
                                        </Card>
                                    </Modal>
                                </Box>
                            </Box>

                        { notificationsList.map((notification, index) => {
                            return (
                                <MenuItem 
                                    key={index} 
                                    style={{ flexGrow: 1, whiteSpace: "normal" }}
                                >
                                        <Stack sx={{ width: 1 }}>
                                            <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                                <Box sx={{ display: "flex" }}>
                                                    <Badge 
                                                        color="success"
                                                        variant="dot" 
                                                        invisible={notification.isRead} 
                                                        sx={{ mt: 1.5 }} 
                                                    />

                                                    <Typography
                                                        component={Link}
                                                        to={notification.path}
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
                                                        fontWeight={!notification.isRead ? 600 : 400}
                                                    >
                                                        {notification.content}
                                                    </Typography>
                                                </Box>

                                                <IconButton
                                                    size="small"
                                                    onClick={() => handleNotifRead(notification)}
                                                    sx={{
                                                        p: 0,
                                                        width: "1.2rem",
                                                        height: "1.2rem",
                                                        display:
                                                        notification.isRead === true ? "none" : "block",
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
                                </MenuItem>
                            )
                        })}

                        <MenuItem sx={{
                            display: "flex",
                            justifyContent: "center",
                            backgroundColor: grey[100]
                        }}>
                            <Typography color="primary">See more</Typography>
                        </MenuItem>
                    </Paper>
            </Menu>
        </React.Fragment>
    )
}