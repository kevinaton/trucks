import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import PropTypes from 'prop-types';
import {
    Box,
    Button,
    Checkbox,
    FormControl,
    FormControlLabel,
    InputLabel,
    MenuItem,
    Select,
    Stack,
    Tabs,
    Tab,
    TextField,
    Typography
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import CloseIcon from '@mui/icons-material/Close';
import moment from 'moment';
import { isEmpty } from 'lodash';
import { 
    getVehicleCategories, 
    getDriversSelectList,
    getActiveTrailersSelectList,
    getBedConstructionSelectList,
    getFuelTypeSelectList, 
    getWialonDeviceTypesSelectList,
    getTruckForEdit 
} from '../../store/actions';
import { assetType } from '../../common/enums/assetType';

const TabPanel = (props) => {
    const { children, value, index, ...other } = props;
  
    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`create-new-truck-tabpanel-${index}`}
            aria-labelledby={`create-new-truck-tab-${index}`}
            {...other}
        >
            {value === index && (
                <Box sx={{ p: 3 }}>
                    <Typography>{children}</Typography>
                </Box>
            )}
        </div>
    );
};

TabPanel.propTypes = {
    children: PropTypes.node,
    index: PropTypes.number.isRequired,
    value: PropTypes.number.isRequired,
};

const a11yProps = (index) => {
    return {
        id: `create-new-truck-${index}`,
        'aria-controls': `create-new-truck-tabpanel-${index}`,
    };
};

const AddOrEditTruckForm = ({
    pageConfig,
    closeModal
}) => {
    console.log('pageConfig: ', pageConfig)
    const today = moment();
    const [value, setValue] = useState(0);
    const [officeOptions, setOfficeOptions] = useState(null);
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState(null);
    const [defaultDriverOptions, setDefaultDriverOptions] = useState(null);
    const [activeTrailersOptions, setActiveTrailersOptions] = useState(null);
    const [bedConstructionOptions, setBedConstructionOptions] = useState(null);
    const [fuelTypeOptions, setFuelTypeOptions] = useState(null);
    const [wialonDeviceTypesOptions, setWialonDeviceTypesOptions] = useState(null);
    const [truckInfo, setTruckInfo] = useState(null);

    // general tab
    const [id, setId] = useState(null);
    const [vehicleCategoryIsPowered, setVehicleCategoryIsPowered] = useState(null);
    const [vehicleCategoryAssetType, setVehicleCategoryAssetType] = useState(null);
    const [truckCode, setTruckCode] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [officeId, setOfficeId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [vehicleCategoryId, setVehicleCategoryId] = useState({
        value: truckInfo != null ? truckInfo.vehicleCategoryId : '',
        required: true,
        error: false,
        errorText: ''
    });
    const [defaultDriverId, setDefaultDriverId] = useState({
        value: truckInfo != null ? truckInfo.defaultDriverId : '',
        required: false,
        error: false,
        errorText: '',
        disabled: true
    });
    const [defaultTrailerId, setDefaultTrailerId] = useState(null);
    const [isActive, setIsActive] = useState(true);
    const [inactivationDate, setInactivationDate] = useState({
        value: today,
        required: !isActive ? true : false,
        error: false,
        errorText: ''
    });
    const [isOutOfService, setIsOutOfService] = useState(false);
    const [reason, setReason] = useState({
        value: '',
        required: isOutOfService ? true : false,
        error: false,
        errorText: ''
    });
    const [isApportioned, setIsApportioned] = useState(false);
    const [showCanPullTrailer, setShowCanPullTrailer] = useState(false);
    const [canPullTrailer, setCanPullTrailer] = useState(false);
    const [year, setYear] = useState({
        value: truckInfo != null ? truckInfo.year : '',
        error: false,
        errorText: ''
    });
    const [make, setMake] = useState('');
    const [model, setModel] = useState('');
    const [inServiceDate, setInServiceDate] = useState({
        value: today,
        required: true,
        error: false,
        errorText: ''
    });
    const [vin, setVin] = useState('');
    const [plate, setPlate] = useState('');
    const [plateExpiration, setPlateExpiration] = useState(today);
    const [cargoCapacity, setCargoCapacity] = useState({
        value: truckInfo != null ? truckInfo.cargoCapacity : '',
        error: false,
        errorText: ''
    });
    const [cargoCapacityCyds, setCargoCapacityCyds] = useState({
        value: truckInfo != null ? truckInfo.cargoCapacityCyds : '',
        error: false,
        errorText: ''
    });
    const [insurancePolicyNumber, setInsurancePolicyNumber] = useState('');
    const [insuranceValidUntil, setInsuranceValidUntil] = useState(today);
    const [purchaseDate, setPurchaseDate] = useState(today);
    const [purchasePrice, setPurchasePrice] = useState('');
    const [soldDate, setSoldDate] = useState(today);
    const [soldPrice, setSoldPrice] = useState('');
    const [truxTruckId, setTruxTruckId] = useState('');

    // maintenance tab
    const [showBedConstruction, setShowBedConstruction] = useState(false);
    const [bedConstruction, setBedConstruction] = useState('');
    const [fuelType, setFuelType] = useState('');
    const [fuelCapacity, setFuelCapacity] = useState({
        value: truckInfo != null ? truckInfo.fuelCapacity : '',
        error: false,
        errorText: ''
    });
    const [steerTires, setSteerTires] = useState('');
    const [driveAxleTires, setDriveAxleTires] = useState('');
    const [dropAxleTires, setDropAxleTires] = useState('');
    const [trailerTires, setTrailerTires] = useState('');
    const [transmission, setTransmission] = useState('');
    const [engine, setEngine] = useState('');
    const [rearEnd, setRearEnd] = useState('');

    // service tab
    const [currentMileage, setCurrentMileage] = useState('');
    const [currentHours, setCurrentHours] = useState('');

    // files tab
    const [file, setFile] = useState(null);
    const [files, setFiles] = useState([]);

    // gps configuration tab
    const [dtdTrackerDeviceTypeId, setDtdTrackerDeviceTypeId] = useState(null);
    const [dtdTrackerDeviceTypeName, setDtdTrackerDeviceTypeName] = useState('');
    const [dtdTrackerServerAddress, setDtdTrackerServerAddress] = useState('');
    const [dtdTrackerUniqueId, setDtdTrackerUniqueId] = useState('');
    const [dtdTrackerPassword, setDtdTrackerPassword] = useState('');

    const dispatch = useDispatch();
    const { 
        offices,
        vehicleCategories, 
        driversSelectList,
        activeTrailersSelectList,
        bedConstructionSelectList,
        fuelTypeSelectList,
        wialonDeviceTypesSelectList,
        truckForEdit
    } = useSelector((state) => ({
        offices: state.OfficeReducer.offices,
        vehicleCategories: state.TruckReducer.vehicleCategories, 
        driversSelectList: state.DriverReducer.driversSelectList,
        activeTrailersSelectList: state.TruckReducer.activeTrailersSelectList,
        bedConstructionSelectList: state.TruckReducer.bedConstructionSelectList,
        fuelTypeSelectList: state.TruckReducer.fuelTypeSelectList, 
        wialonDeviceTypesSelectList: state.TruckReducer.wialonDeviceTypesSelectList,
        truckForEdit: state.TruckReducer.truckForEdit
    }));

    useEffect(() => {
        if (officeOptions === null && !isEmpty(offices) && !isEmpty(offices.result)) {
            const { result } = offices;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setOfficeOptions(result.items);
            }
        }
    }, [officeOptions, offices]);

    useEffect(() => {
        dispatch(getVehicleCategories());
        dispatch(getDriversSelectList());
        dispatch(getBedConstructionSelectList());
        dispatch(getFuelTypeSelectList());
        dispatch(getWialonDeviceTypesSelectList());
        dispatch(getTruckForEdit());
    }, []);

    useEffect(() => {
        if (!isEmpty(vehicleCategories) && !isEmpty(vehicleCategories.result)) {
            const { result } = vehicleCategories;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setVehicleCategoryOptions(result.items);
            }
        }
    }, [vehicleCategories]);

    useEffect(() => {
        if (!isEmpty(driversSelectList) && !isEmpty(driversSelectList.result)) {
            const { result } = driversSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setDefaultDriverOptions(result.items);
            }
        }
    }, [driversSelectList]);

    useEffect(() => {
        if (!isEmpty(activeTrailersSelectList) && !isEmpty(activeTrailersSelectList.result)) {
            const { result } = activeTrailersSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setActiveTrailersOptions(result.items);
            }
        }
    }, [activeTrailersSelectList]);

    useEffect(() => {
        if (!isEmpty(bedConstructionSelectList) && !isEmpty(bedConstructionSelectList.result)) {
            const { result } = bedConstructionSelectList;
            if (!isEmpty(result)) {
                setBedConstructionOptions(result);
            }
        }
    }, [bedConstructionSelectList]);

    useEffect(() => {
        if (!isEmpty(fuelTypeSelectList) && !isEmpty(fuelTypeSelectList.result)) {
            const { result } = fuelTypeSelectList;
            if (!isEmpty(result)) {
                setFuelTypeOptions(result);
            }
        }
    }, [fuelTypeSelectList]);

    useEffect(() => {
        if (!isEmpty(wialonDeviceTypesSelectList) && !isEmpty(wialonDeviceTypesSelectList.result)) {
            const { result } = wialonDeviceTypesSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setWialonDeviceTypesOptions(result.items);
            }
        }
    }, [wialonDeviceTypesSelectList]);

    useEffect(() => {
        if (truckInfo === null && !isEmpty(truckForEdit) && !isEmpty(truckForEdit.result)) {
            const { result } = truckForEdit;
            if (!isEmpty(result)) {
                setTruckInfo(result);
                setId(result.id);
                setVehicleCategoryAssetType(result.vehicleCategoryAssetType);
                setShowCanPullTrailer(Boolean(result.canPullTrailer));

                if (result.vehicleCategoryAssetType === assetType.DUMP_TRUCK || result.vehicleCategoryAssetType === assetType.TRAILER) {
                    showBedConstruction(true);
                }
                
                let shouldDisableDefaultDriver;
                if (result.vehicleCategoryIsPowered !== null) {
                    const isPowered = Boolean(result.vehicleCategoryIsPowered.toLowerCase());
                    setVehicleCategoryIsPowered(isPowered);
                    shouldDisableDefaultDriver = !isPowered;
                } else {
                    shouldDisableDefaultDriver = true;
                }

                setDefaultDriverId({
                    ...defaultDriverId,
                    disabled: shouldDisableDefaultDriver
                })
            }
        }
    }, [truckInfo, truckForEdit, defaultDriverId, showBedConstruction]);

    // handle change tab
    const handleChange = (event, newValue) => {
        setValue(newValue);
    };

    const handleTruckCodeInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        if (inputValue.length <= 25) {
            setTruckCode({
                ...truckCode,
                value: inputValue,
                error: false,
                errorText: ''
            });
        }
    };

    const handleOfficeIdInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        setOfficeId({
            ...officeId,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleVehicleCategoryIdInputChange = (selectedOption) => {
        const { isPowered, assetType } = selectedOption.item;
        setVehicleCategoryIsPowered(isPowered);
        setVehicleCategoryAssetType(assetType);

        if ([assetType.DUMP_TRUCK, assetType.TRAILER].includes(assetType)) {
            setBedConstruction(true);
        }

        if (isPowered) {
            setShowCanPullTrailer(true);
            setCanPullTrailer(assetType === assetType.TRACTOR);
        } else {
            setDefaultDriverId({
                ...defaultDriverId,
                disabled: true
            })
        }
        
        const inputValue = selectedOption.id;
        setVehicleCategoryId({
            ...vehicleCategoryId,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleDefaultDriverIdInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        setDefaultDriverId({
            ...defaultDriverId,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleDefaultTrailerIdInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        setDefaultTrailerId({
            ...defaultTrailerId,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleIsActiveChange = (e) => {
        setIsActive(e.target.checked);
    };

    const handleInactivationDateChange = (newDate) => {
        setInactivationDate({
            ...inactivationDate,
            value: moment(newDate).format('MM/DD/YYYY'),
            error: false,
            errorText: ''
        });
    };

    const handleIsOutOfServiceChange = (e) => {
        setIsOutOfService(e.target.checked);
    };

    const handleReasonInputChange = (e) => {
        const inputValue = e.target.value;
        //const remainingChars = 500 - inputValue.length;

        setReason({
            ...reason,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleIsApportionedChange = (e) => {
        setIsApportioned(e.target.checked);
    };

    const handleCanPullTrailerChange = (e) => {
        if (e.target.checked && activeTrailersOptions === null) {
            dispatch(getActiveTrailersSelectList());
        }
        setCanPullTrailer(e.target.checked);
    };

    const handleYearInputChange = (e) => {
        const inputValue = parseInt(e.target.value);
        const minValue = 1900;
        const maxValue = 2100;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Year must be greater than ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Year must be less than ${maxValue}`;
        }

        setYear({
            ...year,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleMakeInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setMake(inputValue);
        }
    };

    const handleModelInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setModel(inputValue);
        }
    };

    const handleInServiceDateChange = (newDate) => {
        setInServiceDate({
            ...inServiceDate,
            value: moment(newDate).format('MM/DD/YYYY'),
            error: false,
            errorText: ''
        });
    };

    const handleVinInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setVin(inputValue);
        }
    };

    const handlePlateInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 20) {
            setPlate(inputValue);
        }
    };

    const handlePlateExpirationChange = (newDate) => {
        setPlateExpiration(moment(newDate).format('MM/DD/YYYY'));
    };

    const handleCargoCapacityInputChange = (e) => {
        const inputValue = parseInt(e.target.value);
        const minValue = 0;
        const maxValue = 100000;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Please enter a value greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Please enter a value less than or equal to ${maxValue}`;
        }

        setCargoCapacity({
            ...cargoCapacity,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleCargoCapacityCydsInputChange = (e) => {
        const inputValue = parseInt(e.target.value);
        const minValue = 0;
        const maxValue = 100000;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Please enter a value greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Please enter a value less than or equal to ${maxValue}`;
        }
        
        setCargoCapacityCyds({
            ...cargoCapacityCyds,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleInsurancePolicyNumberInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setInsurancePolicyNumber(inputValue);
        }
    };

    const handleInsuranceValidUntilChange = (newDate) => {
        setInsuranceValidUntil(moment(newDate).format('MM/DD/YYYY'));
    };

    const handlePurchaseDateChange = (newDate) => {
        setPurchaseDate(moment(newDate).format('MM/DD/YYYY'));
    };

    const handlePurchasePriceInputChange = (e) => {
        const inputValue = parseFloat(e.target.value);
        if (!isNaN(inputValue) && 
            inputValue >= 0 && 
            inputValue <= 999999999999999) {
            setPurchasePrice(inputValue);
        } else {
            setPurchasePrice('');
        }
    };

    const handleSoldDateChange = (newDate) => {
        setSoldDate(moment(newDate).format('MM/DD/YYYY'));
    };

    const handleSoldPriceInputChange = (e) => {
        const inputValue = parseFloat(e.target.value);
        if (!isNaN(inputValue) && 
            inputValue >= 0 && 
            inputValue <= 999999999999999) {
            setSoldPrice(inputValue);
        } else {
            setSoldPrice('');
        }
    };

    const handleTruxTruckIdInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 20) {
            setTruxTruckId(inputValue);
        }
    };

    const handleBedConstructionInputChange = (e) => {
        setBedConstruction(e.target.value);
    };

    const handleFuelTypeInputChange = (e) => {
        setFuelType(e.target.value);
    };

    const handleFuelCapacityInputChange = (e) => {
        const inputValue = parseInt(e.target.value);
        const minValue = 0;
        const maxValue = 10000;

        let isNotValid = false;
        let errMsg = '';

        if (inputValue < minValue) {
            isNotValid = true;
            errMsg = `Please enter a value greater than or equal to ${minValue}`;
        } else if (inputValue > maxValue) {
            isNotValid = true;
            errMsg = `Please enter a value less than or equal to ${maxValue}`;
        }
        
        setFuelCapacity({
            ...fuelCapacity,
            value: !isNaN(inputValue) ? inputValue : '',
            error: isNotValid,
            errorText: errMsg
        });
    };

    const handleSteerTiresInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setSteerTires(inputValue);
        }
    };

    const handleDriveAxleTiresInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setDriveAxleTires(inputValue);
        }
    }; 
    
    const handleDropAxleTiresInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setDropAxleTires(inputValue);
        }
    };

    const handleTrailerTiresInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setTrailerTires(inputValue);
        }
    };

    const handleTransmissionInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setTransmission(inputValue);
        }
    };

    const handleEngineInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setEngine(inputValue);
        }
    };

    const handleRearEndInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 50) {
            setRearEnd(inputValue);
        }
    };

    const handleCurrentMileageInputChange = (e) => {
        setCurrentMileage(e.target.value);
    };

    const handleCurrentHoursInputChange = (e) => {
        setCurrentHours(e.target.value);
    }; 

    const handleDtdTrackerDeviceTypeIdInputChange = (selectedOption) => {
        const { serverAddress } = selectedOption.item;
        setDtdTrackerDeviceTypeId(selectedOption.id);
        setDtdTrackerServerAddress(serverAddress);
    };

    const handleDtdTrackerUniqueIdInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 100) {
            setDtdTrackerUniqueId(inputValue);
        }
    }; 

    const handleDtdTrackerPasswordInputChange = (e) => {
        setDtdTrackerPassword(e.target.value);
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };
    
    const handleSave = (e) => {
        e.preventDefault();
        
        // validate truck code
        if (truckCode.required && !truckCode.value) {
            setTruckCode({
                ...truckCode,
                error: true
            });
        }

        // validate office id
        if (officeId.required && !officeId.value) {
            setOfficeId({
                ...officeId,
                error: true
            });
        }

        // validate vehicle category id
        if (vehicleCategoryId.required && !vehicleCategoryId.value) {
            setVehicleCategoryId({
                ...vehicleCategoryId,
                error: true
            });
        }

        var data = {
            id,
            vehicleCategoryIsPowered,
            vehicleCategoryAssetType,
            truckCode: truckCode.value,
            officeId: officeId.value,
            vehicleCategoryId: vehicleCategoryId.value,
            defaultDriverId: defaultDriverId.value,
            defaultTrailerId: defaultTrailerId,
            isActive,
            inactivationDate: moment(inactivationDate.value).format('MM/DD/YYYY'),
            isOutOfService,
            reason: reason.value,
            isApportioned,
            canPullTrailer,
            year: year.value,
            make,
            model,
            inServiceDate: moment(inServiceDate.value).format('MM/DD/YYYY'),
            vin,
            plate,
            plateExpiration: moment(plateExpiration).format('MM/DD/YYYY'),
            cargoCapacity: cargoCapacity.value,
            cargoCapacityCyds: cargoCapacityCyds.value,
            insurancePolicyNumber,
            insuranceValidUntil: moment(insuranceValidUntil).format('MM/DD/YYYY'),
            purchaseDate: moment(purchaseDate).format('MM/DD/YYYY'),
            purchasePrice,
            soldDate: moment(soldDate).format('MM/DD/YYYY'),
            soldPrice,
            truxTruckId,
        };
        console.log('data: ', data)
    };

    const renderGeneralForm = () => {
        const { features } = pageConfig;
        console.log('defaultDriverId.disabled: ', defaultDriverId.disabled)
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
                <TextField 
                    id='truck-code'
                    name='truck-code' 
                    type='text' 
                    label={
                        <>
                            Truck Code {truckCode.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                        </>
                    }
                    value={truckCode.value} 
                    defaultValue={truckInfo.truckCode} 
                    onChange={handleTruckCodeInputChange} 
                    error={truckCode.error} 
                    helperText={truckCode.error ? truckCode.errorText : ''} 
                    fullWidth
                />
                
                { features.allowMultiOffice && 
                    <FormControl fullWidth>
                        <InputLabel id='officeId-label'>
                            Office {officeId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                        </InputLabel>
                        <Select
                            labelId='officeId-label'
                            id='officeId'
                            label={
                                <>
                                    Office {officeId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                                </>
                            }
                            value={officeId.value} 
                            defaultValue={truckInfo.officeId}
                            onChange={handleOfficeIdInputChange} 
                            error={officeId.error} 
                            helperText={officeId.error ? officeId.errorText : ''} 
                        >
                            <MenuItem value=''>Select an option</MenuItem>
                            
                            { officeOptions && officeOptions.map((option) => (
                                <MenuItem key={option.id} value={option.id}>
                                    {option.name}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                }

                <FormControl fullWidth>
                    <InputLabel id='vehicleCategoryId-label'>
                        Category {vehicleCategoryId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                    </InputLabel>
                    <Select
                        labelId='vehicleCategoryId-label'
                        id='vehicleCategoryId'
                        label={
                            <>
                                Category {vehicleCategoryId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                            </>
                        }
                        value={vehicleCategoryId.value} 
                        defaultValue={truckInfo.vehicleCategoryId}
                        error={vehicleCategoryId.error} 
                        helperText={vehicleCategoryId.error ? vehicleCategoryId.errorText : ''} 
                    >
                        <MenuItem value=''>Select an option</MenuItem>

                        { vehicleCategoryOptions && vehicleCategoryOptions.map((option) => (
                            <MenuItem 
                                key={option.id} 
                                value={option.id}
                                onClick={() => handleVehicleCategoryIdInputChange(option)}
                            >
                                {option.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                <FormControl
                    disabled={defaultDriverId.disabled}
                    fullWidth
                >
                    <InputLabel id='defaultDriver-label'>Default Driver</InputLabel>
                    <Select
                        labelId='defaultDriver-label'
                        id='defaultDriver'
                        label='Default Driver' 
                        value={defaultDriverId.value} 
                        defaultValue={truckInfo.defaultDriverId}
                        onChange={handleDefaultDriverIdInputChange}
                    >
                        <MenuItem value=''>Select an option</MenuItem>

                        { truckInfo.defaultDriverId !== null && 
                            <MenuItem value={truckInfo.defaultDriverId}>
                                {truckInfo.defaultDriverName}
                            </MenuItem>
                        }

                        { defaultDriverOptions && defaultDriverOptions.map((option) => (
                            <MenuItem key={option.id} value={option.id}>
                                {option.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                { canPullTrailer !== null && canPullTrailer && 
                    <FormControl fullWidth>
                        <InputLabel id='defaultTrailer-label'>Default Trailer</InputLabel>
                        <Select
                            labelId='defaultTrailer-label'
                            id='defaultTrailer'
                            label='Default Trailer' 
                            value={defaultDriverId.value} 
                            defaultValue={truckInfo.defaultTrailerId}
                            onChange={handleDefaultTrailerIdInputChange}
                        >
                            <MenuItem value=''>Select an option</MenuItem>

                            { truckInfo.defailtTrailerId !== null && 
                                <MenuItem value={truckInfo.defailtTrailerId}>
                                    {truckInfo.defaultTrailerCode}
                                </MenuItem>
                            }

                            { activeTrailersOptions && activeTrailersOptions.map((option) => (
                                <MenuItem key={option.id} value={option.id}>
                                    {option.name}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                }

                <FormControlLabel 
                    control={
                        <Checkbox 
                            checked={isActive} 
                            defaultChecked={truckInfo.isActive}
                            onChange={handleIsActiveChange}
                        />
                    } 
                    label="Active" 
                    fullWidth
                />

                { !isActive && 
                    <LocalizationProvider 
                        dateAdapter={AdapterMoment} 
                        adapterLocale='en-us'
                        fullWidth
                    >
                        <DatePicker 
                            id='inactivationDate'
                            name='inactivationDate'
                            label={
                                <>
                                    Inactivation Date <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                </>
                            } 
                            value={inactivationDate.value} 
                            defaultValue={truckInfo.inactivationDate !== null ? moment(truckInfo.inactivationDate) : moment()}
                            onChange={handleInactivationDateChange} 
                            error={inactivationDate.error}
                            helperText={inactivationDate.error ? inactivationDate.errorText : ''} 
                            sx={{ flexShrink: 0 }} 
                        />
                    </LocalizationProvider>
                }

                <FormControlLabel 
                    control={
                        <Checkbox 
                            checked={isOutOfService} 
                            defaultChecked={truckInfo.isOutOfService}
                            onChange={handleIsOutOfServiceChange}
                        />
                    } 
                    label="Out of Service" 
                    fullWidth
                />

                { isOutOfService && 
                    <TextField
                        id="reason"
                        label={
                            <>
                                Reason {truckCode.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                            </>
                        }
                        value={reason.value} 
                        defaultValue={truckInfo.reason}
                        onChange={handleReasonInputChange} 
                        multiline
                        rows={2} 
                        error={reason.error} 
                        helperText={reason.error ? reason.errorText : ''} 
                        fullWidth 
                        maxLength={500}
                    />
                }

                { Boolean(vehicleCategoryIsPowered) && 
                    <React.Fragment>
                        <FormControlLabel 
                            control={
                                <Checkbox 
                                    checked={isApportioned} 
                                    defaultChecked={truckInfo.isApportioned}
                                    onChange={handleIsApportionedChange}
                                />
                            } 
                            label="Apportioned" 
                            fullWidth
                        />

                        <FormControlLabel 
                            control={
                                <Checkbox 
                                    checked={canPullTrailer} 
                                    defaultChecked={truckInfo.canPullTrailer}
                                    onChange={handleCanPullTrailerChange}
                                />
                            } 
                            label="Can Pull Trailer" 
                            fullWidth
                        />
                    </React.Fragment>
                }

                <TextField 
                    id='year'
                    name='year'
                    type='text' 
                    label='Year'
                    value={year.value} 
                    defaultValue={truckInfo.year}
                    onChange={handleYearInputChange} 
                    error={year.error} 
                    helperText={year.error ? year.errorText : ''} 
                    fullWidth
                />

                <TextField 
                    id='make'
                    name='make'
                    type='text'
                    label='Make'
                    value={make} 
                    defaultValue={truckInfo.make}
                    onChange={handleMakeInputChange}
                    fullWidth
                />

                <TextField
                    id='model'
                    name='model'
                    type='text'
                    label='Model'
                    value={model} 
                    defaultValue={truckInfo.model}
                    onChange={handleModelInputChange}
                    fullWidth
                />

                <LocalizationProvider 
                    dateAdapter={AdapterMoment} 
                    adapterLocale='en-us'
                    fullWidth
                >
                    <DatePicker 
                        id='inServiceDate'
                        name='inServiceDate'
                        label={
                            <>
                                In Service Date <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                            </>
                        } 
                        value={inServiceDate.value} 
                        defaultValue={truckInfo.inServiceDate !== null ? moment(truckInfo.inServiceDate) : moment()} 
                        onChange={handleInServiceDateChange} 
                        error={inServiceDate.error}
                        helperText={inServiceDate.error ? inServiceDate.errorText : ''} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </LocalizationProvider>

                <TextField
                    id='vin'
                    name='vin'
                    type='text'
                    label='VIN'
                    value={vin} 
                    defaultValue={truckInfo.vin}
                    onChange={handleVinInputChange}
                    fullWidth
                />

                <TextField
                    id='plate'
                    name='plate'
                    type='text'
                    label='Plate'
                    value={plate} 
                    defaultValue={truckInfo.plate}
                    onChange={handlePlateInputChange}
                    fullWidth
                />

                <LocalizationProvider 
                    dateAdapter={AdapterMoment} 
                    adapterLocale='en-us'
                    fullWidth
                >
                    <DatePicker 
                        id='plateExpiration'
                        name='plateExpiration'
                        label='Plate Expiration'
                        value={plateExpiration} 
                        defaultValue={truckInfo.plateExpiration !== null ? moment(truckInfo.plateExpiration) : moment()}
                        onChange={handlePlateExpirationChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </LocalizationProvider>

                <TextField 
                    id='cargoCapacity'
                    name='cargoCapacity'
                    type='text'
                    label='Ave Load (tons)'
                    value={cargoCapacity.value} 
                    defaultValue={truckInfo.cargoCapacity}
                    onChange={handleCargoCapacityInputChange}
                    error={cargoCapacity.error} 
                    helperText={cargoCapacity.error ? cargoCapacity.errorText : ''} 
                    fullWidth 
                />

                <TextField 
                    id='cargoCapacityCyds' 
                    name='cargoCapacityCyds'
                    type='text' 
                    label='Ave Load (cyds)' 
                    value={cargoCapacityCyds.value} 
                    defaultValue={truckInfo.cargoCapacityCyds}
                    onChange={handleCargoCapacityCydsInputChange}
                    error={cargoCapacityCyds.error} 
                    helperText={cargoCapacityCyds.error ? cargoCapacityCyds.errorText : ''} 
                    fullWidth
                />

                <TextField 
                    id='insurancePolicyNumber' 
                    name='insurancePolicyNumber'
                    type='text' 
                    label='Insurance Policy Number' 
                    value={insurancePolicyNumber} 
                    defaultValue={truckInfo.insurancePolicyNumber}
                    onChange={handleInsurancePolicyNumberInputChange}
                    fullWidth
                />

                <LocalizationProvider 
                    dateAdapter={AdapterMoment} 
                    adapterLocale='en-us'
                    fullWidth
                >
                    <DatePicker 
                        id='insuranceValidUntil'
                        name='insuranceValidUntil'
                        label='Insurance Valid Until'
                        value={insuranceValidUntil} 
                        defaultValue={truckInfo.insuranceValidUntil !== null ? moment(truckInfo.insuranceValidUntil) : moment()}
                        onChange={handleInsuranceValidUntilChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </LocalizationProvider>

                <LocalizationProvider 
                    dateAdapter={AdapterMoment} 
                    adapterLocale='en-us'
                    fullWidth
                >
                    <DatePicker 
                        id='purchaseDate'
                        name='purchaseDate'
                        label='Purchase Date'
                        value={purchaseDate} 
                        defaultValue={truckInfo.purchaseDate !== null ? moment(truckInfo.purchaseDate) : moment()}
                        onChange={handlePurchaseDateChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </LocalizationProvider>

                <TextField 
                    id='purchasePrice'
                    name='purchasePrice'
                    type='text'
                    label='Purchase Price'
                    value={purchasePrice} 
                    defaultValue={truckInfo.purchasePrice}
                    onChange={handlePurchasePriceInputChange}
                    fullWidth
                />

                <LocalizationProvider 
                    dateAdapter={AdapterMoment} 
                    adapterLocale='en-us'
                    fullWidth
                >
                    <DatePicker 
                        id='soldDate'
                        name='soldDate'
                        label='Sold Date'
                        value={soldDate} 
                        defaultValue={truckInfo.soldDate !== null ? moment(truckInfo.soldDate) : moment()}
                        onChange={handleSoldDateChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </LocalizationProvider>

                <TextField 
                    id='soldPrice'
                    name='soldPrice'
                    type='text'
                    label='Sold Price'
                    value={soldPrice} 
                    defaultValue={truckInfo.soldPrice}
                    onChange={handleSoldPriceInputChange}
                    fullWidth
                />

                { Boolean(features.allowImportingTruxEarnings) && 
                    <TextField 
                        id='truxTruckId' 
                        name='truxTruckId'
                        type='text'
                        label='Trux Truck Id' 
                        value={truxTruckId} 
                        defaultValue={truckInfo.truxTruckId}
                        onChange={handleTruxTruckIdInputChange}
                        fullWidth
                    />
                }
            </Stack>
        );
    };

    const renderMaintenanceForm = () => {
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
                { showBedConstruction && 
                    <FormControl fullWidth>
                        <InputLabel id='bedConstruction-label'>Bed Construction</InputLabel>
                        <Select
                            labelId='bedConstruction-label'
                            id='bedConstruction'
                            label='Bed Construction'
                            value={bedConstruction} 
                            defaultValue={truckInfo.bedConstruction} 
                            onChange={handleBedConstructionInputChange}
                        >
                            <MenuItem value=''>Select an option</MenuItem>

                            { bedConstructionOptions && bedConstructionOptions.map((option) => (
                                <MenuItem key={option.key} value={option.key}>
                                    {option.value}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                }

                <FormControl fullWidth>
                    <InputLabel id='fuelType-label'>Fuel Type</InputLabel>
                    <Select
                        labelId='fuelType-label'
                        id='fuelType'
                        label='fuelType'
                        value={fuelType} 
                        defaultValue={truckInfo.fuelType} 
                        onChange={handleFuelTypeInputChange} 
                    >
                        <MenuItem value=''>Select an option</MenuItem>

                        { fuelTypeOptions && fuelTypeOptions.map((option) => (
                            <MenuItem key={option.key} value={option.key}>
                                {option.value}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                <TextField 
                    id='fuelCapacity'
                    name='fuelCapacity'
                    type='text'
                    label='Fuel Capacity'
                    value={fuelCapacity.value}
                    defaultValue={truckInfo.fuelCapacity} 
                    onChange={handleFuelCapacityInputChange}
                />

                <TextField 
                    id='steerTires' 
                    name='steerTires'
                    type='text'
                    label='Steer Tires'
                    value={steerTires}
                    defaultValue={truckInfo.steerTires} 
                    onChange={handleSteerTiresInputChange}
                />

                <TextField 
                    id='driveAxleTires' 
                    name='driveAxleTires'
                    type='text'
                    label='Drive Axle Tires'
                    value={driveAxleTires}
                    defaultValue={truckInfo.driveAxleTires} 
                    onChange={handleDriveAxleTiresInputChange}
                /> 

                <TextField 
                    id='dropAxleTires'
                    name='dropAxleTires'
                    type='text'
                    label='Drop Axle Tires'
                    value={dropAxleTires}
                    defaultValue={truckInfo.dropAxleTires} 
                    onChange={handleDropAxleTiresInputChange}
                />

                <TextField 
                    id='trailerTires'
                    name='trailerTires'
                    type='text'
                    label='Trailer Tires'
                    value={trailerTires}
                    defaultValue={truckInfo.trailerTires} 
                    onChange={handleTrailerTiresInputChange}
                />

                <TextField 
                    id='transmission' 
                    name='transmission'
                    type='text'
                    label='Transmission'
                    value={transmission}
                    defaultValue={truckInfo.transmission} 
                    onChange={handleTransmissionInputChange}
                />

                <TextField 
                    id='engine'
                    name='engine'
                    type='text'
                    label='Engine'
                    value={engine}
                    defaultValue={truckInfo.engine} 
                    onChange={handleEngineInputChange}
                />

                <TextField 
                    id='rearEnd'
                    name='rearEnd'
                    type='text'
                    label='Rear End'
                    value={rearEnd}
                    defaultValue={truckInfo.rearEnd} 
                    onChange={handleRearEndInputChange}
                />
            </Stack>
        );
    };

    const renderServiceForm = () => {
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
                <TextField 
                    id='currentMileage' 
                    name='currentMileage'
                    type='text'
                    label='Current Mileage' 
                    value={currentMileage}
                    defaultValue={truckInfo.currentMileage} 
                    onChange={handleCurrentMileageInputChange}
                />

                <TextField 
                    id='currentHours'
                    name='currentHours'
                    type='text'
                    label='Current Hours'
                    value={currentHours} 
                    defaultValue={truckInfo.currentHours} 
                    onChange={handleCurrentHoursInputChange}
                />
            </Stack>
        );
    };

    const renderFilesForm = () => {
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
                
            </Stack>
        );
    };

    const renderGPSConfigurationForm = () => {
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
                <FormControl fullWidth>
                    <InputLabel id='dtdTrackerDeviceTypeId-label'>Device Type</InputLabel>
                    <Select
                        labelId='dtdTrackerDeviceTypeId-label'
                        id='dtdTrackerDeviceTypeId'
                        label='dtdTrackerDeviceTypeId'
                        value={dtdTrackerDeviceTypeId} 
                        defaultValue={truckInfo.dtdTrackerDeviceTypeId} 
                    >
                        <MenuItem value=''>Select an option</MenuItem>

                        { truckInfo.dtdTrackerDeviceTypeId !== null && 
                            <MenuItem value={truckInfo.dtdTrackerDeviceTypeId}>
                                {truckInfo.dtdTrackerDeviceTypeName}
                            </MenuItem>
                        }
                        
                        { wialonDeviceTypesOptions && wialonDeviceTypesOptions.map((option) => (
                            <MenuItem 
                                key={option.id}
                                value={option.id} 
                                onClick={() => handleDtdTrackerDeviceTypeIdInputChange(option)}
                            >
                                {option.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                <TextField 
                    id='dtdTrackerServerAddress' 
                    name='dtdTrackerServerAddress'
                    type='text' 
                    label='Server Address' 
                    value={dtdTrackerServerAddress} 
                    defaultValue={truckInfo.dtdTrackerServerAddress} 
                    disabled
                />

                <TextField 
                    id='dtdTrackerUniqueId' 
                    name='dtdTrackerUniqueId'
                    type='text'
                    label='Unique Id'
                    value={dtdTrackerUniqueId}
                    defaultValue={truckInfo.dtdTrackerUniqueId} 
                    onChange={handleDtdTrackerUniqueIdInputChange}
                />

                <TextField 
                    id='dtdTrackerPassword' 
                    name='dtdTrackerPassword'
                    type='text'
                    label='Password'
                    value={dtdTrackerPassword}
                    defaultValue={truckInfo.dtdTrackerPassword} 
                    onChange={handleDtdTrackerPasswordInputChange}
                />
            </Stack>
        );
    };

    return (
        <React.Fragment>
            { !isEmpty(pageConfig) && !isEmpty(truckInfo) && 
                <React.Fragment>
                    <Box
                        display='flex'
                        justifyContent='space-between'
                        alignItems='center'
                        sx={{ 
                            p: 2 
                        }} 
                    >
                        <Typography variant='h6' component='h2'>
                            { truckInfo.id > 0 ? <>Edit truck</> : <>Create new truck</> }
                        </Typography>
                        <Button 
                            onClick={closeModal} 
                            sx={{ minWidth: '32px' }}
                        >
                            <CloseIcon />
                        </Button>
                    </Box>

                    <Box sx={{ width: '100%' }}>
                        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                            <Tabs value={value} onChange={handleChange} aria-label="Create new truck tabs">
                                <Tab label="General" {...a11yProps(0)} />

                                <Tab label="Maintenance" {...a11yProps(1)} />

                                { truckInfo.id > 0 && 
                                    <React.Fragment>
                                        <Tab label="Service" {...a11yProps(2)} />
                                        <Tab label="Files" {...a11yProps(3)} />
                                    </React.Fragment>
                                }

                                <Tab label="GPS Configuration" {...a11yProps(truckInfo.id > 0 ? 4 : 2)} />
                            </Tabs>
                        </Box>

                        <TabPanel value={value} index={0}>
                            {renderGeneralForm()}
                        </TabPanel>

                        <TabPanel value={value} index={1}>
                            {renderMaintenanceForm()}
                        </TabPanel>

                        { truckInfo.id > 0 && 
                            <React.Fragment>
                                <TabPanel value={value} index={2}>
                                    {renderServiceForm()}
                                </TabPanel>
                                
                                <TabPanel value={value} index={3}>
                                    {renderFilesForm()}
                                </TabPanel>
                            </React.Fragment>
                        }

                        <TabPanel value={value} index={truckInfo.id > 0 ? 4 : 2}>
                            {renderGPSConfigurationForm()}
                        </TabPanel>
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

export default AddOrEditTruckForm;