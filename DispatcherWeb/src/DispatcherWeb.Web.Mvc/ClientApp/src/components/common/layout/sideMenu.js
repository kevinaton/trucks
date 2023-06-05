import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Link } from 'react-router-dom';
import { isEmpty } from 'lodash';
import {
    Box,
    Button,
    Fade,
    List,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Popover,
    Tooltip,
    Typography,
} from '@mui/material';
import { Drawer } from '../../DTComponents';
import { sideMenuItems } from '../../../common/data/menus';
import { getMenuItems } from '../../../store/actions';
import { theme } from '../../../Theme';

export const SideMenu = ({
    isAuthenticated,
    currentPageName,
    drawerOpen,
    DrawerHeader,
    collapseOpen,
    isSmall,
    setCollapseOpen,
    handleDrawerOpen,
    handleDrawerClose,
}) => {
    const [menus, setMenus] = useState(sideMenuItems);
    const [sideMenus, setSideMenus] = useState([]);
    const [subAnchorEl, setSubAnchorEl] = React.useState(null);

    const dispatch = useDispatch();
    const { menuItems } = useSelector((state) => ({
        menuItems: state.LayoutReducer.menuItems,
    }));

    useEffect(() => {
        if (isAuthenticated) {
            if (isEmpty(menuItems)) {
                dispatch(getMenuItems());
            } else {
                const { result } = menuItems;
                if (!isEmpty(result) && !isEmpty(result.items)) {
                    setSideMenus(result.items);
                }
            }
        }
    }, [dispatch, menuItems, isAuthenticated]);

    // Handles the collapse function of the sidemenu.
    const handleCollapseOpen = (menu) => {
        setCollapseOpen((prev) => {
            const newOpenState = { ...prev };

            Object.keys(prev).forEach((name) => {
                if (name !== menu.displayName && prev[name]) {
                    newOpenState[name] = false;
                }
            });

            newOpenState[menu.displayName] = !prev[menu.displayName];

            return newOpenState;
        });
    };

    // Handles the selection state of the ListItemButton
    const handleListItemButton = (value) => {
        const newSideMenus = sideMenus.map((menu) => {
            if (menu.displayName === value) {
                return { ...menu, select: true };
            } else if (menu.items) {
                const updatedSubMenu = menu.items.map((sub) => {
                    if (sub.displayName === value) {
                        return { ...sub, select: currentPageName === menu.displayName };
                    }
                    return { ...sub, select: false };
                });
                return { ...menu, submenu: updatedSubMenu, select: false };
            }
            return { ...menu, select: false };
        });

        setMenus(newSideMenus);
    };

    // Handles opening of submenu
    const handlePopoverOpen = (event, menu) => {
        setSubAnchorEl(event.currentTarget);
        handleCollapseOpen(menu);
    };

    // Handles closing of submenu
    const handlePopoverClose = (menu) => {
        setSubAnchorEl(null);
        setCollapseOpen((prev) => {
            return {
                ...prev,
                [menu.name]: false,
            };
        });
    };

    const drawer = (
        <div>
            <DrawerHeader />
            <Box sx={{ p: 1, textAlign: 'center' }}>
                <Button
                    variant='contained'
                    sx={{ fontSize: '0.8rem', display: drawerOpen ? 'inline' : 'none' }}
                    color='primary'
                    fullWidth
                    size='large'
                    startIcon={
                        <i className='fa-regular fa-plus' style={{ fontSize: '0.8rem' }}></i>
                    }>
                    Add New
                </Button>
                <Button
                    variant='contained'
                    aria-label='add new'
                    color='primary'
                    sx={{
                        display: drawerOpen ? 'none' : 'block',
                        minWidth: '36px',
                        p: 0,
                        width: '100%',
                        height: '100%',
                        color: '#ffffff',
                        backgroundColor: '#67A2D7',
                    }}>
                    <i className='fa-regular fa-plus' style={{ fontSize: '0.8rem' }}></i>
                </Button>
            </Box>

            <List sx={{ p: 0 }}>
                {sideMenus.map((menu, index) => {
                    const isSubMenuOpen = menu.items.length > 0 && collapseOpen[menu.displayName];
                    const isMvc = menu.url && !menu.url.startsWith('/app/redir?') ? true : false;
                    return (
                        <ListItem
                            component={menu.url && !isMvc ? Link : 'div'}
                            to={menu.url ? menu.url : {}}
                            key={menu.displayName}
                            disablePadding
                            sx={{
                                display: 'block',
                                textDecoration: 'none',
                                color: theme.palette.text.primary,
                            }}>
                            <Tooltip
                                title={menu.displayName}
                                placement='right'
                                TransitionComponent={Fade}
                                enterNextDelay={2000}>
                                <ListItemButton
                                    sx={{
                                        minHeight: 48,
                                        justifyContent: drawerOpen ? 'initial' : 'center',
                                        px: 2.5,
                                        ...(isSubMenuOpen && {
                                            bgcolor: theme.palette.action.hover,
                                        }),
                                    }}
                                    selected={menu.displayName === currentPageName ? true : false}
                                    onMouseOver={
                                        menu.items.length > 0
                                            ? (event) => handlePopoverOpen(event, menu)
                                            : (event) => {
                                                  handleListItemButton(event.target.textContent);
                                              }
                                    }
                                    onClick={() => {
                                        if (isMvc)
                                            window.location.href = `${window.location.origin}/${menu.url}`;
                                    }}>
                                    <ListItemIcon
                                        sx={{
                                            minWidth: 0.2,
                                            justifyContent: 'left',
                                        }}>
                                        <i
                                            className={
                                                menu.select
                                                    ? `${menu.icon} secondary-icon active-icon`
                                                    : `${menu.icon} secondary-icon`
                                            }></i>
                                    </ListItemIcon>
                                    <ListItemText
                                        level='body2'
                                        primary={
                                            <Typography
                                                variant='body2'
                                                style={{
                                                    fontSize: 14,
                                                    textOverflow: 'ellipsis',
                                                    overflow: 'hidden',
                                                    whiteSpace: 'nowrap',
                                                }}>
                                                {menu.displayName}
                                            </Typography>
                                        }
                                        sx={{
                                            ...(isSubMenuOpen && { color: '#546674' }),
                                        }}
                                    />
                                    {menu.items.length > 0 &&
                                        drawerOpen &&
                                        (isSubMenuOpen ? (
                                            <i className='fa-regular fa-chevron-left secondary-icon fa-sm'></i>
                                        ) : (
                                            <i className='fa-regular fa-chevron-right secondary-icon fa-sm'></i>
                                        ))}
                                </ListItemButton>
                            </Tooltip>

                            {menu.items.length > 0 && (
                                <Popover
                                    open={Boolean(subAnchorEl) && isSubMenuOpen}
                                    anchorEl={subAnchorEl}
                                    onClose={handlePopoverClose}
                                    anchorOrigin={{
                                        vertical: 'bottom',
                                        horizontal: 'right',
                                    }}
                                    transformOrigin={{
                                        vertical: 'bottom',
                                        horizontal: 'left',
                                    }}
                                    PaperProps={{
                                        sx: {
                                            overflow: 'auto',
                                            mt: 1,
                                        },
                                    }}>
                                    <List onMouseLeave={handlePopoverClose}>
                                        {menu.items.map((sub) => {
                                            const isSubMvc = !sub.url.startsWith('/app/redir?')
                                                ? true
                                                : false;

                                            return (
                                                <ListItem
                                                    component={!isSubMvc ? Link : 'div'}
                                                    key={sub.displayName}
                                                    to={sub.url}
                                                    disablePadding
                                                    sx={{
                                                        display: 'block',
                                                        textDecoration: 'none',
                                                        color: theme.palette.text.primary,
                                                        minWidth: 140,
                                                    }}
                                                    onClick={(event) => {
                                                        if (isSubMvc)
                                                            window.location.href = `${window.location.origin}/${sub.url}`;

                                                        handleListItemButton(
                                                            event.target.textContent
                                                        );
                                                    }}>
                                                    <Tooltip
                                                        title={sub.displayName}
                                                        placement='right'
                                                        TransitionComponent={Fade}
                                                        enterNextDelay={2000}>
                                                        <ListItemButton
                                                            sx={{ pl: drawerOpen ? 4 : 2 }}
                                                            selected={
                                                                sub.displayName === currentPageName
                                                                    ? true
                                                                    : false
                                                            }>
                                                            <ListItemIcon
                                                                sx={{
                                                                    minWidth: 0.2,
                                                                    justifyContent: 'left',
                                                                }}>
                                                                <i
                                                                    className={
                                                                        sub.select
                                                                            ? `${sub.icon} secondary-icon active-icon`
                                                                            : `${sub.icon} secondary-icon`
                                                                    }></i>
                                                            </ListItemIcon>
                                                            <ListItemText
                                                                primary={
                                                                    <Typography
                                                                        variant='body2'
                                                                        style={{
                                                                            display: drawerOpen
                                                                                ? 'block'
                                                                                : 'none',
                                                                            fontSize: 14,
                                                                            textOverflow:
                                                                                'ellipsis',
                                                                            overflow: 'hidden',
                                                                            whiteSpace: 'nowrap',
                                                                        }}>
                                                                        {sub.displayName}
                                                                    </Typography>
                                                                }
                                                            />
                                                        </ListItemButton>
                                                    </Tooltip>
                                                </ListItem>
                                            );
                                        })}
                                    </List>
                                </Popover>
                            )}
                        </ListItem>
                    );
                })}
            </List>
        </div>
    );

    return (
        <Box>
            <Drawer
                anchor='left'
                variant='temporary'
                open={drawerOpen}
                onClose={handleDrawerOpen}
                ModalProps={{
                    BackdropProps: {
                        onClick: handleDrawerClose,
                    },
                }}
                sx={{
                    display: { xs: 'block', sm: 'none' },
                }}>
                {drawer}
            </Drawer>

            <Drawer
                anchor='left'
                variant='permanent'
                open={drawerOpen}
                overflow='auto'
                sx={{ display: { xs: 'none', sm: 'block' } }}
                ModalProps={{}}>
                {drawer}
            </Drawer>
        </Box>
    );
};
