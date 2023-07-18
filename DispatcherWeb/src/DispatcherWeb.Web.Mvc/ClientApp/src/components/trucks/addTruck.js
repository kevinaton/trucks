import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import PropTypes from 'prop-types';
import { styled } from '@mui/material/styles';
import {
    Box,
    Button,
    Checkbox,
    FormControl,
    FormControlLabel,
    Grid,
    InputLabel,
    MenuItem,
    Paper,
    Select,
    Stack,
    Tabs,
    Tab,
    TextField,
    Typography,
    FormGroup
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import CloseIcon from '@mui/icons-material/Close';
import moment from 'moment';
import { isEmpty } from 'lodash';
import { getVehicleCategories, getTruckForEdit } from '../../store/actions';

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

const AddTruckForm = ({
    pageConfig,
    closeModal
}) => {
    console.log('pageConfig: ', pageConfig)
    const today = moment();
    const [value, setValue] = useState(0);
    const [officeOptions, setOfficeOptions] = useState(null);
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState(null);
    const [truckInfo, setTruckInfo] = useState(null);

    // general tab
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
        errorText: ''
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
    const [canPullTrailer, setCanPullTrailer] = useState(false);
    const [year, setYear] = useState('');
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
    const [cargoCapacity, setCargoCapacity] = useState('');
    const [cargoCapacityCyds, setCargoCapacityCyds] = useState('');
    const [insurancePolicyNumber, setInsurancePolicyNumber] = useState('');
    const [insuranceValidUntil, setInsuranceValidUntil] = useState(today);
    const [purchaseDate, setPurchaseDate] = useState(today);
    const [purchasePrice, setPurchasePrice] = useState('');
    const [soldDate, setSoldDate] = useState(today);
    const [soldPrice, setSoldPrice] = useState('');
    const [truxTruckId, setTruxTruckId] = useState('');

    // maintenance tab
    const [bedConstruction, setBedConstruction] = useState('');
    const [fuelType, setFuelType] = useState('');
    const [fuelCapacity, setFuelCapacity] = useState('');
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
        truckForEdit
    } = useSelector((state) => ({
        offices: state.OfficeReducer.offices,
        vehicleCategories: state.TruckReducer.vehicleCategories,
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
        if (truckInfo === null && !isEmpty(truckForEdit) && !isEmpty(truckForEdit.result)) {
            const { result } = truckForEdit;
            if (!isEmpty(result)) {
                console.log('result: ', result)
                setTruckInfo(result);
            }
        }
    }, [truckInfo, truckForEdit]);

    const handleChange = (event, newValue) => {
        console.log('newValue: ', newValue);
        setValue(newValue);
    };

    const handleTruckCodeInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        if (inputValue.length <= 64) {
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

    const handleVehicleCategoryIdInputChange = (e) => {
        e.preventDefault();
        
        const inputValue = e.target.value;
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
        setReason({
            ...reason,
            value: e.target.value,
            error: false,
            errorText: ''
        });
    };

    const handleIsApportionedChange = (e) => {
        setIsApportioned(e.target.checked);
    };

    const handleCanPullTrailerChange = (e) => {
        setCanPullTrailer(e.target.checked);
    };

    const handleYearInputChange = (e) => {
        setYear(e.target.value);
    };

    const handleMakeInputChange = (e) => {
        setMake(e.target.value);
    };

    const handleModelInputChange = (e) => {
        setModel(e.target.value);
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
        setVin(e.target.value);
    };

    const handlePlateInputChange = (e) => {
        setPlate(e.target.value);
    };

    const handlePlateExpirationChange = (newDate) => {
        setPlateExpiration(moment(newDate).format('MM/DD/YYYY'));
    };

    const handleCargoCapacityInputChange = (e) => {
        setCargoCapacity(e.target.value);
    };

    const handleCargoCapacityCydsInputChange = (e) => {
        setCargoCapacityCyds(e.target.value);
    };

    const handleInsurancePolicyNumberInputChange = (e) => {
        setInsurancePolicyNumber(e.target.value);
    };

    const handleInsuranceValidUntilChange = (newDate) => {
        setInsuranceValidUntil(moment(newDate).format('MM/DD/YYYY'));
    };

    const handlePurchaseDateChange = (newDate) => {
        setPurchaseDate(moment(newDate).format('MM/DD/YYYY'));
    };

    const handlePurchasePriceInputChange = (e) => {
        setPurchasePrice(e.target.value);
    };

    const handleSoldDateChange = (newDate) => {
        setSoldDate(moment(newDate).format('MM/DD/YYYY'));
    };

    const handleSoldPriceInputChange = (e) => {
        setSoldPrice(e.target.value);
    };

    const handleTruxTruckIdInputChange = (e) => {
        setTruxTruckId(e.target.value);
    };

    const handleBedConstructionInputChange = (e) => {
        setBedConstruction(e.target.value);
    };

    const handleFuelTypeInputChange = (e) => {
        setFuelType(e.target.value);
    };

    const handleFuelCapacityInputChange = (e) => {
        setFuelCapacity(e.target.value);
    };

    const handleSteerTiresInputChange = (e) => {
        setSteerTires(e.target.value);
    };

    const handleDriveAxleTiresInputChange = (e) => {
        setDriveAxleTires(e.target.value);
    }; 
    
    const handleDropAxleTiresInputChange = (e) => {
        setDropAxleTires(e.target.value);
    };

    const handleTrailerTiresInputChange = (e) => {
        setTrailerTires(e.target.value);
    };

    const handleTransmissionInputChange = (e) => {
        setTransmission(e.target.value);
    };

    const handleEngineInputChange = (e) => {
        setEngine(e.target.value);
    };

    const handleRearEndInputChange = (e) => {
        setRearEnd(e.target.value);
    };

    const handleCurrentMileageInputChange = (e) => {
        setCurrentMileage(e.target.value);
    };

    const handleCurrentHoursInputChange = (e) => {
        setCurrentHours(e.target.value);
    }; 

    const handleDtdTrackerDeviceTypeIdInputChange = (e) => {
        setDtdTrackerDeviceTypeId(e.target.value);
    };

    const handleDtdTrackerUniqueIdInputChange = (e) => {
        setDtdTrackerUniqueId(e.target.value);
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
    };

    const renderGeneralForm = () => {
        const { features } = pageConfig;
        return (
            <Stack 
                spacing={2} 
                sx={{
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
                        onChange={handleVehicleCategoryIdInputChange}
                    >
                        <MenuItem value=''>Select an option</MenuItem>

                        { vehicleCategoryOptions && vehicleCategoryOptions.map((option) => (
                            <MenuItem key={option.id} value={option.id}>
                                {option.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                <FormControl
                    disabled={Boolean(truckInfo.vehicleCategoryIsPowered)}
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
                    </Select>
                </FormControl>

                { Boolean(truckInfo.canPullTrailer) && 
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
                        label="Reason" 
                        value={reason.value} 
                        defaultValue={truckInfo.reason}
                        onChange={handleReasonInputChange} 
                        multiline
                        rows={2} 
                        fullWidth
                    />
                }

                { Boolean(truckInfo.vehicleCategoryIsPowered) && 
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
                    type='number' 
                    label='Year'
                    value={year} 
                    defaultValue={truckInfo.year}
                    onChange={handleYearInputChange} 
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
                    value={cargoCapacity} 
                    defaultValue={truckInfo.cargoCapacity}
                    onChange={handleCargoCapacityInputChange}
                    fullWidth 
                />

                <TextField 
                    id='cargoCapacityCyds' 
                    name='cargoCapacityCyds'
                    type='text' 
                    label='Ave Load (cyds)' 
                    value={cargoCapacityCyds} 
                    defaultValue={truckInfo.cargoCapacityCyds}
                    onChange={handleCargoCapacityCydsInputChange}
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
                    maxHeight: '712px',
                    overflowY: 'auto'
                }}
            >
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
                    </Select>
                </FormControl>

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
                    </Select>
                </FormControl>

                <TextField 
                    id='fuelCapacity'
                    name='fuelCapacity'
                    type='text'
                    label='Fuel Capacity'
                    value={fuelCapacity}
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
                        onChange={handleDtdTrackerDeviceTypeIdInputChange}
                    >
                        <MenuItem value=''>Select an option</MenuItem>
                    </Select>
                </FormControl>

                <TextField 
                    id='dtdTrackerDeviceTypeName' 
                    name='dtdTrackerDeviceTypeName'
                    type='hidden' 
                    value={dtdTrackerDeviceTypeName} 
                    defaultValue={truckInfo.dtdTrackerDeviceTypeName} 
                /> 

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
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ 
                    p: 2 
                }} 
            >
                <Typography variant='h6' component='h2'>
                    Create new truck
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            { !isEmpty(pageConfig) && !isEmpty(truckInfo) && 
                <Box sx={{ width: '100%' }}>
                    <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                        <Tabs value={value} onChange={handleChange} aria-label="Create new truck tabs">
                            <Tab label="General" {...a11yProps(0)} />

                            <Tab label="Maintenance" {...a11yProps(1)} />

                            {/* { truckInfo.id > 0 && 
                                <React.Fragment>
                                    <Tab label="Service" {...a11yProps(2)} />
                                    <Tab label="Files" {...a11yProps(3)} />
                                </React.Fragment>
                            } */}

                            <Tab label="GPS Configuration" {...a11yProps(2)} />
                        </Tabs>
                    </Box>

                    <TabPanel value={value} index={0}>
                        {renderGeneralForm()}
                    </TabPanel>

                    <TabPanel value={value} index={1}>
                        {renderMaintenanceForm()}
                    </TabPanel>

                    {/* { truckInfo.id > 0 && 
                        <React.Fragment>
                            <TabPanel value={value} index={2}>
                                {renderServiceForm()}
                            </TabPanel>
                            
                            <TabPanel value={value} index={3}>
                                {renderFilesForm()}
                            </TabPanel>
                        </React.Fragment>
                    } */}

                    <TabPanel value={value} index={2}>
                        {renderGPSConfigurationForm()}
                    </TabPanel>
                </Box>
            }

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
    );
};

export default AddTruckForm;