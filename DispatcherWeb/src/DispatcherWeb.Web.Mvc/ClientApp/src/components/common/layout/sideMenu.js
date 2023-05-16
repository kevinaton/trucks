import React, { useEffect, useState } from 'react'
import { useSelector, useDispatch } from 'react-redux'
import { Link } from 'react-router-dom'
import { isEmpty } from 'lodash'
import {
    Box,
    Button,
    Collapse,
    Fade,
    List,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Tooltip,
    Typography,
} from '@mui/material'
import { Drawer } from '../../DTComponents'
import { sideMenuItems } from '../../../common/data/menus'
import { getMenuItems } from '../../../store/actions'

export const SideMenu = ({
    drawerOpen,
    DrawerHeader,
    collapseOpen,
    isSmall,
    setCollapseOpen,
    handleDrawerOpen,
    handleDrawerClose
}) => {
    const [menus, setMenus] = useState(sideMenuItems)
    const [sideMenus, setSideMenus] = useState([])

    const dispatch = useDispatch()
    const { menuItems } = useSelector(state => ({
        menuItems: state.LayoutReducer.menuItems
    }))
    
    useEffect(() => {
        if (isEmpty(menuItems)) {
            dispatch(getMenuItems())
        } else {
            console.log('menuItems: ', menuItems)
            const { result } = menuItems
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setSideMenus(result.items)
            }
        }
    }, [dispatch, menuItems])

    // Handles the collapse function of the sidemenu.
    const handleCollapseOpen = (menu) => {
        setCollapseOpen((prev) => {
            const newOpenState = { ...prev }

            Object.keys(prev).forEach((name) => {
                if (name !== menu.displayName && prev[name]) {
                newOpenState[name] = false
                }
            })

            newOpenState[menu.displayName] = !prev[menu.displayName]

            return newOpenState
        })
    }

    // Handles the selection state of the ListItemButton
    const handleListItemButton = (value) => {
        const newSideMenus = sideMenus.map((menu) => {
            if (menu.displayName === value) {
                return { ...menu, select: true }
            } else if (menu.items) {
                const updatedSubMenu = menu.items.map((sub) => {
                    if (sub.displayName === value) {
                        return { ...sub, select: true }
                    }
                    return { ...sub, select: false }
                })
                return { ...menu, submenu: updatedSubMenu, select: false }
            }
            return { ...menu, select: false }
        })

        setMenus(newSideMenus)
    }

    const drawer = (
        <div>
            <DrawerHeader />
            <Box sx={{ p: 1, textAlign: "center" }}>
                <Button
                    variant="contained"
                    sx={{ fontSize: "0.8rem", display: drawerOpen ? "inline" : "none" }}
                    color="primary"
                    fullWidth
                    size="large"
                    startIcon={
                        <i
                            className="fa-regular fa-plus"
                            style={{ fontSize: "0.8rem" }}
                        ></i>
                    }
                >
                    Add New
                </Button>
                <Button
                    variant="contained"
                    aria-label="add new"
                    color="primary"
                    sx={{
                        display: drawerOpen ? "none" : "block",
                        minWidth: "36px",
                        p: 0,
                        width: "100%",
                        height: "100%",
                        color: "#ffffff",
                        backgroundColor: "#67A2D7",
                    }}
                >
                    <i className="fa-regular fa-plus" style={{ fontSize: "0.8rem" }}></i>
                </Button>
            </Box>

            <List sx={{ p: 0 }}>
                {sideMenus.map((menu, index) => {
                    const isSubMenuOpen = menu.items.length > 0 && collapseOpen[menu.displayName]
                    return (
                        <ListItem
                            component={menu.url ? Link : "div"}
                            to={menu.url ? menu.url : {}}
                            key={menu.displayName}
                            disablePadding
                            sx={{
                                display: "block",
                                textDecoration: "none",
                                color: "#212121"
                            }}
                        >
                        <Tooltip
                            title={menu.displayName}
                            placement="right"
                            TransitionComponent={Fade}
                            enterNextDelay={2000}
                        >
                            <ListItemButton
                                sx={{
                                    minHeight: 48,
                                    justifyContent: drawerOpen ? "initial" : "center",
                                    px: 2.5,
                                    ...(isSubMenuOpen && { bgcolor: "#f8f9fa" })
                                }}
                                selected={menu.select}
                                onClick={
                                    menu.items.length > 0
                                    ? () => handleCollapseOpen(menu)
                                    : (event) => handleListItemButton(event.target.textContent)
                                }
                            >
                                <ListItemIcon
                                    sx={{
                                        minWidth: 0.2,
                                        justifyContent: "left"
                                    }}
                                >
                                    <i
                                        className={
                                            menu.select
                                                ? `${menu.icon} secondary-icon active-icon`
                                                : `${menu.icon} secondary-icon`
                                        }
                                    ></i>
                                </ListItemIcon>
                                <ListItemText
                                    level="body2"
                                    primary={
                                        <Typography
                                            variant="body2"
                                            style={{
                                                fontSize: 14,
                                                textOverflow: "ellipsis",
                                                overflow: "hidden",
                                                whiteSpace: "nowrap"
                                            }}
                                        >
                                            {menu.displayName}
                                        </Typography>
                                    }
                                    sx={{
                                        opacity: drawerOpen ? 1 : 0,
                                        ...(isSubMenuOpen && { color: "#546674" })
                                    }}
                                />
                            {menu.items.length > 0 &&
                                drawerOpen &&
                                (isSubMenuOpen ? (
                                    <i className="fa-regular fa-chevron-down secondary-icon fa-sm"></i>
                                ) : (
                                    <i className="fa-regular fa-chevron-right secondary-icon fa-sm"></i>
                                ))}
                            </ListItemButton>
                        </Tooltip>

                        {menu.items.length > 0 && (
                            <Collapse in={isSubMenuOpen} unmountOnExit>
                                <List>
                                    {menu.items.map((sub) => {
                                        return (
                                            <ListItem
                                                component={Link}
                                                key={sub.displayName}
                                                to={sub.url}
                                                disablePadding
                                                sx={{
                                                    display: "block",
                                                    textDecoration: "none",
                                                    color: "#212121"
                                                }}
                                                onClick={(event) =>
                                                    handleListItemButton(event.target.textContent)
                                                }
                                            >
                                                <Tooltip
                                                    title={sub.displayName}
                                                    placement="right"
                                                    TransitionComponent={Fade}
                                                    enterNextDelay={2000}
                                                >
                                                    <ListItemButton
                                                        sx={{ pl: drawerOpen ? 4 : 2 }}
                                                        selected={sub.select}
                                                    >
                                                        <ListItemIcon
                                                            sx={{
                                                                minWidth: 0.2,
                                                                justifyContent: "left"
                                                            }}
                                                        >
                                                            <i
                                                                className={
                                                                    sub.select
                                                                        ? `${sub.icon} secondary-icon active-icon`
                                                                        : `${sub.icon} secondary-icon`
                                                                }
                                                            ></i>
                                                        </ListItemIcon>
                                                        
                                                        <ListItemText
                                                            primary={
                                                                <Typography
                                                                    variant="body2"
                                                                    style={{
                                                                    display: drawerOpen ? "block" : "none",
                                                                    fontSize: 14,
                                                                    textOverflow: "ellipsis",
                                                                    overflow: "hidden",
                                                                    whiteSpace: "nowrap"
                                                                    }}
                                                                >
                                                                    {sub.displayName}
                                                                </Typography>
                                                            }
                                                        />
                                                    </ListItemButton>
                                                </Tooltip>
                                            </ListItem>
                                        )
                                    })}
                                </List>
                            </Collapse>
                        )}
                        </ListItem>
                    )
                })}
            </List>
        </div>
    )

    return (
        <Box>
            <Drawer
                anchor="left"
                variant="temporary"
                open={drawerOpen}
                onClose={handleDrawerOpen}
                ModalProps={{
                    BackdropProps: {
                        onClick: handleDrawerClose,
                    }
                }}
                sx={{
                    display: { xs: "block", sm: "none" }
                }}
            >
                {drawer}
            </Drawer>

            <Drawer
                anchor="left"
                variant="permanent"
                open={drawerOpen}
                overflow="auto"
                sx={{ display: { xs: "none", sm: "block" } }}
                ModalProps={{}}
            >
                {drawer}
            </Drawer>
        </Box>
    )
}