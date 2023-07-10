import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { 
    Autocomplete,
    Box,
    Checkbox,
    FormControlLabel,
    IconButton,
    Menu,
    MenuItem,
    TextField,
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import moment from 'moment';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import _, { isEmpty } from 'lodash';
import { getOffices } from '../../store/actions';

const SchedulingDataFilter = ({
    dataFilter,
    handleFilterChange
}) => {
    const [date, setDate] = useState(null);
    const [officeOptions, setOfficeOptions] = useState([]);
    const [officeId, setOfficeId] = useState(null);
    const [defaultSelection, setDefaultSelection] = useState(-1);

    const [isJob, setJob] = useState(false);
    const [title, setTitle] = useState('Add Job');

    const [settingsAnchor, setSettingsAnchor] = useState(null);
    const settingsOpen = Boolean(settingsAnchor);

    const dispatch = useDispatch();
    const { offices } = useSelector((state) => ({
        offices: state.OfficeReducer.offices
    }));

    useEffect(() => {
        dispatch(getOffices());
    }, [dispatch]);

    // set default filter
    useEffect(() => {
        if (!isEmpty(officeOptions) && !isEmpty(dataFilter)) {
            if (officeId === null && 
                date === null && 
                dataFilter.officeId !== null && dataFilter.date !== null && 
                dataFilter.officeId !== officeId && dataFilter.date !== date
            ) {
                setOfficeId(dataFilter.officeId);
                setDate(moment(dataFilter.date));
            }
        }
    }, [officeOptions, dataFilter, officeId, date]);

    useEffect(() => {
        if (!isEmpty(offices) && !isEmpty(offices.result)) {
            const { result } = offices;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setOfficeOptions(result.items);
            }
        }
    }, [offices]);

    useEffect(() => {
        if (officeId !== null && !isEmpty(officeOptions)) {
            const defaultSelectionIndex = _.findIndex(officeOptions, 
                (item) => parseInt(item.id) === officeId
            );
            setDefaultSelection(defaultSelectionIndex);
        }
    }, [officeId, officeOptions]);

    const handleOfficeFilterChange = (e, newValue) => {
        e.preventDefault();

        handleFilterChange({
            ...dataFilter,
            officeId: newValue
        });
    }

    const handleDateFilterChange = (newDate) => {
        handleFilterChange({
            ...dataFilter,
            date: moment(newDate).format('MM/DD/YYYY')
        });
    };

    const handleHideOrShowCompletedOrders = (e) => {
        e.preventDefault();

        handleFilterChange({
            ...dataFilter,
            hideCompletedOrders: e.target.checked
        });
    }

    // Handle click of settings located at the top right
    const handleSettingsClick = (e) => {
        setSettingsAnchor(e.currentTarget);
    };
    const handleSettingsClose = () => {
        setSettingsAnchor(null);
    };

    // Handle Add jobs
    const handleAddJob = (e) => {
        e.preventDefault();
        setSettingsAnchor(null);
        setTitle('Add Job');
        setJob(true);
    };

    return (
        <React.Fragment>
            { !isEmpty(dataFilter) && 
                <Box
                    component='form'
                    sx={{
                        p: 3,
                        display: 'flex',
                        flexWrap: 'wrap',
                        gap: 2,
                        justifyContent: 'flex-start',
                    }}>
                    <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale='de'>
                        <DatePicker
                            label='date'
                            value={date}
                            onChange={handleDateFilterChange}
                            sx={{ flexShrink: 0 }}
                        />

                        {!isEmpty(officeOptions) && defaultSelection !== -1 && 
                            <Autocomplete
                                id='office'
                                options={officeOptions} 
                                getOptionLabel={(option) => option.name} 
                                defaultValue={officeOptions[defaultSelection]}
                                sx={{ flex: 1, flexShrink: 0 }}
                                renderInput={(params) => <TextField {...params} label='Office' />} 
                                onChange={(e, value) => handleOfficeFilterChange(e, value.id)}
                            />
                        }

                        <FormControlLabel
                            control={
                                <Checkbox 
                                    checked={dataFilter.hideCompletedOrders}
                                    onChange={handleHideOrShowCompletedOrders}
                                />
                            }
                            label='Hide Completed Orders'
                            sx={{ flexShrink: 0, m: 0 }}
                        />

                        <FormControlLabel
                            control={<Checkbox />}
                            label='Hide Progress Bar'
                            sx={{ flexShrink: 1, m: 0 }}
                        />

                        {/* <FormControlLabel
                            control={<Checkbox />}
                            label='Hide Schedule Progress'
                            sx={{ flexShrink: 1, m: 0 }}
                        /> */}

                        <FormControlLabel
                            control={
                                <IconButton
                                    sx={{ width: 25, height: 25 }}
                                    onClick={handleSettingsClick}>
                                    <i className='fa-regular fa-ellipsis-vertical'></i>
                                </IconButton>
                            }
                            sx={{
                                flex: 1,
                                m: 0,
                                justifyContent: 'flex-end',
                            }}></FormControlLabel>

                        <Menu
                            anchorEl={settingsAnchor}
                            id='settings-menu'
                            open={settingsOpen}
                            onClose={handleSettingsClose}
                            onClick={handleSettingsClose}>
                            <MenuItem onClick={handleSettingsClose}>
                                <i className='fa-regular fa-truck secondary-icon pr-2'></i> Add
                                a lease hauler
                            </MenuItem>
                            <MenuItem onClick={handleSettingsClose}>
                                <i className='fa-regular fa-check secondary-icon pr-2'></i> Mark
                                all jobs complete
                            </MenuItem>
                            <MenuItem onClick={handleAddJob}>
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
            }
        </React.Fragment>
    )
}

export default SchedulingDataFilter;