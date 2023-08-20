import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import moment from 'moment';
import {
    Autocomplete,
    Box,
    Button,
    Checkbox,
    FormControl,
    FormControlLabel,
    IconButton,
    InputAdornment,
    InputLabel,
    OutlinedInput,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { DatePicker, LocalizationProvider, MobileTimePicker } from '@mui/x-date-pickers';
import { DemoContainer } from '@mui/x-date-pickers/internals/demo';
import _, { isEmpty } from 'lodash';
import AddOrEditCustomer from '../customers/addOrEditCustomer';
import AddOrEditLocation from '../locations/addOrEditLocation';
import AddOrEditService from '../services/addOrEditService';
import data from '../../common/data/data.json';
import { renderDate } from '../../helpers/misc_helper';
import { grey } from '@mui/material/colors';
import { theme } from '../../Theme';
import {
    getActiveCustomersSelectList, 
    getDesignationsSelectList, 
    getOrderForEdit, 
    getLocationsSelectList,
    getServicesWithTaxInfoSelectList,
    getUnitsOfMeasureSelectList
} from '../../store/actions';

const { Customers, offices, Designation, Addresses, Items, FreightUom } = data;

const AddOrEditJob = ({ 
    userAppConfiguration, 
    openModal,
    closeModal
}) => {
    const [isLoadingActiveCustomers, setIsLoadingActiveCustomers] = useState(false);
    const [activeCustomersOptions, setActiveCustomersOptions] = useState(null);
    const [shiftOptions, setShiftOptions] = useState(null);
    const [locationOptions, setLocationOptions] = useState(null);
    const [isLoadingDesignations, setIsLoadingDesignationsOpts] = useState(false);
    const [designationOptions, setDesignationOptions] = useState(null);
    const [isLoadingLoadAtLocations, setIsLoadingLoadAtLocations] = useState(false);
    const [loadAtOptions, setLoadAtOptions] = useState(null);
    const [deliverToOptions, setDeliverToOptions] = useState(null);
    const [isLoadingServices, setIsLoadingServices] = useState(false);
    const [serviceOptions, setServiceOptions] = useState(null);
    const [isLoadingUnitsOfMeasure, setIsLoadingUnitsOfMeasure] = useState(false);
    const [unitOfMeasureOptions, setUnitOfMeasureOptions] = useState(null);
    const [orderInfo, setOrderInfo] = useState(null);

    const [id, setId] = useState(null);
    const [orderId, setOrderId] = useState(null);
    const [orderLineId, setOrderLineId] = useState(null);
    const [ticketId, setTicketId] = useState(null);
    const [isMaterialPricePerUnitOverridden, setIsMaterialPricePerUnitOverridden] = useState(false);
    const [isFreightPricePerUnitOverridden, setIsFreightPricePerUnitOverridden] = useState(false);
    const [isMaterialPriceOverridden, setIsMaterialPriceOverridden] = useState(false);
    const [isFreightPriceOverridden, setIsFreightPriceOverridden] = useState(false);
    const [isTaxable, setIsTaxable] = useState(false);
    const [staggeredTimeKind, setStaggeredTimeKind] = useState('');
    const [quoteServiceId, setQuoteServiceId] = useState(null);
    const [focusFieldId, setFocusFieldId] = useState(null);
    const [defaultLoadAtLocationId, setDefaultLoadAtLocationId] = useState(null);
    const [defaultLoadAtLocationName, setDefaultLoadAtLocationName] = useState('');
    const [defaultServiceId, setDefaultServiceId] = useState(null);
    const [defaultServiceName, setDefaultServiceName] = useState('');
    const [defaultFreightUomId, setDefaultFreightUomId] = useState(null);
    const [defaultFreightUomName, setDefaultFreightUomName] = useState('');
    const [projectId, setProjectId] = useState(null);
    const [contactId, setContactId] = useState(null);
    const [materialCompanyOrderId, setMaterialCompanyOrderId] = useState(null);
    const [poNumber, setPoNumber] = useState('');
    const [spectrumNumber, setSpectrumNumber] = useState('');
    const [directions, setDirections] = useState('');
    const [deliveryDate, setDeliveryDate] = useState({
        value: null,
        required: true,
        error: false,
        errorText: ''
    });
    const [customerId, setCustomerId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [quoteId, setQuoteId] = useState(null);
    const [shift, setShift] = useState('');
    const [locationId, setLocationId] = useState({
        value: '',
        defaultValue: null,
        initialized: false
    });
    const [jobNumber, setJobNumber] = useState('');
    const [designation, setDesignation] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [loadAtId, setLoadAtId] = useState('');
    const [deliverToId, setDeliverToId] = useState('');
    const [serviceId, setServiceId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [freightUomId, setFreightUomId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [materialUomId, setMaterialUomId] = useState('');
    const [freightPricePerUnit, setFreightPricePerUnit] = useState('');
    const [materialPricePerUnit, setMaterialPricePerUnit] = useState('');
    const [productionPay, setProductionPay] = useState('');
    const [freightRateToPayDrivers, setFreightRateToPayDrivers] = useState(false);
    const [loadBased, setLoadBased] = useState(false);
    const [freightQuantity, setFreightQuantity] = useState('');
    const [materialQuantity, setMaterialQuantity] = useState('');
    const [freightPrice, setFreightPrice] = useState('');
    const [materialPrice, setMaterialPrice] = useState('');
    const [leaseHaulerRate, setLeaseHaulerRate] = useState('');
    const [salesTaxRate, setSalesTaxRate] = useState('');
    const [numberOfTrucks, setNumberOfTrucks] = useState('');
    const [isMultipleLoads, setIsMultipleLoads] = useState(false);
    const [timeOnJob, setTimeOnJob] = useState('');
    const [chargeTo, setChargeTo] = useState('');
    const [priority, setPriority] = useState('');
    const [note, setNote] = useState('');
    const [requiresCustomerNotification, setRequiresCustomerNotification] = useState(false);
    const [customerNotificationContactName, setCustomerNotificationContactName] = useState('');
    const [customerNotificationPhoneNumber, setCustomerNotificationPhoneNumber] = useState('');
    const [defaultFuelSurchargeCalculationName, setDefaultFuelSurchargeCalculationName] = useState('');
    const [fuelSurchargeCalculationId, setFuelSurchargeCalculationId] = useState('');
    const [defaultBaseFuelCost, setDefaultBaseFuelCost] = useState('');
    const [defaultCanChangeBaseFuelCost, setDefaultCanChangeBaseFuelCost] = useState(false);
    const [baseFuelCost, setBaseFuelCost] = useState('');
    const [autoGenerateTicketNumber, setAutoGenerateTicketNumber] = useState(false);
    const [ticketNumber, setTicketNumber] = useState('');
    
    const [item, setItem] = useState('');
    const [materialUom, setMaterialUom] = useState('');
    const [materialRate, setMaterialRate] = useState('');
    const [material, setMaterial] = useState('');
    const [subContractorRate, setSubContractorRate] = useState('');
    const [requestedNumberOfTrucks, setRequestedNumberOfTrucks] = useState('');
    const [isRunUntilStopped, setIsRunUntilStopped] = useState(false);
    const [isRequireNotification, setIsRequireNotification] = useState(false);

    const priorityTypes = ['High', 'Medium', 'Low'];
    const [isLock, setIsLock] = useState(false);

    const [newCustomerOption, setNewCustomerOption] = useState('');
    const [newLoadAtOption, setNewLoadAtOption] = useState('');
    const [newDeliverToOption, setNewDeliverToOption] = useState('');
    const [newServiceOption, setNewServiceOption] = useState('');
    
    const dispatch = useDispatch();
    const {
        isLoadingActiveCustomersOpts,
        activeCustomersSelectList,
        isLoadingDesignationsOpts,
        designationsSelectList,
        offices,
        isLoadingLocationsOpts,
        locationsSelectList,
        isLoadingServicesWithTaxInfoOpts,
        servicesWithTaxInfoSelectList,
        isLoadingUnitOfMeasuresOpts,
        unitsOfMeasureSelectList,
        orderForEdit
    } = useSelector((state) => ({
        isLoadingActiveCustomersOpts: state.CustomerReducer.isLoadingActiveCustomersOpts,
        activeCustomersSelectList: state.CustomerReducer.activeCustomersSelectList,
        isLoadingDesignationsOpts: state.DesignationReducer.isLoadingDesignationsOpts,
        designationsSelectList: state.DesignationReducer.designationsSelectList,
        offices: state.OfficeReducer.offices,
        isLoadingLocationsOpts: state.LocationReducer.isLoadingLocationsOpts,
        locationsSelectList: state.LocationReducer.locationsSelectList,
        isLoadingServicesWithTaxInfoOpts: state.ServiceReducer.isLoadingServicesWithTaxInfoOpts,
        servicesWithTaxInfoSelectList: state.ServiceReducer.servicesWithTaxInfoSelectList,
        isLoadingUnitOfMeasuresOpts: state.UnitOfMeasureReducer.isLoadingUnitOfMeasuresOpts,
        unitsOfMeasureSelectList: state.UnitOfMeasureReducer.unitsOfMeasureSelectList,
        orderForEdit: state.OrderReducer.orderForEdit
    }));

    useEffect(() => {
        dispatch(getActiveCustomersSelectList({
            maxResultCount: 1000,
            skipCount: 0
        }));
        dispatch(getDesignationsSelectList());
        dispatch(getOrderForEdit());
    }, []);

    useEffect(() => {
        if (!isLoadingActiveCustomersOpts && !isEmpty(activeCustomersSelectList)) {
            const { result } = activeCustomersSelectList;
            if (!isEmpty(result)) {
                setActiveCustomersOptions(result.items);
            }
        }
    }, [activeCustomersSelectList]);

    useEffect(() => {
        if (isLoadingActiveCustomersOpts !== isLoadingActiveCustomers) {
            setIsLoadingActiveCustomers(isLoadingActiveCustomersOpts);
        }
    }, [isLoadingActiveCustomersOpts]);

    useEffect(() => {
        if (!isEmpty(offices) && !isEmpty(offices.result)) {
            const { result } = offices;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setLocationOptions(result.items);
            }
        }
    }, [offices]);

    useEffect(() => {
        if (!isEmpty(locationOptions) && !isEmpty(orderInfo) && locationId.defaultValue === null) {
            const { officeId } = orderInfo;
            const defaultIndex = _.findIndex(locationOptions, { id: officeId.toString() });
            setLocationId({
                ...locationId,
                defaultValue: defaultIndex,
                initialized: true
            });
        }
    }, [locationOptions, orderInfo]);

    useEffect(() => {
        if (!isLoadingDesignationsOpts && !isEmpty(designationsSelectList)) {
            const { result } = designationsSelectList;
            if (!isEmpty(result)) {
                setDesignationOptions(result);
            }
        }
    }, [designationsSelectList]);

    useEffect(() => {
        if (isLoadingDesignationsOpts !== isLoadingDesignations) {
            setIsLoadingDesignationsOpts(isLoadingDesignationsOpts);
        }
    }, [isLoadingDesignationsOpts]);

    useEffect(() => {
        if (!isLoadingLocationsOpts && !isEmpty(locationsSelectList)) {
            const { result } = locationsSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setLoadAtOptions(result.items);
                setDeliverToOptions(result.items);
            }
        }
    }, [locationsSelectList]);

    useEffect(() => {
        if (isLoadingLocationsOpts !== isLoadingLoadAtLocations) {
            setIsLoadingLoadAtLocations(isLoadingLocationsOpts);
        }
    }, [isLoadingLocationsOpts]);

    useEffect(() => {
        if (!isLoadingServicesWithTaxInfoOpts && !isEmpty(servicesWithTaxInfoSelectList)) {
            const { result } = servicesWithTaxInfoSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setServiceOptions(result.items);
            }
        }
    }, [servicesWithTaxInfoSelectList]);

    useEffect(() => {
        if (isLoadingServicesWithTaxInfoOpts !== isLoadingServices) {
            setIsLoadingServices(isLoadingServicesWithTaxInfoOpts);
        }
    }, [isLoadingServicesWithTaxInfoOpts]);

    useEffect(() => {
        if (!isLoadingUnitOfMeasuresOpts && !isEmpty(unitsOfMeasureSelectList)) {
            const { result } = unitsOfMeasureSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setUnitOfMeasureOptions(result.items);
            }
        }
    }, [unitsOfMeasureSelectList]);

    useEffect(() => {
        if (isLoadingUnitOfMeasuresOpts !== isLoadingUnitsOfMeasure) {
            setIsLoadingUnitsOfMeasure(isLoadingUnitOfMeasuresOpts);
        }
    }, [isLoadingUnitOfMeasuresOpts]);

    useEffect(() => {
        if (orderInfo === null && !isEmpty(orderForEdit)) {
            console.log('orderForEdit: ', orderForEdit)
            setOrderInfo(orderForEdit);
            setId(orderForEdit.id);
        }
    }, [orderInfo, orderForEdit]);

    const handleDeliveryDateChange = (newDate) => {
        setDeliveryDate({
            ...deliveryDate,
            value: moment(newDate).format('MM/DD/YYYY'),
            error: false,
            errorText: ''
        });
    };

    const handleCustomerChange = (e, newValue) => {
        e.preventDefault();

        console.log('newValue: ', newValue)

        setCustomerId({
            ...customerId,
            value: newValue.id,
            error: false,
            errorText: ''
        });
    };

    const handleAddCustomerOption = (e) => {
        e.preventDefault();

        if (newCustomerOption.trim() !== '') {
            const newCustomerOptionValue = newCustomerOption.trim().toLowerCase();
            
            openModal(
                <AddOrEditCustomer 
                    closeModal={closeModal}
                />,
                500
            );
        }
    };

    const handleJobShiftChange = (e, newValue) => {
        e.preventDefault();
        setShift(newValue);
    };

    const handleLocationChange = (e, newValue) => {
        e.preventDefault();
        setLocationId({
            ...locationId,
            value: newValue,
        });
    };

    const handleJobNumberChange = (e) => {
        e.preventDefault();
        setJobNumber(e.target.value);
    };

    const handleDesignationChange = (e, newValue) => {
        e.preventDefault();
        
        if (newValue !== '' && newValue !== null) { 
            if (isEmpty(locationsSelectList)) {
                dispatch(getLocationsSelectList({
                    maxResultCount: 1000,
                    skipCount: 0,
                }));
            }

            if (isEmpty(servicesWithTaxInfoSelectList)) {
                dispatch(getServicesWithTaxInfoSelectList({
                    maxResultCount: 1000,
                    skipCount: 0,
                }));
            }

            if (isEmpty(unitsOfMeasureSelectList)) {
                dispatch(getUnitsOfMeasureSelectList({
                    maxResultCount: 1000,
                    skipCount: 0,
                }));
            }
        }

        setDesignation({
            ...designation,
            value: newValue,
            error: false,
            errorText: ''
        });
    };

    const handleLoadAtIdChange = (e, newValue) => {
        e.preventDefault();
    };

    const handleAddLoadAtOption = (e) => {
        e.preventDefault();

        if (newLoadAtOption.trim() !== '') {
            const newLoadAtOptionValue = newLoadAtOption.trim().toLowerCase();
            
            openModal(
                <AddOrEditLocation 
                    closeModal={closeModal}
                />,
                500
            );
        }
    };

    const handleDeliverToIdChange = (e, newValue) => {
        e.preventDefault();
    };

    const handleAddDeliverToOption = (e) => {
        e.preventDefault();

        if (newDeliverToOption.trim() !== '') {
            const newDeliverToOptionValue = newDeliverToOption.trim().toLowerCase();
            
            openModal(
                <AddOrEditLocation 
                    closeModal={closeModal}
                />,
                500
            );
        }
    };

    const handleServiceIdChange = (e, newValue) => {
        e.preventDefault();

        setServiceId({
            ...serviceId,
            value: newValue,
            error: false,
            errorText: ''
        });
    };

    const handleAddServiceOption = (e) => {
        e.preventDefault();
            
        if (newServiceOption.trim() !== '') {
            const newServiceOptionValue = newServiceOption.trim().toLowerCase();
            
            openModal(
                <AddOrEditService 
                    closeModal={closeModal} 
                />,
                500
            );
        }
    };

    const handleFreightUomIdChange = (e, newValue) => {
        e.preventDefault();
        
        setFreightUomId({
            ...freightUomId,
            value: newValue,
            error: false,
            errorText: ''
        });
    };

    const handleFreightLock = (e) => {
        setIsLock(!isLock);
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();

        const data = {

        };
    };

    console.log('userAppConfiguration: ', userAppConfiguration)

    return (
        <React.Fragment>
            { !isEmpty(userAppConfiguration) && !isEmpty(orderInfo) && 
                <React.Fragment>
                    <Box
                        display='flex'
                        justifyContent='space-between'
                        alignItems='center'
                        sx={{ p: 2 }} 
                    >
                        <Typography variant='h6' component='h2'>Add Job</Typography>
                        <Button 
                            onClick={closeModal} 
                            sx={{ minWidth: '32px' }}
                        >
                            <CloseIcon />
                        </Button>
                    </Box>
                    
                    <Box sx={{
                        p: 2,
                        width: '100%'
                    }}>
                        <LocalizationProvider
                            dateAdapter={AdapterMoment}
                            adapterLocale={moment.locale()}>
                                <Box 
                                    component='form' 
                                    autoComplete='true' 
                                    sx={{ w: 1 }}
                                >
                                    <Stack direction='column' spacing={2}>
                                        <Stack 
                                            direction={{ xs: 'column', sm: 'row' }} 
                                            spacing={2}
                                        >
                                            <DatePicker
                                                id='deliveryDate'
                                                label={
                                                    <>
                                                        Delivery date <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                    </>
                                                }
                                                views={['year', 'month', 'day']}
                                                value={deliveryDate.value !== null ? renderDate(deliveryDate.value) : moment()}
                                                emptyLabel=''
                                                onChange={handleDeliveryDateChange}
                                                error={deliveryDate.error}
                                                helperText={deliveryDate.error ? deliveryDate.errorText : ''}
                                                sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                required
                                            />

                                            { activeCustomersOptions && 
                                                <Autocomplete
                                                    id='customerId'
                                                    options={activeCustomersOptions} 
                                                    getOptionLabel={(option) => option.name} 
                                                    renderOption={(props, option) => (
                                                        <li {...props}>
                                                            {option.name}
                                                        </li>
                                                    )}
                                                    renderInput={(params) => (
                                                        <div>
                                                            <TextField 
                                                                {...params} 
                                                                label={
                                                                    <>
                                                                        Customer <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                                    </>
                                                                } 
                                                                variant='outlined' 
                                                                value={newCustomerOption} 
                                                                onChange={(e) => setNewCustomerOption(e.target.value)} 
                                                                emptyLabel=''
                                                                error={customerId.error} 
                                                                helperText={customerId.errorText} 
                                                                InputProps={{
                                                                    ...params.InputProps,
                                                                    endAdornment: (
                                                                        <React.Fragment>
                                                                            {params.InputProps.endAdornment}
                                                                            <IconButton
                                                                                onClick={(e) => handleAddCustomerOption(e)}
                                                                                disabled={newCustomerOption.trim() === ''}
                                                                            >
                                                                                <AddCircleOutlineIcon />
                                                                            </IconButton>
                                                                        </React.Fragment>
                                                                    )
                                                                }}
                                                            />
                                                        </div>
                                                    )} 
                                                    onChange={(e, value) => handleCustomerChange(e, value)} 
                                                    sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                />
                                            }
                                        </Stack>

                                        <Stack 
                                            direction={{ xs: 'column', sm: 'row' }} 
                                            spacing={2}
                                        >
                                            { userAppConfiguration.settings.useShifts && shiftOptions && 
                                                <Autocomplete 
                                                    id='jobShift' 
                                                    options={shiftOptions} 
                                                    getOptionLabel={(option) => option.name} 
                                                    renderInput={(params) => (
                                                        <TextField {...params} label='Shift' />
                                                    )}
                                                    onChange={(e, value) => handleJobShiftChange(e, value.id)}
                                                    sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                />
                                            }

                                            { locationId.initialized && userAppConfiguration.features.allowMultiOffice &&
                                                <Autocomplete
                                                    id='locationId' 
                                                    options={locationOptions} 
                                                    getOptionLabel={(option) => option.name} 
                                                    defaultValue={locationOptions[locationId.defaultValue]} 
                                                    renderInput={(params) => (
                                                        <TextField 
                                                            {...params} 
                                                            label='Office' 
                                                            disabled={orderInfo.isSingleOffice}
                                                        />
                                                    )}
                                                    onChange={(e, value) => handleLocationChange(e, value.id)}
                                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                />
                                            }
                                        </Stack>
                                        
                                        <Stack 
                                            direction={{ xs: 'column', sm: 'row' }} 
                                            spacing={2}
                                        >
                                            <TextField
                                                id='jobNumber'
                                                type='number'
                                                variant='outlined'
                                                label='Job Number' 
                                                value={jobNumber} 
                                                defaultValue={orderInfo.jobNumber} 
                                                onChange={handleJobNumberChange}
                                                sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                            />

                                            { designationOptions && 
                                                <Autocomplete
                                                    id='jobDesignation'
                                                    options={designationOptions} 
                                                    getOptionLabel={(option) => option.value}
                                                    renderInput={(params) => (
                                                        <TextField 
                                                            {...params} 
                                                            label={
                                                                <>
                                                                    Designation <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                                </>
                                                            } 
                                                            error={designation.error}
                                                            helperText={designation.error ? designation.errorText : ''}
                                                        />
                                                    )} 
                                                    onChange={(e, value) => handleDesignationChange(e, value.key)}
                                                    sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                />
                                            }
                                        </Stack>

                                        { designation.value && 
                                            <React.Fragment>
                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { !isLoadingLoadAtLocations && 
                                                        <Autocomplete
                                                            id='loadAtId'
                                                            options={loadAtOptions} 
                                                            getOptionLabel={(option) => option.name}
                                                            renderOption={(props, option) => (
                                                                <li {...props}>
                                                                    {option.name}
                                                                </li>
                                                            )}
                                                            renderInput={(params) => (
                                                                <div>
                                                                    <TextField 
                                                                        {...params}
                                                                        label='Load At' 
                                                                        variant='outlined' 
                                                                        value={newLoadAtOption} 
                                                                        onChange={(e) => setNewLoadAtOption(e.target.value)}
                                                                        emptyLabel='' 
                                                                        InputProps={{
                                                                            ...params.InputProps,
                                                                            endAdornment: (
                                                                                <React.Fragment>
                                                                                    {params.InputProps.endAdornment} 
                                                                                    <IconButton 
                                                                                        onClick={(e) => handleAddLoadAtOption(e)} 
                                                                                        disabled={newLoadAtOption.trim() === ''}
                                                                                    >
                                                                                        <AddCircleOutlineIcon />
                                                                                    </IconButton>
                                                                                </React.Fragment>
                                                                            )
                                                                        }}
                                                                    />
                                                                </div>
                                                            )}
                                                            onChange={(e, value) => handleLoadAtIdChange(e, value)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }

                                                    { !isLoadingLoadAtLocations && 
                                                        <Autocomplete
                                                            id='deliverToId'
                                                            options={deliverToOptions} 
                                                            getOptionLabel={(option) => option.name}
                                                            renderOption={(props, option) => (
                                                                <li {...props}>
                                                                    {option.name}
                                                                </li>
                                                            )}
                                                            renderInput={(params) => (
                                                                <div>
                                                                    <TextField
                                                                        {...params}
                                                                        label='Deliver To' 
                                                                        variant='outlined'
                                                                        value={newDeliverToOption} 
                                                                        onChange={(e) => setNewDeliverToOption(e.target.value)}
                                                                        emptyLabel=''
                                                                        InputProps={{
                                                                            ...params.InputProps,
                                                                            endAdornment: (
                                                                                <React.Fragment>
                                                                                    {params.InputProps.endAdornment}
                                                                                    <IconButton 
                                                                                        onClick={(e) => handleAddDeliverToOption(e)}
                                                                                        disabled={newDeliverToOption.trim() === ''}
                                                                                    >
                                                                                        <AddCircleOutlineIcon />
                                                                                    </IconButton>
                                                                                </React.Fragment>
                                                                            )
                                                                        }}
                                                                    />
                                                                </div>
                                                            )} 
                                                            onChange={(e, value) => handleDeliverToIdChange(e, value)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { !isLoadingServices && 
                                                        <Autocomplete
                                                            id='serviceId'
                                                            options={serviceOptions} 
                                                            getOptionLabel={(option) => option.name}
                                                            renderOption={(props, option) => (
                                                                <li {...props}>
                                                                    {option.name}
                                                                </li>
                                                            )}
                                                            renderInput={(params) => (
                                                                <div>
                                                                    <TextField
                                                                        {...params}
                                                                        label={
                                                                            <>
                                                                                Item <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                                            </>
                                                                        }
                                                                        variant='outlined' 
                                                                        value={newServiceOption} 
                                                                        onChange={(e) => setNewServiceOption(e.target.value)} 
                                                                        emptyLabel='' 
                                                                        error={serviceId.error}
                                                                        helperText={serviceId.error ? serviceId.errorText : ''}
                                                                        InputProps={{
                                                                            ...params.InputProps,
                                                                            endAdornment: (
                                                                                <React.Fragment>
                                                                                    {params.InputProps.endAdornment}
                                                                                    <IconButton
                                                                                        onClick={(e) => handleAddServiceOption(e)} 
                                                                                        disabled={newServiceOption.trim() === ''}
                                                                                    >
                                                                                        <AddCircleOutlineIcon />
                                                                                    </IconButton>
                                                                                </React.Fragment>
                                                                            )
                                                                        }}
                                                                    />
                                                                </div>
                                                            )} 
                                                            onChange={(e, value) => handleServiceIdChange(e, value)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { !isLoadingUnitsOfMeasure && 
                                                        <Autocomplete
                                                            id='freightUomId' 
                                                            options={unitOfMeasureOptions} 
                                                            getOptionLabel={(option) => option.name}
                                                            renderInput={(params) => (
                                                                <TextField
                                                                    {...params} 
                                                                    label={
                                                                        <>
                                                                            Freight UOM <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                                        </>
                                                                    } 
                                                                    error={freightUomId.error} 
                                                                    helperText={freightUomId.error ? freightUomId.errorText : ''}
                                                                />
                                                            )} 
                                                            onChange={(e, value) => handleFreightUomIdChange(e, value)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <TextField
                                                        id='freightRate'
                                                        type='number'
                                                        variant='outlined'
                                                        label='Freight Rate'
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                    />

                                                    <TextField
                                                        id='freightQty'
                                                        type='number'
                                                        variant='outlined'
                                                        label='Freight Qty'
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                    />
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <FormControl
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        variant='outlined'
                                                    >
                                                        <InputLabel htmlFor='freightInput'>Freight</InputLabel>
                                                        <OutlinedInput
                                                            id='freightInput'
                                                            disabled={isLock}
                                                            type='number'
                                                            variant='outlined'
                                                            label='Freight'
                                                            endAdornment={
                                                                <InputAdornment position='end'>
                                                                    <IconButton
                                                                        aria-label='toggle-lock-freight'
                                                                        onClick={handleFreightLock}
                                                                        edge='end'>
                                                                        {isLock ? (
                                                                            <i className='fa-regular fa-lock'></i>
                                                                        ) : (
                                                                            <i className='fa-regular fa-lock-open'></i>
                                                                        )}
                                                                    </IconButton>
                                                                </InputAdornment>
                                                            }
                                                            sx={{
                                                                backgroundColor:
                                                                    isLock === true
                                                                        ? theme.palette.secondary.main
                                                                        : '#ffffff',
                                                            }}
                                                        />
                                                    </FormControl>
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <TextField
                                                        id='subContractorRate'
                                                        type='number'
                                                        variant='outlined'
                                                        label='Sub-contractor Rate'
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                    />
                                                    <TextField
                                                        id='salesTaxRate'
                                                        type='number'
                                                        variant='outlined'
                                                        label='Sales Tax Rate'
                                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                    />
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <TextField
                                                        id='requestedNumberOfTrucks'
                                                        type='number'
                                                        variant='outlined'
                                                        label='Requested Number of Trucks'
                                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                    />
                                                    <FormControlLabel
                                                        label='Run Until Stopped'
                                                        control={<Checkbox />}
                                                    />
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <DemoContainer
                                                        sx={{ p: 0, flexBasis: { xs: '100%', sm: '50%' } }}
                                                        components={['MobileTimePicker']}>
                                                        <MobileTimePicker
                                                            label='Time on Job'
                                                            sx={{ flexBasis: '100%' }}
                                                        />
                                                    </DemoContainer>
                                                    <TextField
                                                        id='chargeTo'
                                                        variant='outlined'
                                                        label='Charge To'
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                    />
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    <Autocomplete
                                                        id='priority'
                                                        options={priorityTypes}
                                                        renderInput={(params) => (
                                                            <TextField {...params} label='Priority' />
                                                        )}
                                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                    />
                                                </Stack>

                                                <Stack direction='column' spacing={1}>
                                                    <TextField
                                                        id='note'
                                                        multiline
                                                        variant='outlined'
                                                        label='Note'
                                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                    />
                                                    <FormControlLabel
                                                        label='Requires Notification'
                                                        control={<Checkbox />}
                                                    />
                                                </Stack>
                                            </React.Fragment>
                                        }
                                    </Stack>
                                </Box>
                        </LocalizationProvider>
                    </Box>

                    <Box sx={{ p: 2 }}>
                        <Stack 
                            spacing={2}
                            direction='row' 
                            justifyContent='flex-end'
                        >
                            <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
                            <Button variant='contained' color='primary' onClick={(e) => handleSave(e)}>
                                <i className='fa-regular fa-save' style={{ marginRight: '6px' }}></i>
                                <Typography>Save</Typography>
                            </Button>
                        </Stack>
                    </Box>
                </React.Fragment>
            }
        </React.Fragment>
    );
};

export default AddOrEditJob;
