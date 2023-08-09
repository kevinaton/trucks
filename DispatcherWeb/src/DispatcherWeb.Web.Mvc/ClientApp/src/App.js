import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Paper, useMediaQuery } from '@mui/material';
import Box from '@mui/material/Box';
import CssBaseline from '@mui/material/CssBaseline';
import { SnackbarProvider } from 'notistack';
import './fontawesome/css/all.css';
import { RouterConfig } from './navigation/RouterConfig';
import { DrawerHeader } from './components/DTComponents';
import { sideMenuItems } from './common/data/menus';
import { Appbar, SideMenu, ProgressBar } from './components';
import { getUserAppConfig, getUserGeneralSettings, getUserInfo } from './store/actions';
import { isEmpty } from 'lodash';
import { baseUrl } from './helpers/api_helper';
import * as signalR from '@microsoft/signalr';
import moment from 'moment';
import 'moment-timezone';
import SignalRContext from './components/common/signalr/signalrContext';
import SyncRequestContext from './components/common/signalr/syncRequestContext';
import { CustomModal } from './components/common/modals/customModal';
import { CustomDialog } from './components/common/dialogs/customDialog';

const App = (props) => {
    const [anchorElNav, setAnchorElNav] = useState(null);
    const [drawerOpen, setDrawerOpen] = useState(true);
    const isSmall = useMediaQuery((theme) => theme.breakpoints.down('lg'));
    const isBig = useMediaQuery((theme) => theme.breakpoints.up('lg'));
    const [collapseOpen, setCollapseOpen] = useState(
        sideMenuItems.reduce((acc, menu) => {
            if (menu.submenu) {
                acc[menu.name] = false;
            }
            return acc;
        }, {})
    );
    const [currentPageName, setCurrentPageName] = useState('');
    const [isAuthenticated, setIsAuthenticated] = useState(null);
    const [userAppConfiguration, setUserAppConfiguration] = useState(null);
    const [generalSettings, setGeneralSettings] = useState(null);
    const [isConnecting, setIsConnecting] = useState(false);
    const [connection, setConnection] = useState(null);
    const [isSyncRequestConnecting, setIsSyncRequestConnecting] = useState(false);
    const [syncRequestConnection, setSyncRequestConnection] = useState(null);
    const [modals, setModals] = useState([]);
    const [nextModalZIndex, setNextModalZIndex] = useState(1);
    const [dialog, setDialog] = useState(null);
    const [isUIBusy, setIsUIBusy] = useState(false);

    const dispatch = useDispatch();
    const {
        userInfo,
        userAppConfig,
        userGeneralSettings
    } = useSelector(state => ({
        userInfo: state.UserReducer.userInfo,
        userAppConfig: state.UserReducer.userAppConfig,
        userGeneralSettings: state.UserReducer.userGeneralSettings
    }));

    // Checks screen if it is small
    useEffect(() => {
        if (isSmall) {
            setDrawerOpen(false);
        }

        if (isBig) {
            setDrawerOpen(true);
        }
    }, [isSmall, isBig]);

    useEffect(() => {
        const checkLoginStatus = async () => {
            if (isEmpty(userInfo)) {
                dispatch(getUserInfo());
            } else {
                const { result } = userInfo;
                if (!isEmpty(result) && !isEmpty(result.user)) {
                    setIsAuthenticated(true);
                } else {
                    window.location.href = `${baseUrl}/Account/Login`;
                }
            }
        }

        checkLoginStatus();
    }, [dispatch, userInfo]);

    useEffect(() => {
        if (isAuthenticated) {
            if (connection === null && !isConnecting) {
                setIsConnecting(true);
                const startConnection = (transport) => {
                    const hubConnection = getHubConnection('signalr', transport);
                    hubConnection.start()
                        .then(() => {
                            setConnection(hubConnection);
                            setIsConnecting(false);
                        })
                        .catch((err) => {
                            if (transport !== signalR.HttpTransportType.LongPolling) {
                                return startConnection(transport + 1);
                            }
                        });
                };
                
                startConnection(signalR.HttpTransportType.WebSockets);
            }

            if (syncRequestConnection === null && !isSyncRequestConnecting) {
                setIsSyncRequestConnecting(true);
                const startSyncRequestConnection = (transport) => {
                    const hubConnection = getHubConnection('signalr-dispatcher', transport);
                    hubConnection.start()
                        .then(() => {
                            setSyncRequestConnection(hubConnection);
                            setIsSyncRequestConnecting(false);
                        })
                        .catch((err) => {
                            if (transport !== signalR.HttpTransportType.LongPolling) {
                                return startSyncRequestConnection(transport + 1);
                            }
                        });
                };
                startSyncRequestConnection(signalR.HttpTransportType.WebSockets);
            }
        }
    }, [isAuthenticated, connection, syncRequestConnection]);

    useEffect(() => {
        if (isAuthenticated) {
            dispatch(getUserAppConfig());
            dispatch(getUserGeneralSettings());
        }
    }, [dispatch, isAuthenticated]);

    useEffect(() => {
        if (userAppConfiguration === null && 
            !isEmpty(userAppConfig) && 
            !isEmpty(userAppConfig.result)
        ) {
            const { result } = userAppConfig;
            if (!isEmpty(result)) {
                setUserAppConfiguration(result);
            }
        }
    }, [userAppConfig, userAppConfiguration]);

    useEffect(() => {
        if (generalSettings === null && 
            !isEmpty(userGeneralSettings) && 
            !isEmpty(userGeneralSettings.result)
        ) {
            const { result } = userGeneralSettings;
            if (!isEmpty(result) && result.timezoneIana) {
                setGeneralSettings(result);
                // set moment timezone
                moment.tz.setDefault(result.timezoneIana);
            }
        }
    }, [generalSettings, userGeneralSettings]);

    const getHubConnection = (url, transport) => {
        const connectionBuilder = new signalR.HubConnectionBuilder()
            .withUrl(`${baseUrl}/${url}`, transport)
            .withAutomaticReconnect()
            .build();

        connectionBuilder.onclose((err) => {
            if (err) {
                console.log('Connection closed with error: ', err);
            } else {
                console.log('Disconnected');
            }

            setTimeout(() => {
                connectionBuilder.start();
            }, 5000);
        });

        return connectionBuilder;
    };

    const handleCurrentPageName = (name) => {
        document.title = name;
        setCurrentPageName(name);
    };

    const handleOpenNavMenu = (event) => {
        setAnchorElNav(event.currentTarget);
    };

    const handleCloseNavMenu = () => {
        setAnchorElNav(null);
    };

    const handleDrawerOpen = () => {
        setDrawerOpen(true);
    };

    const handleDrawerClose = () => {
        setDrawerOpen(false);
        setCollapseOpen(false);
    };

    const openModal = (content, size) => {
        const modal = {
            content,
            open: true,
            zIndex: nextModalZIndex, // Assign the next available z-index value
            size
        };

        setNextModalZIndex((prevZIndex) => prevZIndex + 1); // Increment the next available z-index value
        setModals((prevModals) => [...prevModals, modal]);
    };

    const closeModal = () => {
        setModals((prevModals) => {
            const updatedModals = [...prevModals];
            updatedModals.pop();
            return updatedModals;
        });
    };

    const openDialog = (data) => {
        const { 
            type, 
            title, 
            content, 
            action, 
            primaryBtnText,
            secondaryBtnText 
        } = data;
        setDialog({
            open: true,
            type,
            title,
            content,
            action,
            primaryBtnText,
            secondaryBtnText
        });
    };

    const closeDialog = () => {
        setDialog({
            ...dialog,
            open: false,
            type: '',
            title: '',
            content: null,
            action: null
        })
    };

    return (
        <SnackbarProvider 
            maxSnack={5} 
            anchorOrigin={{ 
                vertical: 'bottom', 
                horizontal: 'right' 
            }}
        >
            <SignalRContext.Provider value={connection}>
                { isUIBusy && <ProgressBar /> }

                <Box sx={{ display: 'flex' }}>
                    <CssBaseline />
                    {/* This is the appbar located at the top of the app. */}

                    { generalSettings !== null &&
                        <React.Fragment>
                            <Appbar 
                                isAuthenticated={isAuthenticated}
                                drawerOpen={drawerOpen}
                                handleDrawerClose={handleDrawerClose}
                                handleDrawerOpen={handleDrawerOpen}
                                handleOpenNavMenu={handleOpenNavMenu}
                                anchorElNav={anchorElNav}
                                handleCloseNavMenu={handleCloseNavMenu} 
                                openModal={(content, size) => openModal(content, size)} 
                                closeModal={closeModal} 
                                openDialog={(data) => openDialog(data)}
                                closeDialog={closeDialog}
                            />

                            <SideMenu 
                                isAuthenticated={isAuthenticated}
                                currentPageName={currentPageName}
                                drawerOpen={drawerOpen}
                                DrawerHeader={DrawerHeader}
                                collapseOpen={collapseOpen}
                                isSmall={isSmall}
                                setCollapseOpen={setCollapseOpen}
                                handleDrawerOpen={handleDrawerOpen}
                                handleDrawerClose={handleDrawerClose}
                            />
                    
                            <Box 
                                component='main' 
                                sx={{ flexGrow: 1, height: '100%', overflow: 'auto' }}
                            >
                                <Paper
                                    sx={{
                                        backgroundColor: '#f1f5f8',
                                        padding: 2,
                                        height: '100vh',
                                        overflow: 'auto',
                                        pb: '50px',
                                    }}
                                >
                                    <DrawerHeader />

                                    {/* This is the route configuration */}
                                    <SyncRequestContext.Provider value={syncRequestConnection}>
                                        <RouterConfig 
                                            isAuthenticated={isAuthenticated} 
                                            userAppConfiguration={userAppConfiguration}
                                            handleCurrentPageName={handleCurrentPageName} 
                                            openModal={(content, size) => openModal(content, size)} 
                                            closeModal={closeModal} 
                                            openDialog={(data) => openDialog(data)}
                                            closeDialog={closeDialog} 
                                            setIsUIBusy={setIsUIBusy}
                                        />
                                    </SyncRequestContext.Provider>
                                </Paper>
                            </Box>
                        </React.Fragment>
                    }
                </Box>
                
                {/* Render the modals */}
                {modals.map((modal, index) => (
                    <CustomModal 
                        key={index} 
                        open={modal.open} 
                        handleClose={closeModal} 
                        content={modal.content} 
                        zIndex={modal.zIndex} 
                        size={modal.size} 
                    />
                ))}

                {/* Render the dialog */}
                { !isEmpty(dialog) && 
                    <CustomDialog 
                        open={dialog.open} 
                        type={dialog.type}
                        title={dialog.title} 
                        content={dialog.content} 
                        handleClose={closeDialog} 
                        handleProceed={dialog.action}
                    />
                }
            </SignalRContext.Provider>
        </SnackbarProvider>
    );
};

export default App;
