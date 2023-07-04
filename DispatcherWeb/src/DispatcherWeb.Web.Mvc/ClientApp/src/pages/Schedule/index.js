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
import TruckMap from './truck-map';
import ScheduleOrders from './schedule-orders';
import { isEmpty } from 'lodash';

const Schedule = props => {
    const pageName = 'Schedule';
    const [view, setView] = useState('all');
    const [isJob, setJob] = useState(false);
    const [title, setTitle] = useState('Add Job');
    const [editData, setEditData] = useState({});

    const [dataFilter, setDataFilter] = useState({
        officeId: null,
        date: moment().format('MM/DD/YYYY'),
        hideCompletedOrders: false,
        hideProgressBar: false,
        sorting: 'Note'
    });

    const { userProfileMenu } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu
    }));

    useEffect(() => {
        props.handleCurrentPageName(pageName);
    }, [props]);

    useEffect(() => {
        if (dataFilter.officeId === null && 
            !isEmpty(userProfileMenu) && !isEmpty(userProfileMenu.result)) {
            const { sessionOfficeId } = userProfileMenu.result;
            setDataFilter({
                ...dataFilter,
                officeId: sessionOfficeId,
            });
        }
    }, [userProfileMenu, dataFilter]);

    // Handle toggle button at the top right
    const handleView = (event, newView) => {
        if (newView !== null) {
            setView(newView);
        }
    };

    const handleFilterChange = (dataFilter) => setDataFilter(dataFilter);

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
                    <TruckMap dataFilter={dataFilter} />

                    {/* List of schedule orders */}
                    <ScheduleOrders dataFilter={dataFilter} />
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default Schedule;