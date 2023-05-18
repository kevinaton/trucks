import * as React from 'react'
import { Paper, useMediaQuery } from '@mui/material'
import Box from '@mui/material/Box'
import CssBaseline from '@mui/material/CssBaseline'
import './fontawesome/css/all.css'
import { RouterConfig } from './navigation/RouterConfig'
import { DrawerHeader } from './components/DTComponents'
import { sideMenuItems } from './common/data/menus'
import { Appbar, SideMenu } from './components'

const App = (props) => {
    const [anchorElNav, setAnchorElNav] = React.useState(null)
    const [drawerOpen, setDrawerOpen] = React.useState(true)
    const isSmall = useMediaQuery((theme) => theme.breakpoints.down("lg"))
    const isBig = useMediaQuery((theme) => theme.breakpoints.up("lg"))
    const [collapseOpen, setCollapseOpen] = React.useState(
        sideMenuItems.reduce((acc, menu) => {
            if (menu.submenu) {
                acc[menu.name] = false
            }
            return acc
        }, {})
    )

    const [currentPageName, setCurrentPageName] = React.useState("")

    // Checks screen if it is small
    React.useEffect(() => {
        if (isSmall) {
            setDrawerOpen(false)
        }

        if (isBig) {
            setDrawerOpen(true)
        }

    }, [isSmall, isBig])

    const handleCurrentPageName = (name) => {
        document.title = name
        setCurrentPageName(name)
    } 

    const handleOpenNavMenu = (event) => {
        setAnchorElNav(event.currentTarget)
    }
    
    const handleCloseNavMenu = () => {
        setAnchorElNav(null)
    }
    
    const handleDrawerOpen = () => {
        setDrawerOpen(true)
    }
    
    const handleDrawerClose = () => {
        setDrawerOpen(false)
        setCollapseOpen(false)
    }
    
    return (
        <Box sx={{ display:'flex' }}>
            <CssBaseline />
            {/* This is the appbar located at the top of the app. */}

            <Appbar
                drawerOpen={drawerOpen}
                handleDrawerClose={handleDrawerClose}
                handleDrawerOpen={handleDrawerOpen}
                handleOpenNavMenu={handleOpenNavMenu}
                anchorElNav={anchorElNav}
                handleCloseNavMenu={handleCloseNavMenu}
            />

            <SideMenu 
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
                component="main" 
                sx={{ flexGrow: 1, height: '100%', overflow: 'auto' }}>
                    <Paper
                        sx={{
                            backgroundColor: "#f1f5f8",
                            padding: 2,
                            height: "100vh",
                            overflow: "auto",
                            pb: "50px",
                        }}
                    >
                        <DrawerHeader />

                        {/* This is the route configuration */}
                        <RouterConfig handleCurrentPageName={handleCurrentPageName} />
                    </Paper>
                </Box>
        </Box>
    )
}

export default App