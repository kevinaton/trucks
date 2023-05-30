import * as React from 'react';
import {
    Avatar,
    Button,
    IconButton,
    Paper,
    Typography,
    FormControl,
    Autocomplete,
    TextField,
    Tooltip,
    TableCell,
} from '@mui/material';
import { KeyboardArrowDown } from '@material-ui/icons';
import { styled } from '@mui/material/styles';
import MuiDrawer from '@mui/material/Drawer';
import MuiAppBar from '@mui/material/AppBar';
import '../fontawesome/css/all.css';

// Header icon button component
export const HeaderIconButton = (props) => {
    return (
        <IconButton
            {...props}
            sx={{
                width: { xs: 'auto', sm: '16%' },
                '&:hover': {
                    xs: { backgroundColor: 'transparent' },
                    sm: { backgroundColor: 'default' },
                },
                mr: { xs: 0, sm: 2 },
            }}
        />
    );
};

// Header button component
export const HeaderButton = (props) => {
    return (
        <Button {...props} sx={{ mr: 2, px: 4 }}>
            <Typography sx={{ fontWeight: 600, fontSize: 12 }}>User</Typography>
            <Avatar
                alt='account'
                src='https://i.pravatar.cc/150?img=3'
                sx={{ ml: 1, width: 24, height: 24 }}
            />
        </Button>
    );
};

// Main backdraft
export const BackDraft = (props) => {
    return <Paper {...props} sx={{ backgroundColor: '#f1f5f8', padding: 2, height: '100%' }} />;
};

// Drawer header
export const DrawerHeader = styled('div')(({ theme }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    '& .MuiDrawer-paper': { borderWidth: 0 },
    padding: theme.spacing(0, 1),
    // necessary for content to be below app bar
    ...theme.mixins.toolbar,
}));

// Width of the drawer
export const drawerWidth = 240; // The width of the side menu

// Customized drawer
export const Drawer = styled(MuiDrawer, {
    shouldForwardProp: (prop) => {
        if (window.innerWidth < 600) {
            return prop !== 'paper';
        }
        return prop !== 'open';
    },
})(({ theme, open }) => ({
    width: drawerWidth,
    zIndex: theme.drawer + 1,
    flexShrink: 0,
    boxSizing: 'border-box',
    ...(open && {
        ...openedMixin(theme),
        '& .MuiDrawer-paper': openedMixin(theme),
    }),
    ...(!open && {
        ...closedMixin(theme),
        '& .MuiDrawer-paper': closedMixin(theme),
    }),
}));

// Custom TableCell
export const Tablecell = ({ label, value }) => {
    return (
        <Tooltip title={label} enterNextDelay={2000}>
            <TableCell aria-label={label}>{value}</TableCell>
        </Tooltip>
    );
};

// Customized AppBar
export const AppBar = styled(MuiAppBar, {
    shouldForwardProp: (prop) => prop !== 'open',
})(({ theme, open }) => ({
    zIndex: theme.zIndex.drawer + 1,
    transition: theme.transitions.create(['width', 'margin'], {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
}));

// Opened mixin. this is used for drawer
export const openedMixin = (theme) => ({
    width: drawerWidth,
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.enteringScreen,
    }),
    overflowX: 'auto',
});

// Closed mixin. this is used for drawer
export const closedMixin = (theme) => ({
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'auto',
    width: `calc(${theme.spacing(7)} + 1px)`,
    [theme.breakpoints.up('sm')]: {
        width: `calc(${theme.spacing(8)} + 1px)`,
    },
});

export const SelectField = ({ label, value, onChange, items }) => {
    return (
        <FormControl sx={{ width: 1 }}>
            <Autocomplete
                id='selectComp'
                value={value}
                options={items}
                onChange={onChange}
                size='small'
                label={label}
                popupIcon={<KeyboardArrowDown />}
                renderInput={(params) => <TextField {...params} label={label} />}
            />
        </FormControl>
    );
};

// Main side menu
// export const SideMenu = ({
//     drawerOpen,
//     DrawerHeader,
//     collapseOpen,
//     isSmall,
//     setCollapseOpen,
//     handleDrawerOpen,
//     handleDrawerClose
// }) => {
//     const [menus, setMenus] = React.useState(sideMenuItems)

//     // Handles the collapse function of the sidemenu.
//     const handleCollapseOpen = (menu) => {
//         setCollapseOpen((prev) => {
//             const newOpenState = { ...prev }

//             Object.keys(prev).forEach((name) => {
//                 if (name !== menu.name && prev[name]) {
//                 newOpenState[name] = false
//                 }
//             })

//             newOpenState[menu.name] = !prev[menu.name]

//             return newOpenState
//         })
//     }

//     // Handles the selection state of the ListItemButton
//     const handleListItemButton = (value) => {
//         const newSideMenus = menus.map((menu) => {
//             if (menu.name === value) {
//                 return { ...menu, select: true }
//             } else if (menu.submenu) {
//                 const updatedSubMenu = menu.submenu.map((sub) => {
//                     if (sub.name === value) {
//                         return { ...sub, select: true }
//                     }
//                     return { ...sub, select: false }
//                 })
//                 return { ...menu, submenu: updatedSubMenu, select: false }
//             }
//             return { ...menu, select: false }
//         })

//         setMenus(newSideMenus)
//     }

//     const drawer = (
//         <div>
//             <DrawerHeader />
//             <Box sx={{ p: 1, textAlign: "center" }}>
//                 <Button
//                     variant="contained"
//                     sx={{ fontSize: "0.8rem", display: drawerOpen ? "inline" : "none" }}
//                     color="primary"
//                     fullWidth
//                     size="large"
//                     startIcon={
//                         <i
//                             className="fa-regular fa-plus"
//                             style={{ fontSize: "0.8rem" }}
//                         ></i>
//                     }
//                 >
//                     Add New
//                 </Button>
//                 <Button
//                     variant="contained"
//                     aria-label="add new"
//                     color="primary"
//                     sx={{
//                         display: drawerOpen ? "none" : "block",
//                         minWidth: "36px",
//                         p: 0,
//                         width: "100%",
//                         height: "100%",
//                         color: "#ffffff",
//                         backgroundColor: "#67A2D7",
//                     }}
//                 >
//                     <i className="fa-regular fa-plus" style={{ fontSize: "0.8rem" }}></i>
//                 </Button>
//             </Box>
//             <List sx={{ p: 0 }}>
//                 {menus.map((menu, index) => {
//                     const isSubMenuOpen = menu.submenu && collapseOpen[menu.name]
//                     return (
//                         <ListItem
//                             component={menu.path ? Link : "div"}
//                             to={menu.path ? menu.path : {}}
//                             key={menu.name}
//                             disablePadding
//                             sx={{
//                                 display: "block",
//                                 textDecoration: "none",
//                                 color: "#212121"
//                             }}
//                         >
//                         <Tooltip
//                             title={menu.name}
//                             placement="right"
//                             TransitionComponent={Fade}
//                             enterNextDelay={2000}
//                         >
//                             <ListItemButton
//                                 sx={{
//                                     minHeight: 48,
//                                     justifyContent: drawerOpen ? "initial" : "center",
//                                     px: 2.5,
//                                     ...(isSubMenuOpen && { bgcolor: "#f8f9fa" })
//                                 }}
//                                 selected={menu.select}
//                                 onClick={
//                                     menu.submenu
//                                     ? () => handleCollapseOpen(menu)
//                                     : (event) => handleListItemButton(event.target.textContent)
//                                 }
//                             >
//                                 <ListItemIcon
//                                     sx={{
//                                         minWidth: 0.2,
//                                         justifyContent: "left"
//                                     }}
//                                 >
//                                     <i
//                                         className={
//                                             menu.select
//                                                 ? `${menu.icon} secondary-icon active-icon`
//                                                 : `${menu.icon} secondary-icon`
//                                         }
//                                     ></i>
//                                 </ListItemIcon>
//                                 <ListItemText
//                                     level="body2"
//                                     primary={
//                                         <Typography
//                                             variant="body2"
//                                             style={{
//                                                 fontSize: 14,
//                                                 textOverflow: "ellipsis",
//                                                 overflow: "hidden",
//                                                 whiteSpace: "nowrap"
//                                             }}
//                                         >
//                                             {menu.name}
//                                         </Typography>
//                                     }
//                                     sx={{
//                                         opacity: drawerOpen ? 1 : 0,
//                                         ...(isSubMenuOpen && { color: "#546674" })
//                                     }}
//                                 />
//                             {menu.submenu &&
//                                 drawerOpen &&
//                                 (isSubMenuOpen ? (
//                                     <i className="fa-regular fa-chevron-down secondary-icon fa-sm"></i>
//                                 ) : (
//                                     <i className="fa-regular fa-chevron-right secondary-icon fa-sm"></i>
//                                 ))}
//                             </ListItemButton>
//                         </Tooltip>

//                         {menu.submenu && (
//                             <Collapse in={isSubMenuOpen} unmountOnExit>
//                                 <List>
//                                     {menu.submenu.map((sub) => {
//                                         return (
//                                             <ListItem
//                                                 component={Link}
//                                                 key={sub.name}
//                                                 to={sub.path}
//                                                 disablePadding
//                                                 sx={{
//                                                     display: "block",
//                                                     textDecoration: "none",
//                                                     color: "#212121"
//                                                 }}
//                                                 onClick={(event) =>
//                                                     handleListItemButton(event.target.textContent)
//                                                 }
//                                             >
//                                                 <Tooltip
//                                                     title={sub.name}
//                                                     placement="right"
//                                                     TransitionComponent={Fade}
//                                                     enterNextDelay={2000}
//                                                 >
//                                                     <ListItemButton
//                                                         sx={{ pl: drawerOpen ? 4 : 2 }}
//                                                         selected={sub.select}
//                                                     >
//                                                         <ListItemIcon
//                                                             sx={{
//                                                                 minWidth: 0.2,
//                                                                 justifyContent: "left"
//                                                             }}
//                                                         >
//                                                             <i
//                                                                 className={
//                                                                     sub.select
//                                                                         ? `${sub.icon} secondary-icon active-icon`
//                                                                         : `${sub.icon} secondary-icon`
//                                                                 }
//                                                             ></i>
//                                                         </ListItemIcon>

//                                                         <ListItemText
//                                                             primary={
//                                                                 <Typography
//                                                                     variant="body2"
//                                                                     style={{
//                                                                     display: drawerOpen ? "block" : "none",
//                                                                     fontSize: 14,
//                                                                     textOverflow: "ellipsis",
//                                                                     overflow: "hidden",
//                                                                     whiteSpace: "nowrap"
//                                                                     }}
//                                                                 >
//                                                                     {sub.name}
//                                                                 </Typography>
//                                                             }
//                                                         />
//                                                     </ListItemButton>
//                                                 </Tooltip>
//                                             </ListItem>
//                                         )
//                                     })}
//                                 </List>
//                             </Collapse>
//                         )}
//                         </ListItem>
//                     )
//                 })}
//             </List>
//         </div>
//     )

//     return (
//         <Box>
//             <Drawer
//                 anchor="left"
//                 variant="temporary"
//                 open={drawerOpen}
//                 onClose={handleDrawerOpen}
//                 ModalProps={{
//                     BackdropProps: {
//                         onClick: handleDrawerClose,
//                     }
//                 }}
//                 sx={{
//                     display: { xs: "block", sm: "none" }
//                 }}
//             >
//                 {drawer}
//             </Drawer>

//             <Drawer
//                 anchor="left"
//                 variant="permanent"
//                 open={drawerOpen}
//                 overflow="auto"
//                 sx={{ display: { xs: "none", sm: "block" } }}
//                 ModalProps={{}}
//             >
//                 {drawer}
//             </Drawer>
//         </Box>
//     )
// }

// export const Appbar = ({
//     drawerOpen,
//     handleDrawerClose,
//     handleDrawerOpen,
//     handleOpenNavMenu,
//     anchorElNav,
//     handleCloseNavMenu
// }) => {
//     return (
//         <AppBar position="fixed" open={drawerOpen} color="inherit" elevation={5}>
//             <Toolbar sx={{ p: 0 }} disableGutters>
//                 <Box sx={{ flexGrow: 1 }}>
//                     {drawerOpen ? (
//                         <Box
//                             sx={{
//                                 ml: 2,
//                                 display: "flex",
//                                 width: drawerWidth - 26,
//                                 justifyContent: "space-between",
//                                 alignItems: "center",
//                             }}
//                         >
//                             <img
//                                 alt=""
//                                 width="30%"
//                                 height="100%"
//                                 src="/reactapp/assets/dumptruckdispatcher-logo.png"
//                             />
//                             <IconButton onClick={handleDrawerClose} aria-label="close drawer">
//                                 <i className="fa-regular fa-bars icon"></i>
//                             </IconButton>
//                         </Box>
//                     ) : (
//                         <IconButton
//                             aria-label="open drawer"
//                             onClick={handleDrawerOpen}
//                             edge="start"
//                             sx={{
//                                 ml: 2,
//                                 ...(drawerOpen && { display: "none" }),
//                             }}
//                         >
//                             <i className="fa-regular fa-bars icon"></i>
//                         </IconButton>
//                     )}
//                 </Box>

//                 {/* Mobile view */}
//                 <Box sx={{ display: { xs: "flex", md: "none" }, padding: 0 }}>
//                     <HeaderIconButton
//                         aria-label="header-menu"
//                         aria-controls="menu-appbar"
//                         aria-haspopup="true"
//                         onClick={handleOpenNavMenu}
//                         color="inherit"
//                     >
//                         <i className="fa-regular fa-ellipsis-vertical icon"></i>
//                     </HeaderIconButton>
//                     <Menu
//                         id="menu-appbar"
//                         anchorEl={anchorElNav}
//                         anchorOrigin={{
//                         vertical: "bottom",
//                         horizontal: "right",
//                         }}
//                         keepMounted
//                         transformOrigin={{
//                         vertical: "top",
//                         horizontal: "left",
//                         }}
//                         open={Boolean(anchorElNav)}
//                         onClose={handleCloseNavMenu}
//                         elevation={5}
//                         sx={{
//                         display: { sm: "flex", md: "none" },
//                         padding: 0,
//                         }}
//                     >
//                         <MenuList
//                             className="header-menu"
//                             sx={{
//                                 display: "flex",
//                                 flexDirection: "row",
//                                 padding: 0,
//                                 alignContent: "center",
//                             }}
//                         >
//                         <MenuItem key="support">
//                             <HeaderIconButton p={0} aria-label="support">
//                             <i
//                                 className="fa-duotone fa-life-ring icon"
//                                 style={{
//                                 "--fa-primary-opacity": "0.3",
//                                 "--fa-secondary-opacity": "1",
//                                 }}
//                             ></i>
//                             </HeaderIconButton>
//                         </MenuItem>
//                         <MenuItem key="notification">
//                             <IconButton p={0} aria-label="open drawer">
//                             <i className="fa-regular fa-bell icon"></i>
//                             </IconButton>
//                         </MenuItem>
//                         <MenuItem key="user">
//                             <Button p={0}>
//                             <Avatar
//                                 alt="account"
//                                 src="https://i.pravatar.cc/150?img=3"
//                                 sx={{ mr: 1, width: 24, height: 24 }}
//                             />
//                             <Typography sx={{ fontWeight: 600, fontSize: 12 }}>
//                                 User
//                             </Typography>
//                             </Button>
//                         </MenuItem>
//                         <MenuItem key="message">
//                             <IconButton p={0} aria-label="open drawer">
//                             <i className="fa-regular fa-message icon"></i>
//                             </IconButton>
//                         </MenuItem>
//                         </MenuList>
//                     </Menu>
//                 </Box>

//                 {/* Desktop view */}
//                 <Box sx={{ display: { xs: "none", md: "flex" } }}>
//                     <HeaderIconButton aria-label="support">
//                         <i
//                         className="fa-duotone fa-life-ring icon"
//                         style={{
//                             "--fa-primary-opacity": "0.3",
//                             "--fa-secondary-opacity": "1",
//                         }}
//                         ></i>
//                     </HeaderIconButton>
//                     <HeaderIconButton
//                         aria-label="open drawer"
//                         onClick={handleCloseNavMenu}
//                     >
//                         <i className="fa-regular fa-bell icon"></i>
//                     </HeaderIconButton>
//                     <HeaderButton onClick={handleCloseNavMenu} />
//                     <HeaderIconButton
//                         aria-label="open drawer"
//                         onClick={handleCloseNavMenu}
//                     >
//                         <i className="fa-regular fa-message-dots icon"></i>
//                     </HeaderIconButton>
//                 </Box>
//             </Toolbar>
//         </AppBar>
//     )
// }
