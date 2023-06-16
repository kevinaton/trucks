import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Avatar,
    Box,
    Button,
    IconButton,
    Menu,
    MenuItem,
    MenuList,
    Toolbar,
    Typography,
} from '@mui/material';
import { drawerWidth, AppBar, HeaderIconButton } from '../../DTComponents';
import { NotificationBell } from '../dropdowns';
import { ProfileMenu } from '../../user-profile';
import { getSupportLinkAddress } from '../../../store/actions';
import { isEmpty } from 'lodash';
import ChatPane from '../../Chatpane';

export const Appbar = ({
    isAuthenticated,
    drawerOpen,
    handleDrawerClose,
    handleDrawerOpen,
    handleOpenNavMenu,
    anchorElNav,
    handleCloseNavMenu,
    openModal,
}) => {
    const [linkAddress, setLinkAddress] = useState(null);
    const dispatch = useDispatch();
    const { supportLinkAddress } = useSelector((state) => ({
        supportLinkAddress: state.LayoutReducer.supportLinkAddress,
    }));
    const [state, setState] = React.useState(false);

    useEffect(() => {
        if (isAuthenticated) {
            if (supportLinkAddress === null) {
                dispatch(getSupportLinkAddress());
            } else {
                const { result } = supportLinkAddress;
                if (!isEmpty(result)) {
                    setLinkAddress(result);
                }
            }
        }
    }, [dispatch, supportLinkAddress, isAuthenticated]);

    return (
        <div>
            <AppBar position='fixed' open={drawerOpen} color='inherit' elevation={5}>
                <Toolbar sx={{ p: 0 }} disableGutters>
                    <Box sx={{ flexGrow: 1 }}>
                        {drawerOpen ? (
                            <Box
                                sx={{
                                    ml: 2,
                                    display: 'flex',
                                    width: drawerWidth - 26,
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                }}>
                                <img
                                    alt=''
                                    width='30%'
                                    height='100%'
                                    src='/reactapp/assets/dumptruckdispatcher-logo.png'
                                />
                                <IconButton onClick={handleDrawerClose} aria-label='close drawer'>
                                    <i className='fa-regular fa-bars icon'></i>
                                </IconButton>
                            </Box>
                        ) : (
                            <IconButton
                                aria-label='open drawer'
                                onClick={handleDrawerOpen}
                                edge='start'
                                sx={{
                                    ml: 2,
                                    ...(drawerOpen && { display: 'none' }),
                                }}>
                                <i className='fa-regular fa-bars icon'></i>
                            </IconButton>
                        )}
                    </Box>

                    {/* Mobile view */}
                    <Box sx={{ display: { xs: 'flex', md: 'none' }, padding: 0 }}>
                        <HeaderIconButton
                            aria-label='header-menu'
                            color='inherit'
                            onClick={handleOpenNavMenu}>
                            <i className='fa-regular fa-ellipsis-vertical icon'></i>
                        </HeaderIconButton>
                        <Menu
                            id='menu-appbar'
                            anchorEl={anchorElNav}
                            anchorOrigin={{
                                vertical: 'bottom',
                                horizontal: 'right',
                            }}
                            keepMounted
                            transformOrigin={{
                                vertical: 'top',
                                horizontal: 'left',
                            }}
                            open={Boolean(anchorElNav)}
                            onClose={handleCloseNavMenu}
                            elevation={5}
                            sx={{
                                display: { sm: 'flex', md: 'none' },
                                padding: 0,
                            }}>
                            <MenuList
                                className='header-menu'
                                sx={{
                                    display: 'flex',
                                    flexDirection: 'row',
                                    padding: 0,
                                    alignContent: 'center',
                                }}>
                                <MenuItem key='support'>
                                    <HeaderIconButton p={0} aria-label='support'>
                                        <i
                                            className='fa-duotone fa-life-ring icon'
                                            style={{
                                                '--fa-primary-opacity': '0.3',
                                                '--fa-secondary-opacity': '1',
                                            }}></i>
                                    </HeaderIconButton>
                                </MenuItem>

                                <NotificationBell isMobileView={true} />

                                <MenuItem key='user'>
                                    <Button p={0}>
                                        <Avatar
                                            alt='account'
                                            src='/reactapp/assets/images/150.jpg'
                                            sx={{ mr: 1, width: 24, height: 24 }}
                                        />
                                        <Typography sx={{ fontWeight: 600, fontSize: 12 }}>
                                            User
                                        </Typography>
                                    </Button>
                                </MenuItem>

                                <MenuItem key='message'>
                                    <IconButton
                                        p={0}
                                        aria-label='open drawer'
                                        onClick={() => {
                                            setState(true);
                                            handleCloseNavMenu();
                                        }}>
                                        <i className='fa-regular fa-message-dots icon'></i>
                                    </IconButton>
                                </MenuItem>
                            </MenuList>
                        </Menu>
                    </Box>

                    {/* Desktop view */}
                    <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
                        <HeaderIconButton aria-label='support' href={linkAddress}>
                            <i
                                className='fa-duotone fa-life-ring icon'
                                style={{
                                    '--fa-primary-opacity': '0.3',
                                    '--fa-secondary-opacity': '1',
                                }}
                            ></i>
                        </HeaderIconButton>

                        <NotificationBell isMobileView={false} />

                        <ProfileMenu openModal={openModal} />

                        <HeaderIconButton aria-label='open drawer' onClick={() => setState(true)}>
                            <i className='fa-regular fa-message-dots icon'></i>
                        </HeaderIconButton>
                    </Box>
                </Toolbar>
            </AppBar>

            <ChatPane state={state} setState={setState} />
        </div>
    );
};
