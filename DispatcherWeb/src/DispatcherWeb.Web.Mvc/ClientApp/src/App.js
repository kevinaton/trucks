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
import { Appbar, SideMenu } from './components';
import { getUserInfo } from './store/actions';
import { isEmpty } from 'lodash';
import { baseUrl } from './helpers/api_helper';
import * as signalR from '@microsoft/signalr';
import SignalRContext from './components/common/signalr/signalrContext';
import { CustomModal } from './components/common/modals/customModal';

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
    const [connection, setConnection] = useState(null);
    const [modals, setModals] = useState([]);
    const [nextModalZIndex, setNextModalZIndex] = useState(1);

    const userInfo = useSelector(state => state.UserReducer.userInfo);
    const dispatch = useDispatch();

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
        if (isAuthenticated && isEmpty(connection)) {
            const startConnection = (transport) => {
                const url = `${baseUrl}/signalr`;
                const newConnection = new signalR.HubConnectionBuilder()
                    .withUrl(url, transport)
                    .withAutomaticReconnect()
                    .build();

                newConnection.onclose((err) => {
                    if (err) {
                        console.log('Connection closed with error: ', err);
                    } else {
                        console.log('Disconnected');
                    }

                    setTimeout(() => {
                        newConnection.start();
                    }, 5000);
                });

                newConnection
                    .start()
                    .then(() => {
                        setConnection(newConnection);
                    })
                    .catch((err) => {
                        console.log(err);
                        if (transport !== signalR.HttpTransportType.LongPolling) {
                            return startConnection(transport + 1);
                        }
                    });
            };
            
            startConnection(signalR.HttpTransportType.WebSockets)
        }
    }, [isAuthenticated, connection]);

    // Checks screen if it is small
    useEffect(() => {
        if (isSmall) {
            setDrawerOpen(false);
        }

        if (isBig) {
            setDrawerOpen(true);
        }
    }, [isSmall, isBig]);

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

    return (
        <SnackbarProvider 
            maxSnack={5} 
            anchorOrigin={{ 
                vertical: 'bottom', 
                horizontal: 'right' 
            }}
        >
            <SignalRContext.Provider value={connection}>
                <Box sx={{ display: 'flex' }}>
                    <CssBaseline />
                    {/* This is the appbar located at the top of the app. */}

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
                            <RouterConfig 
                                isAuthenticated={isAuthenticated} 
                                handleCurrentPageName={handleCurrentPageName} />
                        </Paper>
                    </Box>
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
            </SignalRContext.Provider>
        </SnackbarProvider>
    );
};

export default App;
