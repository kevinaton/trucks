import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Paper, useMediaQuery } from '@mui/material';
import Box from '@mui/material/Box';
import CssBaseline from '@mui/material/CssBaseline';
import './fontawesome/css/all.css';
import { RouterConfig } from './navigation/RouterConfig';
import { DrawerHeader } from './components/DTComponents';
import { sideMenuItems } from './common/data/menus';
import { Appbar, SideMenu } from './components';
import { getUserInfo } from './store/actions';
import { isEmpty } from 'lodash';
import { baseUrl } from './helpers/api_helper';

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

    return (
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
    );
};

export default App;
