import React, { useState, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import {
    Box,
    Paper,
    Typography,
    ToggleButtonGroup,
    ToggleButton
} from '@mui/material';
import moment from 'moment';
import AddEditJob from '../../components/common/modals/addEditJob';
import SchedulingDataFilter from './scheduling-data-filter';
import TruckBlock from './truck-block';
import ScheduleOrders from './schedule-orders';
import { isEmpty } from 'lodash';
import { appLocalStorage } from '../../utils';

const Schedule = props => {
    const pageName = 'Schedule';
    const [isPageNameSet, setPageNameSet] = useState(false);
    const [view, setView] = useState('all');
    const [isJob, setJob] = useState(false);
    const [title, setTitle] = useState('Add Job');
    const [editData, setEditData] = useState({});
    const [dataFilter, setDataFilter] = useState({
        date: moment().format('MM/DD/YYYY'),
        shift: null,
        officeId: null, 
        officeName: null,
        hideCompletedOrders: false,
        hideProgressBar: false,
        sorting: 'Note'
    });
    const [trucks, setTrucks] = useState([]);
    const [orders, setOrders] = useState([]);

    const { 
        userProfileMenu,
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu
    }));

    useEffect(() => {
        if (!isPageNameSet) {
            props.handleCurrentPageName(pageName);
            setPageNameSet(true);
        }
    }, [isPageNameSet, props]);

    useEffect(() => {
        if (dataFilter.officeId === null && 
            !isEmpty(userProfileMenu) && !isEmpty(userProfileMenu.result)) {
            const { sessionOfficeId, sessionOfficeName } = userProfileMenu.result;

            appLocalStorage.getItem('schedule_filter', (result) => {
                let { ...dfilter } = dataFilter;

                const filter = result || {};
                if (filter.date) {
                    dfilter.date = filter.date;
                }

                if (filter.officeId) {
                    dfilter.officeId = filter.officeId;
                } else {
                    dfilter.officeId = sessionOfficeId;
                }

                if (filter.officeName) {
                    dfilter.officeName = filter.officeName;
                } else {
                    dfilter.officeName = sessionOfficeName;
                }

                if (filter.hideCompletedOrders) {
                    dfilter.hideCompletedOrders = filter.hideCompletedOrders;
                }

                if (filter.hideProgressBar) {
                    dfilter.hideProgressBar = filter.hideProgressBar;
                }
    
                setDataFilter({
                    ...dataFilter,
                    officeId:  parseInt(dfilter.officeId),
                    officeName: dfilter.officeName,
                    date: dfilter.date,
                    hideCompletedOrders: dfilter.hideCompletedOrders,
                    hideProgressBar: dfilter.hideProgressBar,
                });
            });
        }
    }, [userProfileMenu, dataFilter]);

    const onSetTrucks = data => setTrucks(data);

    const onSetOrders = data => setOrders(data);

    // Handle toggle button at the top right
    const handleView = (event, newView) => {
        if (newView !== null) {
            setView(newView);
        }
    };

    const handleFilterChange = (dataFilter) => {
        setDataFilter(dataFilter);
        appLocalStorage.setItem('schedule_filter', dataFilter);
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

                {/* Modals */}
                <AddEditJob 
                    state={isJob} 
                    setJob={setJob} 
                    title={title} 
                    data={editData} 
                />

                <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant='h6' component='h2' sx={{ mb: 1 }}>
                        {pageName}
                    </Typography>
                    <ToggleButtonGroup
                        color='primary'
                        aria-label='View state'
                        exclusive
                        value={view}
                        onChange={handleView}>
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
                    <SchedulingDataFilter 
                        dataFilter={dataFilter} 
                        handleFilterChange={handleFilterChange}
                    />
                    
                    {/* List of trucks */}
                    <TruckBlock 
                        userAppConfiguration={props.userAppConfiguration}
                        dataFilter={dataFilter} 
                        trucks={trucks} 
                        orders={orders}
                        onSetTrucks={onSetTrucks} 
                        openModal={props.openModal}
                        closeModal={props.closeModal} 
                        openDialog={props.openDialog}
                    />

                    {/* List of schedule orders */}
                    <ScheduleOrders 
                        userAppConfiguration={props.userAppConfiguration}
                        dataFilter={dataFilter} 
                        trucks={trucks} 
                        orders={orders}
                        onSetOrders={onSetOrders}
                    />
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default Schedule;