import React, {
    useContext,
    useState,
    useEffect,
    useRef,
} from 'react';
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
    useMediaQuery,
    Link,
    LinearProgress,
} from '@mui/material';
import { grey } from '@mui/material/colors';
import { linearProgressClasses } from '@mui/material/LinearProgress';
import {
    Tablecell,
    VerticalLinearProgress,
} from '../../components/DTComponents';
import { theme } from '../../Theme';
import _, { isEmpty } from 'lodash';
import {
    renderTime,
    isToday,
    round,
} from '../../helpers/misc_helper';
import { getScheduleOrders } from '../../store/actions';
import ScheduleTruckAssignment from './schedule-truck-assignment';
import App from '../../config/appConfig';
import SyncRequestContext from '../../components/common/signalr/syncRequestContext';
import { useNavigate } from 'react-router-dom';

const ScheduleOrders = ({
    userAppConfiguration,
    dataFilter,
    trucks,
    orders,
    onSetOrders,
}) => {
    const prevDataFilterRef = useRef(dataFilter);
    const [isLoading, setLoading] = useState(false);
    const [isConnectedToSignalR, setIsConnectedToSignalR] =
        useState(false);

    const [actionAnchor, setActionAnchor] = useState(null);
    const actionOpen = Boolean(actionAnchor);
    const [isOrderOpen, setIsOrderOpen] = useState(false);
    const [isPrintOrderOpen, setIsPrintOrderOpen] =
        useState(false);
    const [hoveredRow, setHoveredRow] = useState(null);

    const [isJob, setJob] = useState(false);
    const [title, setTitle] = useState('Add Job');
    const [editData, setEditData] = useState({});

    const syncRequestConnection = useContext(
        SyncRequestContext
    );
    const dispatch = useDispatch();
    const { isLoadingScheduleOrders, scheduleOrders } =
        useSelector((state) => ({
            isLoadingScheduleOrders:
                state.SchedulingReducer
                    .isLoadingScheduleOrders,
            scheduleOrders:
                state.SchedulingReducer.scheduleOrders,
        }));

    const navigate = useNavigate();
    const isSmScreen = useMediaQuery((theme) =>
        theme.breakpoints.up('sm')
    );

    useEffect(() => {
        if (
            isLoading &&
            !isEmpty(scheduleOrders) &&
            !isEmpty(scheduleOrders.result)
        ) {
            const { items } = scheduleOrders.result;
            if (
                !isEmpty(items) &&
                (isEmpty(orders) ||
                    (!isEmpty(orders) &&
                        !_.isEqual(orders, items)))
            ) {
                onSetOrders(items);
                // setScheduleData(items);
                // setLoading(false);
            }
        }
    }, [isLoading, scheduleOrders, orders, onSetOrders]);

    useEffect(() => {
        // check if dataFilter has changed from its previous state
        if (
            prevDataFilterRef.current.officeId !==
                dataFilter.officeId ||
            prevDataFilterRef.current.date !==
                dataFilter.date ||
            prevDataFilterRef.current.hideCompletedOrders !==
                dataFilter.hideCompletedOrders ||
            prevDataFilterRef.current.hideProgressBar !==
                dataFilter.hideProgressBar ||
            prevDataFilterRef.current.sorting !==
                dataFilter.sorting
        ) {
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

    useEffect(() => {
        if (
            !isConnectedToSignalR &&
            syncRequestConnection !== null
        ) {
            syncRequestConnection.on(
                'syncRequest',
                (payload) => {
                    console.log(
                        'orderline syncRequest: ',
                        payload
                    );
                }
            );
            setIsConnectedToSignalR(true);
        }
    }, [isConnectedToSignalR, syncRequestConnection]);

    useEffect(() => {
        if (isLoading !== isLoadingScheduleOrders) {
            setLoading(isLoadingScheduleOrders);
        }
    }, [isLoading, isLoadingScheduleOrders]);

    const fetchData = () => {
        const { officeId, date } = dataFilter;
        if (officeId !== null && date !== null) {
            dispatch(getScheduleOrders(dataFilter));
        }
    };

    // Handle action on table rows
    const handleActionClick = (e) => {
        setActionAnchor(e.currentTarget);
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
    const handleEditJob = (e, data) => {
        e.preventDefault();
        setActionAnchor(null);
        setIsOrderOpen(false);
        setTitle('Edit Job');
        setEditData(data);
        setJob(true);
    };

    const getPriorityLevel = (data) => {
        const { priority } = data;
        if (priority === 1) {
            return (
                <i className='fa-solid fa-circle-arrow-up error-icon'></i>
            );
        } else if (priority === 2) {
            return (
                <i className='fa-regular fa-circle success-icon'></i>
            );
        }
        return (
            <i className='fa-solid fa-circle-arrow-down secondary-icon'></i>
        );
    };

    const renderProgress = (data) => {
        if (data.isCancelled) {
            return 'Cancel';
        }

        if (
            dataFilter.hideProgressBar ||
            !isToday(dataFilter.date)
        ) {
            return '';
        }

        let shouldRenderProgressBar = true;
        let shouldShowNumberOfLoads = false;

        let designationIsFreightOnly =
            App.Enums.Designations.freightOnly.includes(
                data.designation
            );
        let designationHasMaterial =
            App.Enums.Designations.hasMaterial.includes(
                data.designation
            );
        let amountPercent = 0;
        let amountOrdered = data.amountOrdered || 0;
        let amountLoaded = data.amountLoaded || 0;
        let amountDelivered = data.amountDelivered || 0;

        if (
            !designationHasMaterial &&
            !designationIsFreightOnly
        ) {
            // if the designation is anything else, do not show the progress bar.
            // only show the number of loads in the cell. Display the quantities based on the UOM on hover.
            shouldRenderProgressBar = false;
            shouldShowNumberOfLoads = true;
        }

        if (!amountOrdered) {
            // order quantity is not specified, then the % complete can’t be calculated.
            // show the number of loads in the column, but don’t show the progress bar.
            shouldRenderProgressBar = false;
            shouldShowNumberOfLoads = true;
        }

        if (designationIsFreightOnly) {
            switch ((data.freightUom || '').toLowerCase()) {
                case 'hour':
                case 'hours':
                    shouldRenderProgressBar = false;
                    shouldShowNumberOfLoads = true;
                    //amountLoaded = round(full.hoursOnDispatchesLoaded);
                    //amountDelivered = round(full.hoursOnDispatches);
                    break;
                default:
                    break;
            }
        }

        if (shouldRenderProgressBar) {
            amountPercent = round(
                (amountDelivered / amountOrdered) * 100
            );
        }

        if (isNaN(amountPercent) || amountPercent === null) {
            amountPercent = 0;
        }

        if (isNaN(amountLoaded) || amountLoaded === null) {
            amountLoaded = 0;
        }

        if (
            isNaN(amountDelivered) ||
            amountDelivered === null
        ) {
            amountDelivered = 0;
        }

        if (data.cargoCapacityRequiredError) {
            shouldRenderProgressBar = false;
            shouldShowNumberOfLoads = true;
            //return getCargoCapacityErrorIcon(full.cargoCapacityRequiredError);
        }

        return (
            <Box
                sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                }}>
                {shouldRenderProgressBar && (
                    <Box>
                        <VerticalLinearProgress
                            variant='determinate'
                            color='secondary'
                            value={amountPercent}
                            sx={{
                                [`& .${linearProgressClasses.bar}`]:
                                    {
                                        transform: `translateY(${-amountPercent}%)!important`,
                                    },
                            }}
                        />
                        <Typography variant='caption'>{`${amountPercent}%`}</Typography>
                    </Box>
                )}
            </Box>
        );
    };

    const handleOrderClick = (path, data) => {
        navigate(path, { state: data });
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
                    value={
                        <i className='fa-regular fa-circle'></i>
                    }
                />
                <Tablecell
                    label='Cash on delivery'
                    value='COD'
                    tableCellClasses
                />
                <Tablecell label='Note' value='' />
                <Tablecell
                    label='Customer'
                    value='Customer'
                />
                <Tablecell label='Job Number' value='Job #' />
                <Tablecell
                    label='Time on job'
                    value='Time on job'
                    // value={<i className='fa-regular fa-clock'></i>}
                />
                <Tablecell label='Load at' value='Load At' />
                <Tablecell
                    label='Deliver to'
                    value='Deliver To'
                />
                <Tablecell label='Item' value='Item' />
                <Tablecell label='Quantity' value='Qty' />
                <Tablecell
                    label='Required truck'
                    value={
                        <i className='fa-regular fa-truck'></i>
                    }
                />
                <Tablecell
                    label='Progress'
                    value='Progress'
                />
                <Tablecell label='Closed' value='Closed' />
                <Tablecell label='' value='' />
            </TableRow>
        </TableHead>
    );

    return (
        <TableContainer component={Box}>
            {isSmScreen && (
                <Table
                    stickyHeader
                    aria-label='schedule table'
                    size='small'>
                    {renderHeader()}

                    <TableBody>
                        {!isEmpty(orders) &&
                            orders.map((data, index) => {
                                return (
                                    <React.Fragment
                                        key={index}>
                                        <TableRow
                                            hover={true}
                                            onMouseEnter={() =>
                                                handleRowHover(
                                                    index
                                                )
                                            }
                                            onMouseLeave={() =>
                                                handleRowLeave
                                            }
                                            sx={{
                                                backgroundColor:
                                                    hoveredRow ===
                                                    index
                                                        ? theme
                                                              .palette
                                                              .action
                                                              .hover
                                                        : '#ffffff',
                                                '&.MuiTableRow-root:hover':
                                                    {
                                                        backgroundColor:
                                                            theme
                                                                .palette
                                                                .action
                                                                .hover,
                                                    },
                                            }}>
                                            <Tablecell
                                                label='priority'
                                                value={getPriorityLevel(
                                                    data
                                                )}
                                            />
                                            <Tablecell
                                                label='Cash on delivery'
                                                value={
                                                    <Checkbox
                                                        disabled
                                                        checked={
                                                            data.customerIsCod
                                                        }
                                                    />
                                                }
                                            />
                                            <Tablecell
                                                label='Notes'
                                                value={
                                                    <i
                                                        className={`${
                                                            data.note !==
                                                                null &&
                                                            data.note !==
                                                                ''
                                                                ? 'fa-solid'
                                                                : 'fa-regular'
                                                        } fa-notebook icon`}></i>
                                                }
                                            />
                                            <Tablecell
                                                label='Customer'
                                                value={
                                                    data.customerName
                                                }
                                            />
                                            <Tablecell
                                                label='Job number'
                                                value={
                                                    data.jobNumber
                                                }
                                            />
                                            <Tablecell
                                                label='Time on job'
                                                value={
                                                    <>
                                                        {renderTime(
                                                            data.time,
                                                            ''
                                                        )}{' '}
                                                        {data.isTimeStaggered ? (
                                                            <span
                                                                class='far fa-clock staggered-icon pull-right'
                                                                title='Staggered'></span>
                                                        ) : null}
                                                    </>
                                                }
                                            />
                                            <Tablecell
                                                label='Load at'
                                                value={
                                                    data.loadAtNamePlain
                                                }
                                            />
                                            <Tablecell
                                                label='Deliver to'
                                                value={
                                                    data.deliverToNamePlain
                                                }
                                            />
                                            <Tablecell
                                                label='Item'
                                                value={
                                                    data.item
                                                }
                                            />
                                            <Tablecell
                                                label='Quantity'
                                                value={
                                                    data.quantityFormatted
                                                }
                                            />
                                            <Tablecell
                                                label='Required trucks'
                                                value={
                                                    data.numberOfTrucks
                                                }
                                            />
                                            <Tablecell
                                                label='Progress'
                                                value={renderProgress(
                                                    data
                                                )}
                                            />
                                            <Tablecell
                                                label='Closed'
                                                value={
                                                    <Checkbox
                                                        disabled
                                                        checked={
                                                            data.isClosed
                                                        }
                                                    />
                                                }
                                            />
                                            <Tablecell
                                                label='Action'
                                                value={
                                                    <div>
                                                        <IconButton
                                                            sx={{
                                                                width: 25,
                                                                height: 25,
                                                            }}
                                                            onClick={
                                                                handleActionClick
                                                            }>
                                                            <i className='fa-regular fa-ellipsis-vertical'></i>
                                                        </IconButton>
                                                        <Menu
                                                            anchorEl={
                                                                actionAnchor
                                                            }
                                                            id='settings-menu'
                                                            open={
                                                                actionOpen
                                                            }
                                                            onClose={
                                                                handleActionClose
                                                            }>
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={(
                                                                        event
                                                                    ) =>
                                                                        handleEditJob(
                                                                            event,
                                                                            data
                                                                        )
                                                                    }>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Edit
                                                                                Job
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                </ListItemButton>
                                                                <ListItemButton
                                                                    component={
                                                                        Link
                                                                    }
                                                                    to='/job-summary'>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Job
                                                                                Summary
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                </ListItemButton>
                                                            </ListItem>

                                                            <ListItem
                                                                disablePadding>
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
                                                                in={
                                                                    isOrderOpen
                                                                }
                                                                onClick={() => {
                                                                    setIsOrderOpen(
                                                                        false
                                                                    );
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{
                                                                    backgroundColor:
                                                                        grey[100],
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

                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={() => {
                                                                        setIsPrintOrderOpen(
                                                                            !isPrintOrderOpen
                                                                        );
                                                                    }}>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Print
                                                                                Order
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
                                                                in={
                                                                    isPrintOrderOpen
                                                                }
                                                                onClick={() => {
                                                                    setIsPrintOrderOpen(
                                                                        false
                                                                    );
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{
                                                                    backgroundColor:
                                                                        grey[100],
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
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={
                                                                        handleActionClose
                                                                    }>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            Tickets
                                                                        </Typography>
                                                                    </ListItemText>
                                                                </ListItemButton>
                                                            </ListItem>
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={
                                                                        handleActionClose
                                                                    }>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            View
                                                                            Load
                                                                            History
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
                                            onMouseEnter={() =>
                                                handleRowHover(
                                                    index
                                                )
                                            }
                                            onMouseLeave={() =>
                                                handleRowLeave
                                            }
                                            sx={{
                                                backgroundColor:
                                                    hoveredRow ===
                                                    index
                                                        ? theme
                                                              .palette
                                                              .action
                                                              .hover
                                                        : '#ffffff',
                                                '&.MuiTableRow-root:hover':
                                                    {
                                                        backgroundColor:
                                                            theme
                                                                .palette
                                                                .action
                                                                .hover,
                                                    },
                                            }}>
                                            <Tablecell
                                                label='Trucks'
                                                colSpan={14}
                                                value={
                                                    <Box>
                                                        <Box
                                                            sx={{
                                                                display:
                                                                    'flex',
                                                                alignContent:
                                                                    'center',
                                                                mb: 1,
                                                            }}>
                                                            <Typography
                                                                variant='subtitle2'
                                                                sx={{
                                                                    mr: 1,
                                                                }}>
                                                                Trucks
                                                                assigned
                                                            </Typography>
                                                            <Typography
                                                                sx={{
                                                                    px: 1,
                                                                    w: '10px',
                                                                    h: '10px',
                                                                    textAlign:
                                                                        'center',
                                                                    backgroundColor:
                                                                        theme
                                                                            .palette
                                                                            .grey[200],
                                                                    borderRadius: 80,
                                                                }}
                                                                variant='subtitle2'>
                                                                {
                                                                    data
                                                                        .trucks
                                                                        .length
                                                                }
                                                            </Typography>
                                                        </Box>

                                                        <Grid
                                                            sx={{
                                                                backgroundColor:
                                                                    theme
                                                                        .palette
                                                                        .background
                                                                        .paper,
                                                                borderRadius: 1,
                                                                border: '1px solid #ebedf2',
                                                                pb: 1,
                                                                m: 0,
                                                            }}
                                                            container
                                                            rowSpacing={
                                                                1
                                                            }
                                                            columnSpacing={
                                                                1
                                                            }>
                                                            {data.trucks.map(
                                                                (
                                                                    truck,
                                                                    index
                                                                ) => {
                                                                    return (
                                                                        <Grid
                                                                            item
                                                                            key={
                                                                                index
                                                                            }>
                                                                            <Chip
                                                                                label={
                                                                                    truck.truckCode
                                                                                }
                                                                                onClick={() => {}}
                                                                                onDelete={() => {}}
                                                                                variant={
                                                                                    truck.variant
                                                                                }
                                                                                color={
                                                                                    truck.color
                                                                                }
                                                                                sx={{
                                                                                    borderRadius: 0,
                                                                                    fontSize: 10,
                                                                                    fontWeight: 500,
                                                                                    p: 0,
                                                                                }}
                                                                            />
                                                                        </Grid>
                                                                    );
                                                                }
                                                            )}

                                                            <Grid
                                                                item>
                                                                <Chip
                                                                    label='Test'
                                                                    onClick={() => {}}
                                                                    onDelete={() => {}}
                                                                    variant='success'
                                                                    color='success'
                                                                    sx={{
                                                                        borderRadius: 0,
                                                                        fontSize: 10,
                                                                        fontWeight: 500,
                                                                        p: 0,
                                                                    }}
                                                                />
                                                            </Grid>

                                                            <Grid
                                                                item>
                                                                {!isEmpty(
                                                                    trucks
                                                                ) && (
                                                                    <ScheduleTruckAssignment
                                                                        trucks={
                                                                            trucks
                                                                        }
                                                                        index={
                                                                            index
                                                                        }
                                                                        data={
                                                                            data
                                                                        }
                                                                    />
                                                                )}
                                                            </Grid>
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
            )}

            {!isSmScreen && (
                <Table sx={{ width: '100%' }}>
                    <TableBody sx={{ width: '100%' }}>
                        {!isEmpty(orders) &&
                            orders.map((data, index) => {
                                return (
                                    <React.Fragment
                                        key={index}>
                                        <TableRow
                                            onMouseEnter={() =>
                                                handleRowHover(
                                                    index
                                                )
                                            }
                                            onMouseLeave={() =>
                                                handleRowLeave
                                            }
                                            sx={{
                                                backgroundColor:
                                                    grey[100],
                                            }}>
                                            <Tablecell
                                                style={{
                                                    width: '20%',
                                                }}
                                                label='Customer'
                                                value='Customer'
                                            />
                                            <Tablecell
                                                label={
                                                    data.customerName
                                                }
                                                value={
                                                    data.customerName
                                                }
                                                style={{
                                                    fontWeight:
                                                        'bold',
                                                    fontSize: 14,
                                                }}
                                            />
                                            <Tablecell
                                                label='Action'
                                                value={
                                                    <div>
                                                        <IconButton
                                                            sx={{
                                                                width: 25,
                                                                height: 25,
                                                            }}
                                                            onClick={
                                                                handleActionClick
                                                            }>
                                                            <i className='fa-regular fa-ellipsis-vertical'></i>
                                                        </IconButton>
                                                        <Menu
                                                            anchorEl={
                                                                actionAnchor
                                                            }
                                                            id='settings-menu'
                                                            open={
                                                                actionOpen
                                                            }
                                                            onClose={
                                                                handleActionClose
                                                            }>
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={(
                                                                        event
                                                                    ) =>
                                                                        handleEditJob(
                                                                            event,
                                                                            data
                                                                        )
                                                                    }>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Edit
                                                                                Job
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                </ListItemButton>
                                                            </ListItem>
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    component={
                                                                        Link
                                                                    }
                                                                    to='/job-summary'>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Job
                                                                                Summary
                                                                            </Typography>
                                                                        }
                                                                    />
                                                                </ListItemButton>
                                                            </ListItem>
                                                            <ListItem
                                                                disablePadding>
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
                                                                in={
                                                                    isOrderOpen
                                                                }
                                                                onClick={() => {
                                                                    setIsOrderOpen(
                                                                        false
                                                                    );
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{
                                                                    backgroundColor:
                                                                        grey[100],
                                                                }}>
                                                                <List
                                                                    component='div'
                                                                    disablePadding>
                                                                    <ListItemButton
                                                                        onClick={() =>
                                                                            handleOrderClick(
                                                                                '/orders/details',
                                                                                data
                                                                            )
                                                                        }>
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
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={() => {
                                                                        setIsPrintOrderOpen(
                                                                            !isPrintOrderOpen
                                                                        );
                                                                    }}>
                                                                    <ListItemText
                                                                        primary={
                                                                            <Typography align='left'>
                                                                                Print
                                                                                Order
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
                                                                in={
                                                                    isPrintOrderOpen
                                                                }
                                                                onClick={() => {
                                                                    setIsPrintOrderOpen(
                                                                        false
                                                                    );
                                                                }}
                                                                timeout='auto'
                                                                unmountOnExit
                                                                sx={{
                                                                    backgroundColor:
                                                                        grey[100],
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
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={
                                                                        handleActionClose
                                                                    }>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            Tickets
                                                                        </Typography>
                                                                    </ListItemText>
                                                                </ListItemButton>
                                                            </ListItem>
                                                            <ListItem
                                                                disablePadding>
                                                                <ListItemButton
                                                                    onClick={
                                                                        handleActionClose
                                                                    }>
                                                                    <ListItemText>
                                                                        <Typography align='left'>
                                                                            View
                                                                            Load
                                                                            History
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
                                            style={{
                                                borderBottom: `1px solid ${grey[200]}`,
                                            }}>
                                            <Tablecell
                                                label='Priority'
                                                colSpan={2}
                                                value={
                                                    <Grid
                                                        container
                                                        rowSpacing={
                                                            1
                                                        }
                                                        columnSpacing={
                                                            1
                                                        }
                                                        sx={{
                                                            p: 0,
                                                        }}>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={
                                                                    <div>
                                                                        {`Priority: ${getPriorityLevel(
                                                                            data
                                                                        )}`}{' '}
                                                                        {data.priority ===
                                                                        'high' ? (
                                                                            <i className='fa-solid fa-circle-arrow-up error-icon'></i>
                                                                        ) : data.priority ===
                                                                          'medium' ? (
                                                                            <i className='fa-solid fa-circle-arrow-up warning-icon'></i>
                                                                        ) : (
                                                                            <i className='fa-solid fa-circle-arrow-up success-icon'></i>
                                                                        )}
                                                                    </div>
                                                                }
                                                            />
                                                        </Grid>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={`Job #: ${data.jobNumber}`}
                                                            />
                                                        </Grid>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={
                                                                    <div>
                                                                        {
                                                                            'COD: '
                                                                        }
                                                                        {
                                                                            <Checkbox
                                                                                checked={
                                                                                    data.customerIsCod
                                                                                }
                                                                            />
                                                                        }
                                                                    </div>
                                                                }
                                                            />
                                                        </Grid>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={
                                                                    <div>
                                                                        {
                                                                            'Notes: '
                                                                        }
                                                                        {
                                                                            <i
                                                                                className={`${
                                                                                    data.note !==
                                                                                        null &&
                                                                                    data.note !==
                                                                                        ''
                                                                                        ? 'fa-solid'
                                                                                        : 'fa-regular'
                                                                                } fa-notebook icon`}></i>
                                                                        }
                                                                    </div>
                                                                }
                                                            />
                                                        </Grid>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={`Time: ${
                                                                    data.time
                                                                } ${
                                                                    data.isTimeStaggered ? (
                                                                        <span
                                                                            class='far fa-clock staggered-icon pull-right'
                                                                            title='Staggered'></span>
                                                                    ) : null
                                                                }`}
                                                            />
                                                        </Grid>
                                                        <Grid
                                                            item>
                                                            <Chip
                                                                label={
                                                                    <div>
                                                                        {
                                                                            'Closed: '
                                                                        }
                                                                        {
                                                                            <Checkbox
                                                                                checked={
                                                                                    data.isClosed
                                                                                }
                                                                            />
                                                                        }
                                                                    </div>
                                                                }
                                                            />
                                                        </Grid>
                                                    </Grid>
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Load at'
                                                value='Load at'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.loadAtNamePlain
                                                }
                                                value={
                                                    data.loadAtNamePlain
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Deliver to'
                                                value='Deliver to'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.deliverToNamePlain
                                                }
                                                value={
                                                    data.deliverToNamePlain
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Item'
                                                value='Item'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.item
                                                }
                                                value={
                                                    data.item
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Quantity'
                                                value='Quantity'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.quantityFormatted
                                                }
                                                value={
                                                    data.quantityFormatted
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Truck Required'
                                                value='Truck Req.'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.numberOfTrucks
                                                }
                                                value={
                                                    data.numberOfTrucks
                                                }
                                            />
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Amount Progress'
                                                value='Amount'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            <Tablecell
                                                label={
                                                    data.amountProgress
                                                }
                                                value={
                                                    <Box
                                                        sx={{
                                                            display:
                                                                'flex',
                                                            alignItems:
                                                                'center',
                                                        }}>
                                                        <Box
                                                            sx={{
                                                                width: '100%',
                                                                mr: 1,
                                                            }}>
                                                            <LinearProgress
                                                                variant='determinate'
                                                                color='primary'
                                                                sx={{
                                                                    height: 10,
                                                                }}
                                                                value={
                                                                    20
                                                                }
                                                            />
                                                        </Box>
                                                        <Box
                                                            sx={{
                                                                minWidth: 30,
                                                            }}>
                                                            <Typography>{`${Math.round(
                                                                data.amountProgress
                                                            )}%`}</Typography>
                                                        </Box>
                                                    </Box>
                                                }
                                            />
                                        </TableRow>
                                        <TableRow
                                            sx={{
                                                borderBottom: `1px solid ${grey[200]}`,
                                            }}>
                                            <Tablecell
                                                label='Scheduled'
                                                value='Scheduled'
                                                style={{
                                                    borderRight: `1px solid ${grey[200]}`,
                                                }}
                                            />
                                            {data.schedProgress && (
                                                <Tablecell
                                                    label={
                                                        data.schedProgress
                                                    }
                                                    value={
                                                        <Box
                                                            sx={{
                                                                display:
                                                                    'flex',
                                                                alignItems:
                                                                    'center',
                                                            }}>
                                                            <Box
                                                                sx={{
                                                                    width: '100%',
                                                                    mr: 1,
                                                                }}>
                                                                <LinearProgress
                                                                    variant='determinate'
                                                                    color='primary'
                                                                    sx={{
                                                                        height: 10,
                                                                    }}
                                                                    value={
                                                                        50
                                                                    }
                                                                />
                                                            </Box>
                                                            <Box
                                                                sx={{
                                                                    minWidth: 30,
                                                                }}>
                                                                <Typography>{`${Math.round(
                                                                    data.schedProgress
                                                                )}%`}</Typography>
                                                            </Box>
                                                        </Box>
                                                    }
                                                />
                                            )}
                                        </TableRow>
                                        <TableRow>
                                            <Tablecell
                                                label='Trucks'
                                                colSpan={14}
                                                value={
                                                    <Box>
                                                        <Box
                                                            sx={{
                                                                display:
                                                                    'flex',
                                                                alignContent:
                                                                    'center',
                                                                mb: 1,
                                                            }}>
                                                            <Typography
                                                                variant='subtitle2'
                                                                sx={{
                                                                    mr: 1,
                                                                }}>
                                                                Trucks
                                                                assigned
                                                            </Typography>
                                                            <Typography
                                                                sx={{
                                                                    px: 1,
                                                                    w: '10px',
                                                                    h: '10px',
                                                                    textAlign:
                                                                        'center',
                                                                    backgroundColor:
                                                                        theme
                                                                            .palette
                                                                            .grey[200],
                                                                    borderRadius: 80,
                                                                }}
                                                                variant='subtitle2'>
                                                                {
                                                                    data
                                                                        .trucks
                                                                        .length
                                                                }
                                                            </Typography>
                                                        </Box>
                                                        <Grid
                                                            sx={{
                                                                backgroundColor:
                                                                    theme
                                                                        .palette
                                                                        .background
                                                                        .paper,
                                                                borderRadius: 1,
                                                                border: '1px solid #ebedf2',
                                                                pb: 1,
                                                                m: 0,
                                                            }}
                                                            container
                                                            rowSpacing={
                                                                1
                                                            }
                                                            columnSpacing={
                                                                1
                                                            }>
                                                            {data.trucks.map(
                                                                (
                                                                    truck,
                                                                    index
                                                                ) => {
                                                                    return (
                                                                        <Grid
                                                                            item
                                                                            key={
                                                                                index
                                                                            }>
                                                                            <Chip
                                                                                label={
                                                                                    truck.truckCode
                                                                                }
                                                                                onClick={() => {}}
                                                                                onDelete={() => {}}
                                                                                variant={
                                                                                    truck.variant
                                                                                }
                                                                                color={
                                                                                    truck.color
                                                                                }
                                                                                sx={{
                                                                                    borderRadius: 0,
                                                                                    fontSize: 10,
                                                                                    fontWeight: 500,
                                                                                    p: 0,
                                                                                }}
                                                                            />
                                                                        </Grid>
                                                                    );
                                                                }
                                                            )}

                                                            <Grid
                                                                item>
                                                                <Chip
                                                                    label='Test'
                                                                    onClick={() => {}}
                                                                    onDelete={() => {}}
                                                                    variant='success'
                                                                    color='success'
                                                                    sx={{
                                                                        borderRadius: 0,
                                                                        fontSize: 10,
                                                                        fontWeight: 500,
                                                                        p: 0,
                                                                    }}
                                                                />
                                                            </Grid>

                                                            <Grid
                                                                item>
                                                                {!isEmpty(
                                                                    trucks
                                                                ) && (
                                                                    <ScheduleTruckAssignment
                                                                        trucks={
                                                                            trucks
                                                                        }
                                                                        index={
                                                                            index
                                                                        }
                                                                        data={
                                                                            data
                                                                        }
                                                                    />
                                                                )}
                                                            </Grid>
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
            )}

            {isEmpty(orders) && (
                <Typography
                    align='center'
                    variant='subtitle2'
                    sx={{ p: 2 }}>
                    No data available in table
                </Typography>
            )}
        </TableContainer>
    );
};

export default ScheduleOrders;
