import React, { useEffect } from 'react';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import {
    Autocomplete,
    Box,
    Paper,
    TextField,
    Typography,
    Checkbox,
    FormControlLabel,
    ToggleButtonGroup,
    ToggleButton,
    Chip,
    IconButton,
    Menu,
    MenuItem,
    Grid,
    TableContainer,
    Table,
    TableHead,
    TableRow,
    TableBody,
    ListItem,
    ListItemText,
    ListItemButton,
    Collapse,
    List
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { grey } from '@mui/material/colors';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import moment from 'moment';
import data from '../../common/data/data.json';
import { Tablecell } from '../../components/DTComponents';

const { offices, TruckCode, ScheduleData } = data;

const Schedule = (props) => {
    const pageName = 'Schedule';
    const [date, setDate] = React.useState(moment());
    const [view, setView] = React.useState('all');
    const [settingsAnchor, setSettingsAnchor] = React.useState(null);
    const settingsOpen = Boolean(settingsAnchor);
    const [actionAnchor, setActionAnchor] = React.useState(null);
    const actionOpen = Boolean(actionAnchor);
    const [isOrderOpen, setIsOrderOpen] = React.useState(false);
    const [isPrintOrderOpen, setIsPrintOrderOpen] = React.useState(false);
    
    useEffect(() => {
        props.handleCurrentPageName(pageName);
    }, [props]);

    // Handle toggle button at the top right
    const handleView = (event, newView) => {
        if (newView !== null) {
            setView(newView);
        }
    };

    // Handle click of settings located at the top right
    const handleSettingsClick = (event) => {
        setSettingsAnchor(event.currentTarget);
    };

    const handleSettingsClose = () => {
        setSettingsAnchor(null);
    };

    // Handle action on table rows
    const handleActionClick = (event) => {
        setActionAnchor(event.currentTarget);
    };

    const handleActionClose = () => {
        setActionAnchor(null);
        setIsOrderOpen(false);
    };

    return (
        <HelmetProvider>
            <div>
                <Helmet>
                    <meta charSet='utf-8' />
                    <title>{pageName}</title>
                    <meta name='description' content='Dumptruckdispatcher app' />
                    <meta content='' name='author' />
                    <meta property='og:title' content={pageName} />
                    <meta
                        property='og:image'
                        content='%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>

                <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant='h6' component='h2' sx={{ mb: 1 }}>
                        {pageName}
                    </Typography>
                    <ToggleButtonGroup
                        color='primary'
                        aria-label='View state'
                        exclusive
                        value={view}
                        onChange={handleView}
                    >
                        <ToggleButton value='all' aria-label='all view'>
                            All
                        </ToggleButton>
                        <ToggleButton value='bycategory' aria-label='By category'>
                            By Category
                        </ToggleButton>
                    </ToggleButtonGroup>
                </Box>
                <Paper>
                    {/* Filter settings */}
                    <Box
                        component='form'
                        sx={{
                            p: 3,
                            display: 'flex',
                            flexWrap: 'wrap',
                            gap: 2,
                            justifyContent: 'flex-start',
                        }}
                    >
                        <LocalizationProvider
                            dateAdapter={AdapterMoment}
                            adapterLocale='de'
                        >
                            <DatePicker
                                label='date'
                                value={date}
                                onChange={(newVal) => setDate(newVal)}
                                sx={{ flexShrink: 0 }}
                            />

                            <Autocomplete
                                id='office'
                                options={offices}
                                sx={{ flex: 1, flexShrink: 0 }}
                                renderInput={(params) => (
                                    <TextField {...params} label='Office' />
                                )}
                            />

                            <FormControlLabel
                                control={<Checkbox />}
                                label='Hide Completed Orders'
                                sx={{ flexShrink: 0, m: 0 }}
                            />

                            <FormControlLabel
                                control={<Checkbox />}
                                label='Hide Progress Bar'
                                sx={{ flexShrink: 1, m: 0 }}
                            />

                            <FormControlLabel
                                control={<Checkbox />}
                                label='Hide Schedule Progress'
                                sx={{ flexShrink: 1, m: 0 }}
                            />

                            <FormControlLabel
                                control={
                                    <IconButton
                                        sx={{ width: 25, height: 25 }}
                                        onClick={handleSettingsClick}
                                    >
                                        <i className='fa-regular fa-ellipsis-vertical'></i>
                                    </IconButton>
                                }
                                sx={{ flex: 1, m: 0, justifyContent: 'flex-end' }}
                            ></FormControlLabel>

                            <Menu
                                anchorEl={settingsAnchor}
                                id='settings-menu'
                                open={settingsOpen}
                                onClose={handleSettingsClose}
                                onClick={handleSettingsClose}
                            >
                                <MenuItem onClick={handleSettingsClose}>
                                    <i className='fa-regular fa-truck secondary-icon pr-2'></i>{' '}
                                    Add a lease hauler
                                </MenuItem>
                                <MenuItem onClick={handleSettingsClose}>
                                    <i className='fa-regular fa-check secondary-icon pr-2'></i>{' '}
                                    Mark all jobs complete
                                </MenuItem>
                                <MenuItem onClick={handleSettingsClose}>
                                    <i className='fa-regular fa-plus secondary-icon pr-2'></i> Add
                                    job
                                </MenuItem>
                                <MenuItem onClick={handleSettingsClose}>
                                    <i className='fa-regular fa-print secondary-icon pr-2'></i>
                                    Print schedule
                                </MenuItem>
                                <MenuItem onClick={handleSettingsClose}>
                                    <i className='fa-regular fa-print secondary-icon pr-2'></i>
                                    Print all orders
                                </MenuItem>
                            </Menu>
                        </LocalizationProvider>
                    </Box>

                    {/* Truck Map */}
                    <Box sx={{ p: 3 }}>
                        <Paper variant='outlined' sx={{ p: 1 }}>
                            <Grid container rowSpacing={1} columnSpacing={1}>
                                {TruckCode.map((truck) => {
                                    return (
                                        <Grid item key={truck.label}>
                                            <Chip
                                                label={truck.label}
                                                color={truck.color}
                                                onClick={() => {}}
                                                sx={{
                                                    borderRadius: 0,
                                                    fontSize: 18,
                                                    fontWeight: 600,
                                                    py: 3
                                                }}
                                            />
                                        </Grid>
                                    )
                                })}
                            </Grid>
                        </Paper>
                    </Box>
                    <TableContainer component={Box}>
                        <Table aria-label='schedule table' size='small'>
                            <TableHead>
                                <TableRow sx={{ backgroundColor: '#e6e6e6' }}>
                                    <Tablecell label='Priority' value='' />
                                    <Tablecell label='Cash on delivery' value='COD' />
                                    <Tablecell label='Note' value='' />
                                    <Tablecell label='Customer' value='Customer' />
                                    <Tablecell label='Job Number' value='Job #' />
                                    <Tablecell
                                        label='Time on job'
                                        value={<i className='fa-regular fa-clock'></i>}
                                    />
                                    <Tablecell label='Load at' value='Load At' />
                                    <Tablecell label='Deliver to' value='Deliver To' />
                                    <Tablecell label='Item' value='Item' />
                                    <Tablecell label='Quantity' value='Qty' />
                                    <Tablecell
                                        label='Required truck'
                                        value={<i className='fa-regular fa-truck'></i>}
                                    />
                                    <Tablecell
                                        label='Scheduled truck'
                                        value={<i className='fa-regular fa-truck-clock'></i>}
                                    />
                                    <Tablecell label='Trucks' value='Trucks' />
                                    <Tablecell label='Closed' value='Closed' />
                                    <Tablecell label='' value='' />
                                </TableRow>
                            </TableHead>

                            <TableBody>
                                {ScheduleData.map((data, index) => {
                                    return (
                                        <TableRow
                                            key={index}
                                            hover={true}
                                            sx={{
                                                '&.MuiTableRow-root:hover': {
                                                    backgroundColor: '#F3F7FB'
                                                }
                                            }}
                                        >
                                            <Tablecell
                                                label='priority'
                                                value={
                                                <i className='fa-solid fa-circle-arrow-up error-icon'></i>
                                                }
                                            />
                                            <Tablecell
                                                label='Cash on delivery'
                                                value={<Checkbox checked={data.checkbox} />}
                                            />
                                            <Tablecell
                                                label='Notes'
                                                value={<i className='fa-solid fa-notebook icon'></i>}
                                            />
                                            <Tablecell label='Customer' value={data.customer} />
                                            <Tablecell label='Job number' value={data.job} />
                                            <Tablecell label='Time on job' value={data.time} />
                                            <Tablecell label='Load at' value={data.load} />
                                            <Tablecell label='Deliver to' value={data.deliver} />
                                            <Tablecell label='Item' value={data.item} />
                                            <Tablecell label='Quantity' value={data.quantity} />
                                            <Tablecell
                                                label='Required trucks'
                                                value={data.required}
                                            />
                                            <Tablecell
                                                label='Scheduled trucks'
                                                value={data.scheduled}
                                            />
                                            <Tablecell
                                                label='Trucks'
                                                value={
                                                    <Grid
                                                        sx={{
                                                            backgroundColor: '#f8f9fa',
                                                            borderRadius: 1,
                                                            border: '1px solid #ebedf2',
                                                            pb: 1,
                                                            m: 0
                                                        }}
                                                        container
                                                        rowSpacing={1}
                                                        columnSpacing={1}
                                                    >
                                                        {data.trucks.map((truck, index) => {
                                                            return (
                                                                <Grid item key={index}>
                                                                    <Chip
                                                                        label={truck}
                                                                        onClick={() => {}}
                                                                        onDelete={() => {}}
                                                                        sx={{
                                                                            borderRadius: 0,
                                                                            fontSize: 10,
                                                                            fontWeight: 500,
                                                                            p: 0
                                                                        }}
                                                                    />
                                                                </Grid>
                                                            )
                                                        })}
                                                    </Grid>
                                                }
                                            />
                                            <Tablecell
                                                label='Closed'
                                                value={<Checkbox checked={data.closed} />}
                                            />
                                            <Tablecell
                                                label='Action'
                                                value={
                                                    <div>
                                                        <IconButton
                                                            sx={{ width: 25, height: 25 }}
                                                            onClick={handleActionClick}
                                                        >
                                                            <i className='fa-regular fa-ellipsis-vertical'></i>
                                                        </IconButton>
                                                        <Menu
                                                            anchorEl={actionAnchor}
                                                            id='settings-menu'
                                                            open={actionOpen}
                                                            onClose={handleActionClose}
                                                        >
                                                            <ListItem disablePadding>
                                                                <ListItemButton onClick={handleActionClose}>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Edit Job
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                </ListItemButton>
                                                            </ListItem>

                                                            <ListItem disablePadding>
                                                                <ListItemButton
                                                                    onClick={() => {
                                                                        setIsOrderOpen(!isOrderOpen)
                                                                    }}
                                                                >
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Order
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                    {isOrderOpen ? (
                                                                        <i className='fa-regular fa-chevron-down secondary-icon fa-sm'></i>
                                                                    ) : (
                                                                        <i className='fa-regular fa-chevron-right secondary-icon fa-sm'></i>
                                                                    )}
                                                                </ListItemButton>
                                                            </ListItem>

                                                            <Collapse
                                                                in={isOrderOpen}
                                                                onClick={() => {
                                                                    setIsOrderOpen(false)
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{ backgroundColor: grey[100] }}
                                                            >
                                                                <List component='div' disablePadding>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='View/Edit' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Mark Complete' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Cancel' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Copy' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Transfer' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Change date' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Delete' />
                                                                    </ListItemButton>
                                                                </List>
                                                            </Collapse>

                                                            <ListItem disablePadding>
                                                                <ListItemButton
                                                                    onClick={() => {
                                                                        setIsPrintOrderOpen(!isPrintOrderOpen)
                                                                    }}
                                                                >
                                                                <ListItemText
                                                                    primary={
                                                                        <Typography align='left'>
                                                                            Print Order
                                                                        </Typography>
                                                                    }
                                                                />
                                                                    {isPrintOrderOpen ? (
                                                                        <i className='fa-regular fa-chevron-down secondary-icon fa-sm'></i>
                                                                    ) : (
                                                                        <i className='fa-regular fa-chevron-right secondary-icon fa-sm'></i>
                                                                    )}
                                                                </ListItemButton>
                                                            </ListItem>

                                                            <Collapse
                                                                in={isPrintOrderOpen}
                                                                onClick={() => {
                                                                    setIsPrintOrderOpen(false)
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{ backgroundColor: grey[100] }}
                                                            >
                                                                <List component='div' disablePadding>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='No Prices' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Combined Prices' />
                                                                    </ListItemButton>
                                                                    <ListItemButton>
                                                                        <ListItemText primary='Separate Prices' />
                                                                    </ListItemButton>
                                                                </List>
                                                            </Collapse>
                                                            <ListItem disablePadding>
                                                                <ListItemButton onClick={handleActionClose}>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            Tickets
                                                                        </Typography>
                                                                    </ListItemText>
                                                                </ListItemButton>
                                                            </ListItem>
                                                            <ListItem disablePadding>
                                                                <ListItemButton onClick={handleActionClose}>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            View Load History
                                                                        </Typography>
                                                                    </ListItemText>
                                                                </ListItemButton>
                                                            </ListItem>
                                                        </Menu>
                                                    </div>
                                                }
                                            />
                                        </TableRow>
                                    )
                                })}
                            </TableBody>
                        </Table>
                    </TableContainer>
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default Schedule;