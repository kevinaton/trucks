import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Avatar,
    Box,
    Button,
    IconButton,
    Link,
    Menu,
    MenuItem,
    MenuList,
    Paper,
    Toolbar,
    Typography,
} from '@mui/material';
import { drawerWidth, AppBar, HeaderIconButton, HeaderButton } from '../../DTComponents';
import { getSupportLinkAddress } from '../../../store/actions';
import { theme } from '../../../Theme';
import { isEmpty } from 'lodash';

import { ProfileList } from '../../../common/data/menus';

export const Appbar = ({
    drawerOpen,
    handleDrawerClose,
    handleDrawerOpen,
    handleOpenNavMenu,
    anchorElNav,
    handleCloseNavMenu,
}) => {
    const [linkAddress, setLinkAddress] = useState(null);
    const dispatch = useDispatch();
    const { supportLinkAddress } = useSelector((state) => ({
        supportLinkAddress: state.LayoutReducer.supportLinkAddress,
    }));
    const [anchorProfile, setAnchorProfile] = React.useState(null);
    const isProfile = Boolean(anchorProfile);

    useEffect(() => {
        if (supportLinkAddress === null) {
            dispatch(getSupportLinkAddress());
        } else {
            const { result } = supportLinkAddress;
            if (!isEmpty(result)) {
                setLinkAddress(result);
            }
        }
    }, [dispatch, supportLinkAddress]);

    // Handle showing profile menu
    const handleProfileClick = (event) => {
        setAnchorProfile(event.currentTarget);
    };

    const handleProfileClose = (event) => {
        setAnchorProfile(null);
    };

    return (
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
                    <HeaderIconButton aria-label='header-menu' color='inherit'>
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
                            <MenuItem key='notification'>
                                <IconButton p={0} aria-label='open notification'>
                                    <i className='fa-regular fa-bell icon'></i>
                                </IconButton>
                            </MenuItem>
                            <MenuItem key='user'>
                                <Button p={0}>
                                    <Avatar
                                        alt='account'
                                        src='https://i.pravatar.cc/150?img=3'
                                        sx={{ mr: 1, width: 24, height: 24 }}
                                    />
                                    <Typography sx={{ fontWeight: 600, fontSize: 12 }}>
                                        User
                                    </Typography>
                                </Button>
                            </MenuItem>
                            <MenuItem key='message'>
                                <IconButton p={0} aria-label='open drawer'>
                                    <i className='fa-regular fa-message icon'></i>
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
                            }}></i>
                    </HeaderIconButton>
                    <HeaderIconButton aria-label='open drawer' onClick={handleCloseNavMenu}>
                        <i className='fa-regular fa-bell icon'></i>
                    </HeaderIconButton>
                    <HeaderButton
                        id='profile'
                        aria-haspopup='true'
                        aria-expanded={isProfile ? 'true' : undefined}
                        onClick={handleProfileClick}
                        aria-label='profileSettings'
                    />
                    <Menu
                        id='profile-list'
                        anchorEl={anchorProfile}
                        open={isProfile}
                        onClose={handleProfileClose}>
                        <Paper sx={{ width: 1 }}>
                            <Box
                                sx={{
                                    background: theme.palette.gradient.main,
                                    px: 2,
                                    py: 2,
                                    display: 'flex',
                                    maxWidth: '300px',
                                }}>
                                <Avatar
                                    alt='account'
                                    src='https://i.pravatar.cc/150?img=3'
                                    sx={{ mr: 1, width: 24, height: 24 }}
                                />
                                <Typography
                                    sx={{
                                        overflow: 'hidden',
                                        textOverflow: 'ellipsis',
                                        whiteSpace: 'nowrap',
                                    }}
                                    variant='body1'
                                    fontWeight={700}
                                    color='white'>
                                    Admin
                                </Typography>
                            </Box>
                            {ProfileList.map((list, index) => {
                                return (
                                    <MenuItem
                                        component={Link}
                                        key={index}
                                        onClick={handleProfileClose}
                                        to={list.path}
                                        sx={{ py: 2 }}>
                                        <i
                                            className={`fa-regular ${list.icon} icon`}
                                            style={{ marginRight: 6 }}></i>
                                        <Typography>{list.name}</Typography>
                                    </MenuItem>
                                );
                            })}
                            <MenuItem
                                sx={{
                                    py: 2,
                                    backgroundColor: theme.palette.primary.light,
                                }}>
                                <i
                                    className={`fa-regular fa-right-from-line icon`}
                                    style={{ marginRight: 6 }}></i>
                                <Typography>Logout</Typography>
                            </MenuItem>
                        </Paper>
                    </Menu>
                    <HeaderIconButton aria-label='open drawer' onClick={handleCloseNavMenu}>
                        <i className='fa-regular fa-message-dots icon'></i>
                    </HeaderIconButton>
                </Box>
            </Toolbar>
        </AppBar>
    );
};
