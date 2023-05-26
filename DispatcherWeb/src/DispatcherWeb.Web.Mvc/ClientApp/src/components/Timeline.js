import React, { useCallback } from 'react';
import moment from 'moment';
import {
    Eventcalendar,
    snackbar,
    Page,
    Popup,
    Input,
    Switch,
    Datepicker,
    Button,
    Select,
} from '@mobiscroll/react';
import '@mobiscroll/react/dist/css/mobiscroll.min.css';
import { CheckBox, ShoppingBasket, WarningOutlined } from '@material-ui/icons';
import { Box, useMediaQuery } from '@mui/material';
import data from '../common/data/data.json';
import { SelectField } from './DTComponents';

const { dispatches, trucks, offices, views, menu } = data;

const Timeline = () => {
    const [anchor, setAnchor] = React.useState(null);
    const [formOpen, setDispatchOpen] = React.useState(false);
    const [dataDispatches, setDispatches] = React.useState(dispatches);
    const [isEdit, setEdit] = React.useState(false);
    const [dispatchCustomer, setDispatchCustomer] = React.useState('');
    const [dispatchTime, setDispatchTime] = React.useState('');
    const [dispatchLoadAt, setDispatchLoadAt] = React.useState('');
    const [dispatchDeliverTo, setDispatchDeliverTo] = React.useState('');
    const [dispatchItem, setDispatchItem] = React.useState('');
    const [dispatchIsRunUntilStopped, setDispatchIsRunUntilStopped] = React.useState(false);
    const [isCompact, setCompact] = React.useState(false);
    const [isTimeRange, setIsTimeRange] = React.useState(false);
    const [tempStartTime, setTempStartTime] = React.useState(null);
    const [tempEndTime, setTempEndTime] = React.useState(null);
    const [workStart, setWorkStart] = React.useState('05:00');
    const [workEnd, setWorkEnd] = React.useState('19:00');
    const [rcAnchor, setRcAnchor] = React.useState(null);
    const [isRightClick, setIsRightClick] = React.useState(false);
    const [menuValue, setMenuValue] = React.useState(null);
    const [tempDispatch, setTempDispatch] = React.useState(null);
    const [isCopy, setIsCopy] = React.useState(false);
    const [newActivity, setNewActivity] = React.useState('');
    const [newDeliverTo, setNewDeliverTo] = React.useState('');
    const [newId, setNewId] = React.useState('');
    const [newItem, setNewItem] = React.useState('');
    const [newLoadAt, setLoadAt] = React.useState('');
    const [newResource, setNewResource] = React.useState(null);
    const [newRunUntilStopped, setNewRunUntilStopped] = React.useState(false);
    const [newTitle, setNewTitle] = React.useState('');
    const [menuList, setMenuList] = React.useState(menu);
    const [isDispatchTimes, setIsDispatchTimes] = React.useState(false);
    const [dispatchTimesDate, setDispatchTimesDate] = React.useState(new Date());
    const [dispatchTimesStartTime, setDispatchTimesStartTime] = React.useState(null);
    const [dispatchTimesDuration, setDispatchTimesDuration] = React.useState(null);
    const [isSetDisabled, setIsSetDisabled] = React.useState(true);
    const isSmall = useMediaQuery((theme) => theme.breakpoints.down('lg'));
    const [newRefDate, setNewRefDate] = React.useState(null);
    const [office, setOffice] = React.useState(offices[0]);
    const [viewOption, setViewOption] = React.useState(views[0]);

    React.useEffect(() => {
        // Automate updating of the dispatches start and end date
        const AutoDispatchUpdate = () => {
            // Checks data if it is in the current date. if not it will update the dispatch data
            const currentDate = new Date().toISOString().slice(0, 10); //Gets current date
            const checkDate = dataDispatches[0].start.slice(0, 10);

            if (currentDate !== checkDate) {
                dataDispatches.forEach((dispatch) => {
                    const startDate = dispatch.start.slice(0, 10);
                    const endDate = dispatch.end.slice(0, 10);

                    if (startDate !== currentDate || endDate !== currentDate) {
                        // Run the updateDispatch
                        const startTime = dispatch.start.slice(11); //Get the sart time
                        const endTime = dispatch.end.slice(11); //Get the end time

                        dispatch.start = `${currentDate}T${startTime}`; //updated the start date of the dispatch
                        dispatch.end = `${currentDate}T${endTime}`; //updated the end date of the dispatch

                        setDispatches([...dataDispatches, dispatch]);
                    }
                });
            }
        };

        AutoDispatchUpdate();
    }, [dataDispatches]);

    React.useEffect(() => {
        // handler for context menu
        const handleContextMenu = (e) => {
            // prevent right-click from appearing
            e.preventDefault();
        };

        document.addEventListener('contextmenu', handleContextMenu);

        return () => {
            document.removeEventListener('contextmenu', handleContextMenu);
        };
    });

    React.useEffect(() => {
        setNewRefDate(new Date()); //To refresh timeline if the isCompact changes value
    }, [isCompact]);

    const dispatchesByResource = React.useMemo(() => {
        const result = {};

        // Group the dispatches by resource truck number
        dataDispatches.forEach((dispatch) => {
            if (!result[dispatch.resource]) {
                result[dispatch.resource] = [];
            }
            result[dispatch.resource].push(dispatch);
        });

        // Sort each truck group by the end time in descending order and mark the last dispatch
        Object.keys(result).forEach((resource) => {
            const group = result[resource];
            group.sort((a, b) => moment(b.end) - moment(a.end));
            group.forEach((dispatch, index) => {
                dispatch.last = index === 0;
            });
        });

        return result;
    }, [dataDispatches]);

    // Timeline configuration
    const view = React.useMemo(
        (ev) => {
            return {
                timeline: {
                    type: 'day',
                    startDay: 1, // Monday
                    endDay: 6, // Friday
                    startTime: workStart,
                    endTime: workEnd,
                    timeCellStep: isSmall ? 120 : 60,
                    timeLabelStep: isSmall ? 120 : 60,
                    weekNumbers: false,
                    currentTimeIndicator: true,
                    row: 'variable',
                },
            };
        },
        [workStart, workEnd, isSmall]
    );

    // Load trucks
    const resourceTrucks = React.useMemo(() => {
        // Return the truck objects
        return trucks;
    }, []);

    // Custom Resource view
    const renderMyResource = useCallback((resource) => {
        return (
            <div className='md-work-week-cont'>
                <div className='md-work-week-name'>{resource.name}</div>
                <div className='md-work-week-title'>
                    {resource.truck}{' '}
                    {resource.erIcon === true ? (
                        <WarningOutlined
                            style={{ fill: '#DD4B39', width: '12px', height: '12px' }}
                        />
                    ) : (
                        ''
                    )}
                    {resource.erIcon === true ? (
                        <span className='warning-tooltip'>Unacknowledged dispatches</span>
                    ) : (
                        ''
                    )}
                </div>
                <img
                    className='md-work-week-avatar'
                    src={`/reactapp/assets/${resource.avatar}.png`}
                    alt='Avatar'
                />
            </div>
        );
    }, []);

    // Custom Dispatch events
    const renderScheduleEvent = useCallback(
        (data) => {
            const ev = data.original;
            const currentTime = new Date();
            let bgColor = '#E3E6E9',
                textColor = '#000000',
                borderColor = '',
                iconColor = '#3a7798';

            // Check if dispatch is loaded
            const isLoaded = data.original.status.toLowerCase().includes('loaded');

            // Manage dispatch color
            if (currentTime >= data.startDate && currentTime <= data.endDate) {
                // Present / In-progress dispatches
                // If dispatch is the last one and
                if (dispatchesByResource[data.resource][0].id === data.id && isLoaded === true) {
                    borderColor = '3px solid #feb811';
                }

                bgColor = '#317ab4';
                textColor = '#ffffff';
                iconColor = '#ffffff';
            } else if (currentTime > data.endDate) {
                // Past or done dispatches
                bgColor = '#d6f2ff';
            } else {
                // Future dispatches
                bgColor = '#E3E6E9';
            }

            if (isCompact === false) {
                return (
                    <div
                        className='event-background'
                        style={{ background: bgColor, border: borderColor }}>
                        <div className='event-title' style={{ color: textColor }}>
                            {ev.customer}
                        </div>
                        <div className='event-status' style={{ color: textColor }}>
                            {ev.status}
                        </div>
                        <div className='event-body' style={{ color: textColor }}>
                            {ev.loadat}
                        </div>
                        <div className='event-body' style={{ color: textColor }}>
                            {ev.deliverto}
                        </div>
                        <div className='event-details' style={{ color: textColor }}>
                            {ev.item ? (
                                <ShoppingBasket
                                    style={{ fill: iconColor, width: '12px', height: '12px' }}
                                />
                            ) : (
                                ''
                            )}
                            {ev.item}
                            {ev.runUntilStopped === true ? ', ' : ''}
                            {ev.runUntilStopped === true ? (
                                <CheckBox
                                    style={{
                                        fill: iconColor,
                                        width: '12px',
                                        height: '12px',
                                        margin: '0 0 0 4px',
                                    }}
                                />
                            ) : (
                                ''
                            )}
                            {ev.runUntilStopped === true ? 'Run until stopped' : ''}
                        </div>
                    </div>
                );
            } else {
                return (
                    <div
                        className='event-background'
                        style={{ background: bgColor, border: borderColor }}>
                        <div className='event-compact-title' style={{ color: textColor }}>
                            {ev.customer +
                                ', Load at:' +
                                ev.loadat +
                                ', Deliver  to:' +
                                ev.deliverto}
                        </div>
                        <div className='event-status' style={{ color: textColor }}>
                            {ev.status}
                        </div>
                    </div>
                );
            }
        },
        [isCompact, dispatchesByResource]
    );

    //  Create dispatch form
    const loadDispatchForm = React.useCallback((event) => {
        const dispStart = moment(event.start).format('h:mm a');
        const dispEnd = moment(event.end).format('h:mm a');
        setDispatchCustomer(event.customer);
        setDispatchTime(`${dispStart} to ${dispEnd}`);
        setDispatchLoadAt(event.loadat);
        setDispatchDeliverTo(event.deliverto);
        setDispatchItem(event.item);
        setDispatchIsRunUntilStopped(event.runUntilStopped);
    }, []);

    // Delete event
    const deleteEvent = React.useCallback(
        (event) => {
            setDispatches(dataDispatches.filter((item) => item.id !== event.id));
            setTimeout(() => {
                snackbar({
                    message: 'event deleted',
                    color: 'success',
                });
            });
        },
        [dataDispatches]
    );

    // Handle click of delete dispatch
    const onDeleteDispatch = React.useCallback(() => {
        deleteEvent(tempDispatch);
    }, [deleteEvent, tempDispatch]);

    const onEventDeleted = React.useCallback(
        (args) => {
            deleteEvent(args.event);
        },
        [deleteEvent]
    );

    // Events
    const onEventHoverIn = React.useCallback((args) => {
        setRcAnchor(args.domEvent.target);
        setAnchor(args.domEvent.target);
    }, []);

    const onEventHoverOut = React.useCallback(() => {}, []);

    const onEventClick = React.useCallback(
        (args) => {
            setDispatchOpen(true);
            setEdit(true);
            loadDispatchForm(args.event);
        },
        [loadDispatchForm]
    );

    const onClose = React.useCallback(
        (args) => {
            if (!isEdit) {
                setDispatches([...dataDispatches]);
            }
            setDispatchOpen(false);
        },
        [isEdit, dataDispatches]
    );

    const onEventCreated = React.useCallback(
        (args) => {
            setEdit(false);
            setDispatchOpen(true);
            loadDispatchForm(args.event);
        },
        [loadDispatchForm]
    );

    // Handle right click functionality
    const onEventRightClick = React.useCallback((ev) => {
        setTempDispatch(ev.event);
        setRcAnchor(ev.domEvent.target);

        // Adjust menu if to display end multiple dispatches
        if (ev.event.runUntilStopped === true) {
            setMenuList(menu);
            setIsRightClick(true);
        } else {
            const removedEndMultipleDispatches = menu.filter(
                (item) => item.value !== 'endMultipleDispatches'
            );
            setMenuList(removedEndMultipleDispatches);
            setIsRightClick(true);
        }
    }, []);

    const rightClickClose = React.useCallback(
        (ev) => {
            if (ev.value === 'cancel') {
                onDeleteDispatch();
                setTempDispatch('');
            }

            if (ev.value === 'copy') {
                setIsCopy(true);

                // set new ID
                let largestId = 0;
                dataDispatches.forEach((dispatch) => {
                    const idNum = parseInt(dispatch.id, 10);
                    if (idNum > largestId) {
                        largestId = idNum;
                    }
                });

                setNewId((largestId + 1).toString().padStart(3, '0'));
                setNewActivity(tempDispatch.status);
                setNewDeliverTo(tempDispatch.deliverto);
                setNewItem(tempDispatch.item);
                setLoadAt(tempDispatch.loadat);
                setNewResource(tempDispatch.resource);
                setNewRunUntilStopped(tempDispatch.runUntilStopped);
                setNewTitle(tempDispatch.customer);
            }

            if (ev.value === 'endMultipleDispatches') {
                tempDispatch.runUntilStopped = false;
                const index = dataDispatches.findIndex((x) => x.id === tempDispatch.id);
                const updateDispatch = [...dataDispatches];
                updateDispatch.splice(index, 1, tempDispatch);
                setDispatches(updateDispatch);
            }

            if (ev.value === 'changeDispatchtimes') {
                const st = moment(tempDispatch.start);
                const en = moment(tempDispatch.end);
                const duration = moment.duration(en.diff(st));
                const hrs = Math.floor(duration.asHours()).toString().padStart(2, '0');
                const min = duration.minutes().toString().padStart(2, '0');
                const timeDuration = `${hrs}:${min}`;

                // Checks if date is object or ISO
                if (moment.isDate(tempDispatch.start)) {
                    setDispatchTimesStartTime(st.format('YYYY-MM-DDTHH:mm:ss'));
                    setDispatchTimesDate(st.format('YYYY-MM-DDTHH:mm:ss'));
                } else {
                    setDispatchTimesStartTime(tempDispatch.start);
                    setDispatchTimesDate(tempDispatch.start);
                }

                setDispatchTimesDuration(timeDuration);
                setIsDispatchTimes(true);
            }

            setMenuValue('');
            setIsRightClick(false);
        },
        [onDeleteDispatch, dataDispatches, tempDispatch]
    );

    // Popup
    const responsivePopup = {
        mediumd: {
            display: 'anchored',
            width: 400,
            fullscreen: false,
            touchUi: false,
        },
    };

    // Add dispatch buttons
    const dispatchButtons = React.useMemo(() => {
        return [
            {
                text: 'Done',
                handler: () => {
                    setDispatchOpen(false);
                },
            },
        ];
    }, []);

    // Handle add/edit popup form
    const headerText = React.useMemo(() => (isEdit ? 'View Dispatch' : 'Add Dispatch'), [isEdit]);

    // Handle opening of working hour popup
    const timeRangeOpen = React.useCallback((event) => {
        setIsTimeRange(true);
    }, []);

    // Handle closing of working hour popup
    const timeRangeClose = React.useCallback(() => {
        const sTime = moment(tempStartTime).format('HH:mm').toString();
        const eTime = moment(tempEndTime).format('HH:mm').toString();

        setWorkStart(sTime !== 'Invalid date' ? sTime : '05:00');
        setWorkEnd(eTime !== 'Invalid date' ? eTime : '19:00');
        setIsTimeRange(false);
    }, [tempStartTime, tempEndTime]);

    // get input from working hours popup
    const workingHours = React.useCallback((val) => {
        let val0 = val.value[0];
        let val1 = val.value[1];

        setTempStartTime(val0);
        setTempEndTime(val1);
    }, []);

    // Custom Menu
    const customMenu = useCallback((data) => {
        const item = data.data;
        return (
            <div className='md-item-template'>
                {/* <span
                className={`mbsc-font-icon md-item-template-icon mbsc-icon-${item.icon}`}
                ></span> */}
                <span className='mbsc-font-icon md-item-template-icon'>
                    <i className={`fa-regular ${item.icon} secondary-icon fa-sm`}></i>
                </span>
                <div className='md-item-template-title'>
                    <span>{item.text}</span>
                </div>
            </div>
        );
    }, []);

    // handle copy time picker
    const copyDispatch = useCallback(
        (event, inst) => {
            const oldStartTime = new Date(tempDispatch.start);
            const endTime = new Date(tempDispatch.end);
            const startTime = new Date(event.value);

            // Set new duration
            // Calculate the hour duration
            const hrDuration = (endTime.getTime() - oldStartTime.getTime()) / 3600000;
            const newEndTime = new Date(startTime.getTime() + hrDuration * 3600000);

            const newCopiedDispatch = {
                id: newId,
                start: startTime,
                end: newEndTime.toISOString(),
                title: newTitle,
                status: newActivity,
                loadat: newLoadAt,
                deliverto: newDeliverTo,
                item: newItem,
                runUntilStopped: newRunUntilStopped,
                resource: newResource,
            };

            // Add new copied dispatch to the list of dispatches
            setDispatches([...dataDispatches, newCopiedDispatch]);

            setIsCopy(false);
        },
        [
            tempDispatch,
            newId,
            newActivity,
            newDeliverTo,
            newLoadAt,
            newItem,
            newResource,
            newRunUntilStopped,
            newTitle,
            dataDispatches,
        ]
    );

    // Handle cancel on Copy dispatch
    const copyCancel = () => {
        setNewId(null);
        setNewActivity('');
        setNewDeliverTo('');
        setNewItem('');
        setLoadAt('');
        setNewResource('');
        setNewRunUntilStopped('');
        setNewTitle('');
        setTempDispatch(null);
        setIsCopy(false);
    };

    const closeDispatchTimes = useCallback((ev) => {}, []);

    // change dispatch times button
    const changeDispatchTimesButtons = React.useMemo(() => {
        return [
            {
                text: 'Cancel',
                handler: () => {
                    setIsDispatchTimes(false);
                    setDispatchTimesDate('');
                    setDispatchTimesStartTime('');
                    setDispatchTimesDuration('');
                },
            },
            {
                text: 'Set',
                disabled: isSetDisabled,
                handler: () => {
                    const setDate = dispatchTimesDate.slice(0, 10); //Gets set date
                    const startTime = moment(
                        `${setDate}T${dispatchTimesStartTime.slice(11)}` // Current Start time
                    );
                    const duration = moment.duration(dispatchTimesDuration);
                    const endTime = moment(startTime).add(duration);

                    tempDispatch.start = startTime.format(); //Set new start
                    tempDispatch.end = endTime.format(); // Set new end
                    const index = dataDispatches.findIndex(
                        (x) => x.id === tempDispatch.id //Locate the current id
                    );
                    const updateDispatch = [...dataDispatches];
                    updateDispatch.splice(index, 1, tempDispatch);
                    setDispatches(updateDispatch);

                    setIsDispatchTimes(false);
                },
            },
        ];
    }, [
        dispatchTimesDuration,
        dispatchTimesStartTime,
        isSetDisabled,
        dispatchTimesDate,
        dataDispatches,
        tempDispatch,
    ]);

    const onEventUpdate = React.useCallback((event, int) => {
        console.log('onEventUpdate');
    }, []);

    // Handle the show compact function
    const handleCompact = useCallback((ev) => {
        setCompact(ev.target.checked);
    }, []);

    const handleOffice = (event, newVal) => {
        setOffice(newVal);
    };

    const handleViews = (event, newVal) => {
        setViewOption(newVal);
    };

    return (
        <Page>
            <div className='mbsc-grid'>
                <div className='mbsc-row mbsc-align-items-center'>
                    <div className='mbsc-col-12 mbsc-col-sm-6 mbsc-col-md-4'>
                        <Box sx={{ mt: 3 }}>
                            <SelectField
                                label={'Office'}
                                value={office}
                                onChange={handleOffice}
                                items={offices}
                            />
                        </Box>
                    </div>
                    <div className='mbsc-col-12 mbsc-col-sm-6 mbsc-col-md-4'>
                        <Box sx={{ mt: 3 }}>
                            <SelectField
                                label={'View'}
                                value={viewOption}
                                onChange={handleViews}
                                items={views}
                            />
                        </Box>
                    </div>
                    <div className='mbsc-col-12 mbsc-col-sm-12 mbsc-col-md-4 sets'>
                        {/* Compact */}
                        <Switch
                            className='sets-switch'
                            label='Show compact'
                            defaultChecked={false}
                            checked={isCompact}
                            position='start'
                            rtl={true}
                            onChange={handleCompact}
                        />

                        {/* working hours */}
                        <Button icon='clock' variant='flat' onClick={timeRangeOpen}></Button>

                        <Popup
                            className='time-range'
                            display='center'
                            isOpen={isTimeRange}
                            onClose={timeRangeClose}
                            touchUi={'auto'}
                            showOverlay={true}
                            closeOnOverlayClick={true}
                            contentPadding={false}>
                            <Datepicker
                                controls={['time']}
                                display='inline'
                                select='range'
                                touchUi={'auto'}
                                defaultValue={[workStart, workEnd]}
                                theme='ios'
                                onChange={workingHours}
                            />
                        </Popup>
                    </div>
                </div>
            </div>

            <Eventcalendar
                refDate={newRefDate}
                view={view}
                data={dataDispatches}
                resources={resourceTrucks}
                dragToResize={true}
                dragToMove={true}
                renderResource={renderMyResource}
                renderScheduleEvent={renderScheduleEvent}
                showEventTooltip={false}
                onEventClick={onEventClick}
                onEventHoverIn={onEventHoverIn}
                onEventHoverOut={onEventHoverOut}
                onEventCreated={onEventCreated}
                onEventDeleted={onEventDeleted}
                onEventRightClick={onEventRightClick}
                onEventUpdated={onEventUpdate}
                returnFormat='iso8601'
                height={'auto'}
            />

            {/* View dispatch form */}
            <Popup
                display='center'
                fullScreen={true}
                headerText={headerText}
                anchor={anchor}
                buttons={dispatchButtons}
                isOpen={formOpen}
                onClose={onClose}
                responsive={responsivePopup}>
                <div className='mbsc-form-group'>
                    <Input
                        label='Customer'
                        defaultValue={dispatchCustomer}
                        readOnly={true}
                        inputStyle='outline'
                    />
                    <Input
                        label='Time'
                        defaultValue={dispatchTime}
                        readOnly={true}
                        inputStyle='outline'
                    />
                    <Input
                        label='Load at'
                        defaultValue={dispatchLoadAt}
                        readOnly={true}
                        inputStyle='outline'
                    />
                    <Input
                        label='Deliver to'
                        defaultValue={dispatchDeliverTo}
                        readOnly={true}
                        inputStyle='outline'
                    />
                    <Input
                        label='Item'
                        defaultValue={dispatchItem}
                        readOnly={true}
                        inputStyle='outline'
                    />
                    <span
                        className='tooltip-icon'
                        style={{
                            display: dispatchIsRunUntilStopped === true ? 'block' : 'none',
                        }}>
                        <CheckBox className='form-checkbox ma' /> Run until stopped
                    </span>
                </div>
            </Popup>

            {/* rightclick menu popup */}
            <Select
                data={menuList}
                value={menuValue}
                display='anchored'
                isOpen={isRightClick}
                onClose={rightClickClose}
                showArrow={false}
                anchor={rcAnchor}
                touchUi={false}
                renderItem={customMenu}
                showInput={false}
            />

            {/* Copy function time picker */}
            <Datepicker
                controls={['time']}
                display='center'
                isOpen={isCopy}
                headerText='Set the start time'
                closeOnOverlayClick={true}
                stepMinute={5}
                touchUi={'auto'}
                theme='ios'
                onCancel={copyCancel}
                onChange={copyDispatch}
            />

            {/* Change dispatch times */}
            <Popup
                display='center'
                headerText='Change dispatch times'
                isOpen={isDispatchTimes}
                onClose={closeDispatchTimes}
                touchUi={'auto'}
                showOverlay={true}
                closeOnOverlayClick={true}
                buttons={changeDispatchTimesButtons}>
                <Datepicker
                    controls={['date']}
                    defaultValue={dispatchTimesDate.toLocaleString()}
                    touchUi={'auto'}
                    inputTyping={false}
                    label='Date'
                    inputStyle='outline'
                    returnFormat='iso8601'
                    onChange={(ev) => setDispatchTimesDate(ev.value)}
                />
                <Datepicker
                    controls={['time']}
                    defaultValue={dispatchTimesStartTime}
                    touchUi={'auto'}
                    inputTyping={false}
                    label='Start time'
                    inputStyle='outline'
                    returnFormat='iso8601'
                    onChange={(ev) => setDispatchTimesStartTime(ev.value)}
                />
                <Input
                    placeholder='HH:mm'
                    defaultValue={dispatchTimesDuration}
                    label='Estimated Dispatch Time'
                    inputStyle='outline'
                    onChange={(ev) => {
                        const timeRegex = /^([0-9]{1,2}|[02]?[0-9]|2[0-9]):[0-5][0-9]$/;
                        if (timeRegex.test(ev.target.value)) {
                            setIsSetDisabled(false);
                            setDispatchTimesDuration(ev.target.value);
                        } else setIsSetDisabled(true);
                    }}
                />
            </Popup>
        </Page>
    );
};

export default Timeline;
