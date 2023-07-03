import React, { useState, useEffect, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Checkbox,
    Chip,
    Collapse,
    Grid,
    IconButton,
    List,
    ListItem,
    ListItemText,
    ListItemButton,
    Menu,
    TableContainer,
    Table,
    TableHead,
    TableRow,
    TableBody,
    Typography,
} from '@mui/material';
import { grey } from '@mui/material/colors';
import { linearProgressClasses } from '@mui/material/LinearProgress';
import { Tablecell, VerticalLinearProgress } from '../../components/DTComponents';
import { theme } from '../../Theme';
import { isEmpty } from 'lodash';
import {
    getScheduleOrders
} from '../../store/actions';

// to remove later
import data from '../../common/data/data.json';
const { ScheduleData } = data;

const ScheduleOrders = ({
    dataFilter
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [scheduleData, setScheduleData] = useState(null);

    const [actionAnchor, setActionAnchor] = useState(null);
    const actionOpen = Boolean(actionAnchor);
    const [isOrderOpen, setIsOrderOpen] = useState(false);
    const [isPrintOrderOpen, setIsPrintOrderOpen] = useState(false);
    const [hoveredRow, setHoveredRow] = useState(null);
    const [isJob, setJob] = useState(false);
    const [title, setTitle] = useState('Add Job');
    const [editData, setEditData] = useState({});

    const dispatch = useDispatch();
    const {
        scheduleOrders
    } = useSelector((state) => ({
        scheduleOrders: state.SchedulingReducer.scheduleOrders
    }));

    // useEffect(() => {
    //     if (!isEmpty(dataFilter) && scheduleData === null && !isLoading) {
    //         const { officeId, date } = dataFilter;
    //         if (officeId !== null && date !== null) {
    //             setLoading(true);
    //             dispatch(getScheduleOrders(dataFilter));
    //         }
    //     }
    // }, [dispatch, isLoading, dataFilter, scheduleData]);

    useEffect(() => {
        if (isLoading && !isEmpty(scheduleOrders) && !isEmpty(scheduleOrders.result) && isEmpty(scheduleData)) {
            setLoading(false);
            const { items } = scheduleOrders.result;
            setScheduleData(items);
        }
    }, [isLoading, scheduleOrders, scheduleData]);

    useEffect(() => {
        // check if dataFilter has changed from its previous state
        if (
            prevDataFilterRef.current.officeId !== dataFilter.officeId ||
            prevDataFilterRef.current.date !== dataFilter.date || 
            prevDataFilterRef.current.hideCompletedOrders !== dataFilter.hideCompletedOrders ||
            prevDataFilterRef.current.hideProgressBar !== dataFilter.hideProgressBar ||
            prevDataFilterRef.current.sorting !== dataFilter.sorting
        ) {
            const fetchData = async () => {
                const { officeId, date } = dataFilter;
                if (officeId !== null && date !== null) {
                    setLoading(true);
                    dispatch(getScheduleOrders(dataFilter));
                }
            };

            fetchData();

            // update the previous dataFilter value
            prevDataFilterRef.current = dataFilter;
        }
    }, [dataFilter]);

    useEffect(() => {
        // cleanup logic
        return () => {
            // reset the previous dataFilter value
            prevDataFilterRef.current = null;
        };
    }, []);

    // Handle action on table rows
    const handleActionClick = (event) => {
        setActionAnchor(event.currentTarget);
    };
    const handleActionClose = () => {
        setActionAnchor(null);
        setIsOrderOpen(false);
    };

    // Handle row hover on table
    const handleRowHover = (index) => {
        setHoveredRow(index);
    };
    const handleRowLeave = () => {
        setHoveredRow(null);
    };

    // Handle edit jobs
    const handleEditJob = (event, data) => {
        setActionAnchor(null);
        setIsOrderOpen(false);
        setTitle('Edit Job');
        setEditData(data);
        setJob(true);
        console.log(event);
        console.log(data);
    };

    const renderHeader = () => (
        <TableHead>
            <TableRow
                sx={{
                    '& th': {
                        backgroundColor: grey[200],
                    },
                }}>
                <Tablecell
                    label='Priority'
                    value={<i className='fa-regular fa-circle'></i>}
                />
                <Tablecell
                    label='Cash on delivery'
                    value='COD'
                    tableCellClasses
                />
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
                <Tablecell label='Progress' value='Progress' />
                <Tablecell label='Closed' value='Closed' />
                <Tablecell label='' value='' />
            </TableRow>
        </TableHead>
    );

    return (
        <TableContainer component={Box}>
            <Table stickyHeader aria-label='schedule table' size='small'>
                {renderHeader()}

                <TableBody>
                    {ScheduleData.map((data, index) => {
                        return (
                            <React.Fragment key={index}>
                                <TableRow
                                    hover={true}
                                    onMouseEnter={() => handleRowHover(index)}
                                    onMouseLeave={() => handleRowLeave}
                                    sx={{
                                        backgroundColor:
                                            hoveredRow === index
                                                ? theme.palette.action.hover
                                                : '#ffffff',
                                        '&.MuiTableRow-root:hover': {
                                            backgroundColor: theme.palette.action.hover,
                                        },
                                    }}>
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
                                        value={
                                            <i className='fa-solid fa-notebook icon'></i>
                                        }
                                    />
                                    <Tablecell label='Customer' value={data.customer} />
                                    <Tablecell label='Job number' value={data.job} />
                                    <Tablecell label='Time on job' value={data.time} />
                                    <Tablecell label='Load at' value={data.load} />
                                    <Tablecell
                                        label='Deliver to'
                                        value={data.deliver}
                                    />
                                    <Tablecell label='Item' value={data.item} />
                                    <Tablecell label='Quantity' value={data.quantity} />
                                    <Tablecell
                                        label='Required trucks'
                                        value={data.required}
                                    />
                                    <Tablecell
                                        label='Progress'
                                        value={
                                            <Box
                                                sx={{
                                                    display: 'flex',
                                                    justifyContent: 'space-between',
                                                }}>
                                                <Box>
                                                    <VerticalLinearProgress
                                                        variant='determinate'
                                                        color='secondary'
                                                        value={data.amountProgress}
                                                        sx={{
                                                            [`& .${linearProgressClasses.bar}`]:
                                                                {
                                                                    transform: `translateY(${-data.amountProgress}%)!important`,
                                                                },
                                                        }}
                                                    />
                                                    <Typography variant='caption'>{`${data.amountProgress}%`}</Typography>
                                                </Box>
                                                <Box>
                                                    <VerticalLinearProgress
                                                        variant='determinate'
                                                        color='secondary'
                                                        value={data.schedProgress}
                                                        sx={{
                                                            [`& .${linearProgressClasses.bar}`]:
                                                                {
                                                                    transform: `translateY(${-data.schedProgress}%)!important`,
                                                                },
                                                        }}
                                                    />
                                                    <Typography variant='caption'>{`${data.schedProgress}%`}</Typography>
                                                </Box>
                                            </Box>
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
                                                    onClick={handleActionClick}>
                                                    <i className='fa-regular fa-ellipsis-vertical'></i>
                                                </IconButton>
                                                <Menu
                                                    anchorEl={actionAnchor}
                                                    id='settings-menu'
                                                    open={actionOpen}
                                                    onClose={handleActionClose}>
                                                    <ListItem disablePadding>
                                                        <ListItemButton
                                                            onClick={(event) =>
                                                                handleEditJob(
                                                                    event,
                                                                    data
                                                                )
                                                            }>
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
                                                                setIsOrderOpen(
                                                                    !isOrderOpen
                                                                );
                                                            }}>
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
                                                            setIsOrderOpen(false);
                                                        }}
                                                        timeout='auto'
                                                        unmountOnExit
                                                        sx={{
                                                            backgroundColor: grey[100],
                                                        }}>
                                                        <List
                                                            component='div'
                                                            disablePadding>
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
                                                                setIsPrintOrderOpen(
                                                                    !isPrintOrderOpen
                                                                );
                                                            }}>
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
                                                            setIsPrintOrderOpen(false);
                                                        }}
                                                        timeout='auto'
                                                        unmountOnExit
                                                        sx={{
                                                            backgroundColor: grey[100],
                                                        }}>
                                                        <List
                                                            component='div'
                                                            disablePadding>
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
                                                        <ListItemButton
                                                            onClick={handleActionClose}>
                                                            <ListItemText>
                                                                <Typography align='left'>
                                                                    Tickets
                                                                </Typography>
                                                            </ListItemText>
                                                        </ListItemButton>
                                                    </ListItem>
                                                    <ListItem disablePadding>
                                                        <ListItemButton
                                                            onClick={handleActionClose}>
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

                                <TableRow
                                    hover={true}
                                    onMouseEnter={() => handleRowHover(index)}
                                    onMouseLeave={() => handleRowLeave}
                                    sx={{
                                        backgroundColor:
                                            hoveredRow === index
                                                ? theme.palette.action.hover
                                                : '#ffffff',
                                        '&.MuiTableRow-root:hover': {
                                            backgroundColor: theme.palette.action.hover,
                                        },
                                    }}>
                                    <Tablecell
                                        label='Trucks'
                                        colSpan={14}
                                        value={
                                            <Box>
                                                <Box
                                                    sx={{
                                                        display: 'flex',
                                                        alignContent: 'center',
                                                        mb: 1,
                                                    }}>
                                                    <Typography
                                                        variant='subtitle2'
                                                        sx={{ mr: 1 }}>
                                                        Trucks assigned
                                                    </Typography>
                                                    <Typography
                                                        sx={{
                                                            px: 1,
                                                            w: '10px',
                                                            h: '10px',
                                                            textAlign: 'center',
                                                            backgroundColor:
                                                                theme.palette.grey[200],
                                                            borderRadius: 80,
                                                        }}
                                                        variant='subtitle2'>
                                                        {data.trucks.length}
                                                    </Typography>
                                                </Box>

                                                <Grid
                                                    sx={{
                                                        backgroundColor:
                                                            theme.palette.background
                                                                .paper,
                                                        borderRadius: 1,
                                                        border: '1px solid #ebedf2',
                                                        pb: 1,
                                                        m: 0,
                                                    }}
                                                    container
                                                    rowSpacing={1}
                                                    columnSpacing={1}>
                                                    {data.trucks.map((truck, index) => {
                                                        return (
                                                            <Grid item key={index}>
                                                                <Chip
                                                                    label={truck.name}
                                                                    onClick={() => {}}
                                                                    onDelete={() => {}}
                                                                    variant={
                                                                        truck.variant
                                                                    }
                                                                    color={truck.color}
                                                                    sx={{
                                                                        borderRadius: 0,
                                                                        fontSize: 10,
                                                                        fontWeight: 500,
                                                                        p: 0,
                                                                    }}
                                                                />
                                                            </Grid>
                                                        );
                                                    })}
                                                </Grid>
                                            </Box>
                                        }
                                    />
                                </TableRow>
                            </React.Fragment>
                        );
                    })}
                </TableBody>
            </Table>
        </TableContainer>
    );
};

export default ScheduleOrders;