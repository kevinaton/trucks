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
import App from '../../config/appConfig';
import { renderDate } from '../../helpers/misc_helper';
import { grey } from '@mui/material/colors';
import { theme } from '../../Theme';
import {
    getActiveCustomersSelectList, 
    getDesignationsSelectList, 
    getLocationsSelectList,
    getServicesWithTaxInfoSelectList,
    getVehicleCategories,
    getUnitsOfMeasureSelectList,
    getOrderPrioritySelectList,
    getJobForEdit, 
} from '../../store/actions';

const AddOrEditJob = ({ 
    userAppConfiguration, 
    dataFilter,
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
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState(null);
    const [isLoadingUnitsOfMeasure, setIsLoadingUnitsOfMeasure] = useState(false);
    const [unitOfMeasureOptions, setUnitOfMeasureOptions] = useState(null);
    const [isLoadingOrderPriority, setIsLoadingOrderPriority] = useState(false);
    const [orderPriorityOptions, setOrderPriorityOptions] = useState(null);
    const [jobInfo, setJobInfo] = useState(null);
    const [enableMaterialFields, setEnableMaterialFields] = useState(false);
    const [enableFreightFields, setEnableFreightFields] = useState(false);

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
    const [materialUomId, setMaterialUomId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [freightPricePerUnit, setFreightPricePerUnit] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [materialPricePerUnit, setMaterialPricePerUnit] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [productionPay, setProductionPay] = useState('');
    const [freightRateToPayDrivers, setFreightRateToPayDrivers] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [loadBased, setLoadBased] = useState(false);
    const [freightQuantity, setFreightQuantity] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [materialQuantity, setMaterialQuantity] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [freightPrice, setFreightPrice] = useState('');
    const [isFreightPriceLock, setIsFreightPriceLock] = useState(false);
    const [materialPrice, setMaterialPrice] = useState('');
    const [isMaterialPriceLock, setIsMaterialPriceLock] = useState(false);
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
    const [selectedVehicleCategories, setSelectedVehicleCategories] = useState([]);

    const [item, setItem] = useState('');
    const [materialUom, setMaterialUom] = useState('');
    const [materialRate, setMaterialRate] = useState('');
    const [material, setMaterial] = useState('');
    const [subContractorRate, setSubContractorRate] = useState('');
    const [requestedNumberOfTrucks, setRequestedNumberOfTrucks] = useState('');
    const [isRunUntilStopped, setIsRunUntilStopped] = useState(false);
    const [isRequireNotification, setIsRequireNotification] = useState(false);

    const priorityTypes = ['High', 'Medium', 'Low'];

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
        vehicleCategories,
        isLoadingUnitOfMeasuresOpts,
        unitsOfMeasureSelectList,
        isLoadingOrderPriorityOpts,
        orderPrioritySelectList,
        jobForEdit
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
        vehicleCategories: state.TruckReducer.vehicleCategories,
        isLoadingUnitOfMeasuresOpts: state.UnitOfMeasureReducer.isLoadingUnitOfMeasuresOpts,
        unitsOfMeasureSelectList: state.UnitOfMeasureReducer.unitsOfMeasureSelectList,
        isLoadingOrderPriorityOpts: state.OrderReducer.isLoadingOrderPriorityOpts,
        orderPrioritySelectList: state.OrderReducer.orderPrioritySelectList,
        jobForEdit: state.OrderReducer.jobForEdit
    }));

    useEffect(() => {
        dispatch(getActiveCustomersSelectList({
            maxResultCount: 1000,
            skipCount: 0
        }));
        dispatch(getDesignationsSelectList());
        dispatch(getJobForEdit({
            deliveryDate: dataFilter.date,
            officeId: dataFilter.officeId,
            officeName: dataFilter.officeName
        }));
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
        if (!isEmpty(locationOptions) && !isEmpty(jobInfo) && locationId.defaultValue === null) {
            const { officeId } = jobInfo;
            const defaultIndex = _.findIndex(locationOptions, { id: officeId.toString() });
            setLocationId({
                ...locationId,
                defaultValue: defaultIndex,
                initialized: true
            });
        }
    }, [locationOptions, jobInfo]);

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
        if (!isEmpty(vehicleCategories) && !isEmpty(vehicleCategories.result)) {
            const { result } = vehicleCategories;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setVehicleCategoryOptions(result.items);
            }
        }
    }, [vehicleCategories]);

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
        if (!isLoadingOrderPriorityOpts && !isEmpty(orderPrioritySelectList)) {
            const { result } = orderPrioritySelectList;
            if (!isEmpty(result)) {
                setOrderPriorityOptions(result);
            }
        }
    }, [orderPrioritySelectList]);

    useEffect(() => {
        if (!isLoadingOrderPriorityOpts !== isLoadingOrderPriority) {
            setIsLoadingOrderPriority(isLoadingOrderPriorityOpts);
        }
    }, [isLoadingOrderPriorityOpts]);

    useEffect(() => {
        if (jobInfo === null && !isEmpty(jobForEdit) && !isEmpty(jobForEdit.result)) {
            const { result } = jobForEdit;
            setJobInfo(result);
            setId(result.id);
            setIsFreightPriceOverridden(result.isFreightPriceOverridden);
            setIsFreightPricePerUnitOverridden(result.isFreightPricePerUnitOverridden);
            setIsMaterialPriceOverridden(result.isMaterialPriceOverridden);
            setIsMaterialPricePerUnitOverridden(result.isMaterialPricePerUnitOverridden);

            if (!isEmpty(dataFilter)) {
                setDeliveryDate({
                    ...deliveryDate,
                    value: moment(dataFilter.date)
                });
            }
        }
    }, [jobInfo, jobForEdit]);

    const designationHasMaterial = (selectedDesignation) => {
        return App.Enums.Designations.hasMaterial.includes(selectedDesignation);
    };

    const designationIsMaterialOnly = (selectedDesignation) => {
        return App.Enums.Designations.materialOnly.includes(selectedDesignation);
    };

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
        
        const inputValue = e.target.value;
        if (inputValue.length <= 20) {
            setJobNumber(inputValue);
        }
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

            if (isEmpty(vehicleCategories)) {
                dispatch(getVehicleCategories({
                    maxResultCount: 1000,
                    skipCount: 0
                }));
            }

            if (isEmpty(unitsOfMeasureSelectList)) {
                dispatch(getUnitsOfMeasureSelectList({
                    maxResultCount: 1000,
                    skipCount: 0,
                }));
            }

            if (isEmpty(orderPrioritySelectList)) {
                dispatch(getOrderPrioritySelectList());
            }

            if (designationHasMaterial(newValue)) {
                setEnableMaterialFields(true);
            } else {
                setEnableMaterialFields(false);
            }
    
            if (designationIsMaterialOnly(newValue)) {
                setEnableFreightFields(false);
            } else {
                setEnableFreightFields(true);
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
        setLoadAtId(newValue);
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
        setDeliverToId(newValue);
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

    const handleSelectVehicleCategory = (e, newValue) => {
        e.preventDefault();
        setSelectedVehicleCategories(newValue);
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

    const handleFreightPricePerUnitChange = (e) => {
        e.preventDefault();

        const inputValue = parseFloat(e.target.value);
        const minValue = 0;
        const maxValue = 999999999999999;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Value must be greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Value must be less than or equal to ${maxValue}`;
        }

        setFreightPricePerUnit({
            ...freightPricePerUnit,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleFreightPriceLock = (e) => {
        setIsFreightPriceLock(!isFreightPriceLock);
        setIsFreightPricePerUnitOverridden(!isFreightPriceOverridden)
    };

    const handleMaterialPriceLock = (e) => {
        setIsMaterialPriceLock(!isMaterialPriceLock);
        setIsMaterialPricePerUnitOverridden(!isMaterialPriceOverridden)
    };

    const handleMaterialUomIdChange = (e, newValue) => { 
        e.preventDefault();
        
        setMaterialUomId({
            ...materialUomId,
            value: newValue,
            error: false,
            errorText: ''
        });
    };

    const handleMaterialPricePerUnitChange = (e) => {
        e.preventDefault();

        const inputValue = parseFloat(e.target.value);
        const minValue = 0;
        const maxValue = 999999999999999;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Value must be greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Value must be less than or equal to ${maxValue}`;
        }

        setMaterialPricePerUnit({
            ...materialPricePerUnit,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        })
    };

    const handleFreightRateToPayDriversChange = (e) => {
        e.preventDefault();
        
        const inputValue = parseFloat(e.target.value);
        const minValue = 0;
        const maxValue = 999999999999999;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Value must be greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Value must be less than or equal to ${maxValue}`;
        }

        setFreightRateToPayDrivers({
            ...freightRateToPayDrivers,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleFreightQtyChange = (e) => {
        e.preventDefault();
        
        const inputValue = parseInt(e.target.value);
        const minValue = 0;
        const maxValue = 1000000;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Value must be greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Value must be less than or equal to ${maxValue}`;
        }

        setFreightQuantity({
            ...freightQuantity,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleMaterialQtyChange = (e) => {
        e.preventDefault();

        const inputValue = parseInt(e.target.value);
        const minValue = 0;
        const maxValue = 1000000;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Value must be greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Value must be less than or equal to ${maxValue}`;
        }

        setMaterialQuantity({
            ...materialQuantity,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();

        if (deliveryDate.required && !deliveryDate.value) {
            setDeliveryDate({
                ...deliveryDate,
                error: true
            });
            return;
        }

        if (customerId.required && !customerId.value) {
            setCustomerId({
                ...customerId,
                error: true
            });
            return;
        }

        if (designation.required && !designation.value) {
            setDesignation({
                ...designation,
                error: true
            });
            return;
        }

        if (serviceId.required && !serviceId.value) {
            setServiceId({
                ...serviceId,
                error: true
            });
            return;
        }

        const data = {
            
        };
    };

    console.log('userAppConfiguration: ', userAppConfiguration)

    return (
        <React.Fragment>
            { !isEmpty(userAppConfiguration) && !isEmpty(jobInfo) && 
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
                                                            disabled={jobForEdit.isSingleOffice}
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
                                                defaultValue={jobForEdit.jobNumber} 
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
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }
                                                    
                                                    { userAppConfiguration.settings.allowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders && vehicleCategoryOptions &&
                                                        <Autocomplete 
                                                            multiple 
                                                            id='vehicleCategories' 
                                                            options={vehicleCategoryOptions} 
                                                            getOptionLabel={(option) => option.name} 
                                                            filterSelectedOptions 
                                                            renderInput={(params) => (
                                                                <TextField 
                                                                    {...params} 
                                                                    label='Truck/Trailer Category' 
                                                                />
                                                            )} 
                                                            onChange={(e, value) => handleSelectVehicleCategory(e, value)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { !isLoadingUnitsOfMeasure && 
                                                        <React.Fragment>
                                                            { enableFreightFields && 
                                                                <Autocomplete
                                                                    id='freightUomId' 
                                                                    options={unitOfMeasureOptions} 
                                                                    getOptionLabel={(option) => option.name}
                                                                    renderInput={(params) => (
                                                                        <TextField {...params} 
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
                                                                    sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                                />
                                                            }

                                                            { enableMaterialFields && 
                                                                <Autocomplete 
                                                                    id='materialUomId'
                                                                    options={unitOfMeasureOptions} 
                                                                    getOptionLabel={(option) => option.name} 
                                                                    renderInput={(params) => (
                                                                        <TextField {...params} 
                                                                            label={
                                                                                <>
                                                                                    Material UOM <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                                                                </>
                                                                            } 
                                                                            error={materialUomId.error}
                                                                            helperText={materialUomId.error ? materialUomId.errorText : ''}
                                                                        />
                                                                    )}
                                                                    onChange={(e, value) => handleMaterialUomIdChange(e, value)}
                                                                    sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                                />
                                                            }
                                                        </React.Fragment>
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { enableFreightFields &&
                                                        <TextField
                                                            id='freightPricePerUnit'
                                                            type='number'
                                                            variant='outlined'
                                                            label='Freight Rate' 
                                                            onChange={(e) => handleFreightPricePerUnitChange(e)} 
                                                            error={freightPricePerUnit.error} 
                                                            helperText={freightPricePerUnit.error ? freightPricePerUnit.errorText : ''}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }

                                                    { enableMaterialFields && 
                                                        <TextField
                                                            id='materialPricePerUnit'
                                                            type='number'
                                                            variant='outlined'
                                                            label='Material Rate' 
                                                            onChange={(e) => handleMaterialPricePerUnitChange(e)} 
                                                            error={materialPricePerUnit.error} 
                                                            helperText={materialPricePerUnit.error ? materialPricePerUnit.errorText : ''}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                { userAppConfiguration.settings.allowProductionPay && userAppConfiguration.features.driverProductionPayFeature && 
                                                    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                        <TextField 
                                                            id='freightRateToPayDrivers' 
                                                            type='number' 
                                                            variant='outlined' 
                                                            label='Freight Rate for Driver Pay' 
                                                            onChange={(e) => handleFreightRateToPayDriversChange(e)} 
                                                            error={freightRateToPayDrivers.error}
                                                            helperText={freightRateToPayDrivers.error ? freightRateToPayDrivers.errorText : ''}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                        
                                                        <FormControlLabel
                                                            label='Load-based'
                                                            control={
                                                                <Checkbox 
                                                                    checked 
                                                                    onChange={(e) => setLoadBased(e.target.checked)} 
                                                                />
                                                            }
                                                        />
                                                    </Stack>
                                                }

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { enableFreightFields && 
                                                        <TextField
                                                            id='freightQty'
                                                            type='number'
                                                            variant='outlined'
                                                            label='Freight Quantity' 
                                                            onChange={(e) => handleFreightQtyChange(e)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }

                                                    { enableMaterialFields && 
                                                        <TextField
                                                            id='materialQty'
                                                            type='number'
                                                            variant='outlined'
                                                            label='Material Quantity' 
                                                            onChange={(e) => handleMaterialQtyChange(e)}
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                        />
                                                    }
                                                </Stack>

                                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                                    { enableFreightFields && 
                                                        <FormControl
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                            variant='outlined'
                                                        >
                                                            <InputLabel htmlFor='freightInput'>Freight</InputLabel>
                                                            <OutlinedInput
                                                                id='freightInput'
                                                                disabled={jobInfo.canOverrideTotals && isFreightPriceOverridden ? null : 'disabled'}
                                                                type='number'
                                                                variant='outlined'
                                                                label='Freight'
                                                                endAdornment={ jobInfo.canOverrideTotals ? 
                                                                    <InputAdornment position='end'>
                                                                        { jobInfo.canOverrideTotals && 
                                                                            <IconButton
                                                                                aria-label='toggle-lock-freight'
                                                                                onClick={handleFreightPriceLock}
                                                                                edge='end'>
                                                                                { !isFreightPriceLock ? (
                                                                                    <i className='fa-regular fa-lock'></i>
                                                                                ) : (
                                                                                    <i className='fa-regular fa-lock-open'></i>
                                                                                )}
                                                                            </IconButton>
                                                                        }
                                                                    </InputAdornment>
                                                                    : null
                                                                }
                                                                sx={{ backgroundColor: isFreightPriceLock === true
                                                                    ? theme.palette.secondary.main
                                                                    : '#ffffff',
                                                                }}
                                                            />
                                                        </FormControl>
                                                    }

                                                    { enableMaterialFields && 
                                                        <FormControl
                                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                                            variant='outlined'
                                                        >
                                                            <InputLabel htmlFor='materialInput'>Material</InputLabel>
                                                            <OutlinedInput
                                                                id='materialInput'
                                                                disabled={jobInfo.canOverrideTotals && isMaterialPriceOverridden ? null : 'disabled'}
                                                                type='number'
                                                                variant='outlined'
                                                                label='Material'
                                                                endAdornment={
                                                                    <InputAdornment position='end'>
                                                                        { jobInfo.canOverrideTotals && 
                                                                            <IconButton
                                                                                aria-label='toggle-lock-material'
                                                                                onClick={handleMaterialPriceLock}
                                                                                edge='end'>
                                                                                { !isMaterialPriceLock ? (
                                                                                    <i className='fa-regular fa-lock'></i>
                                                                                ) : (
                                                                                    <i className='fa-regular fa-lock-open'></i>
                                                                                )}
                                                                            </IconButton>
                                                                        }
                                                                    </InputAdornment>
                                                                }
                                                                sx={{
                                                                    backgroundColor:
                                                                        isMaterialPriceLock === true
                                                                            ? theme.palette.secondary.main
                                                                            : '#ffffff',
                                                                }}
                                                            />
                                                        </FormControl>
                                                    }
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
                                                    { !isLoadingOrderPriority && 
                                                        <Autocomplete
                                                            id='priority'
                                                            options={orderPriorityOptions} 
                                                            getOptionLabel={(option) => option.value}
                                                            renderInput={(params) => (
                                                                <TextField {...params} label='Priority' />
                                                            )}
                                                            sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                                        />
                                                    }
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