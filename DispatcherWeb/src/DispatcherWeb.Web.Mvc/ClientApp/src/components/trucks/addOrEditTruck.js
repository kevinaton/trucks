import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import PropTypes from 'prop-types';
import moment from 'moment';
import {
    Autocomplete,
    Box,
    Button,
    Checkbox,
    FormControl,
    FormControlLabel,
    IconButton,
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
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import { isEmpty } from 'lodash';
import { 
    getVehicleCategories, 
    getDriversSelectList,
    getActiveTrailersSelectList,
    getBedConstructionSelectList,
    getFuelTypeSelectList, 
    getWialonDeviceTypesSelectList,
    getTruckForEdit,
    editTruck as onEditTruck,
    resetEditTruck as onResetEditTruck
} from '../../store/actions';
import { assetType } from '../../common/enums/assetType';
import AddOrEditDriver from '../drivers/addOrEditDriver';
import { AlertDialog } from '../common/dialogs';
import { renderDate } from '../../helpers/misc_helper';
import { getDefaultVal, formatDate } from '../../utils';

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
    openModal,
    closeModal,
    openDialog
}) => {
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
        value: null,
        required: false,
        error: false,
        errorText: '',
        disabled: true
    });
    const [currentTrailerId, setCurrentTrailerId] = useState(null);
    const [isActive, setIsActive] = useState(true);
    const [inactivationDate, setInactivationDate] = useState({
        value: null,
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
    const [year, setYear] = useState({
        value: truckInfo != null ? truckInfo.year : '',
        error: false,
        errorText: ''
    });
    const [make, setMake] = useState('');
    const [model, setModel] = useState('');
    const [inServiceDate, setInServiceDate] = useState({
        value: null,
        required: true,
        error: false,
        errorText: ''
    });
    const [vin, setVin] = useState('');
    const [plate, setPlate] = useState('');
    const [plateExpiration, setPlateExpiration] = useState(null);
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
    const [insuranceValidUntil, setInsuranceValidUntil] = useState(null);
    const [purchaseDate, setPurchaseDate] = useState(null);
    const [purchasePrice, setPurchasePrice] = useState('');
    const [soldDate, setSoldDate] = useState(null);
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

    // State to store the newly added option
    const [newOption, setNewOption] = useState('');

    const dispatch = useDispatch();
    const { 
        offices,
        vehicleCategories, 
        driversSelectList,
        activeTrailersSelectList,
        bedConstructionSelectList,
        fuelTypeSelectList,
        wialonDeviceTypesSelectList,
        truckForEdit,
        editTruckSuccess,
        error
    } = useSelector((state) => ({
        offices: state.OfficeReducer.offices,
        vehicleCategories: state.TruckReducer.vehicleCategories, 
        driversSelectList: state.DriverReducer.driversSelectList,
        activeTrailersSelectList: state.TruckReducer.activeTrailersSelectList,
        bedConstructionSelectList: state.TruckReducer.bedConstructionSelectList,
        fuelTypeSelectList: state.TruckReducer.fuelTypeSelectList, 
        wialonDeviceTypesSelectList: state.TruckReducer.wialonDeviceTypesSelectList,
        truckForEdit: state.TruckReducer.truckForEdit,
        editTruckSuccess: state.TruckReducer.editTruckSuccess,
        error: state.TruckReducer.error
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
                // set default
                setBedConstruction(result[0].key)
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

                if (result.defaultDriverId !== null) {
                    setDefaultDriverId({
                        ...defaultDriverId,
                        value: result.defaultDriverId
                    });
                }

                if (result.vehicleCategoryAssetType === assetType.DUMP_TRUCK || result.vehicleCategoryAssetType === assetType.TRAILER) {
                    setShowBedConstruction(true);
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
                });

                if (result.inServiceDate !== null) {
                    setInServiceDate({
                        ...inServiceDate,
                        value: result.inServiceDate
                    });
                }
            }
        }
    }, [truckInfo, truckForEdit, defaultDriverId, inServiceDate, showBedConstruction]);

    useEffect(() => {
        if (editTruckSuccess) {
            dispatch(onResetEditTruck());
            closeModal();
        }
    }, [dispatch, editTruckSuccess, closeModal]);

    useEffect(() => {
        console.log('error: ', error)
        if (!isEmpty(error) && !error.success) {
            const { message } = error.error;

            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog variant='error' message={message} />
                )
            });
        }
    }, [error, openDialog]);

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
            setCanPullTrailer(assetType === assetType.TRACTOR);
        }
        
        const shouldDisableDefaultDriver = isPowered !== true;
        if (shouldDisableDefaultDriver) {
            setDefaultDriverId({
                ...defaultDriverId,
                value: '',
                disabled: shouldDisableDefaultDriver
            });
        } else {
            setDefaultDriverId({
                ...defaultDriverId,
                disabled: shouldDisableDefaultDriver
            });
        }
        
        const inputValue = selectedOption.id;
        setVehicleCategoryId({
            ...vehicleCategoryId,
            value: inputValue,
            error: false,
            errorText: ''
        });
    };

    const handleDefaultDriverIdInputChange = (e, newValue) => {
        e.preventDefault();

        const inputValue = newValue;
        setDefaultDriverId({
            ...defaultDriverId,
            value: inputValue !== null ? inputValue.id : '',
            error: false,
            errorText: ''
        });
    };

    const handleDefaultTrailerIdInputChange = (e) => {
        e.preventDefault();

        const inputValue = e.target.value;
        setCurrentTrailerId({
            ...currentTrailerId,
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
            id: getDefaultVal(id, ''),
            vehicleCategoryIsPowered: vehicleCategoryIsPowered.toString(),
            vehicleCategoryAssetType: vehicleCategoryAssetType.toString(),
            truckCode: truckCode.value.toString(),
            officeId: officeId.value.toString(),
            vehicleCategoryId: vehicleCategoryId.value,
            defaultDriverId: defaultDriverId.value,
            currentTrailerId: getDefaultVal(currentTrailerId, ''),
            isActive: isActive.toString(),
            inactivationDate: formatDate(inactivationDate.value),
            isOutOfService: isOutOfService.toString(),
            reason: reason.value,
            isApportioned: isApportioned.toString(),
            year: year.value.toString(),
            make,
            model,
            inServiceDate: formatDate(inServiceDate.value),
            vin,
            plate,
            plateExpiration: formatDate(plateExpiration),
            cargoCapacity: cargoCapacity.value.toString(),
            cargoCapacityCyds: cargoCapacityCyds.value.toString(),
            insurancePolicyNumber: insurancePolicyNumber.toString(),
            insuranceValidUntil: formatDate(insuranceValidUntil),
            purchaseDate: formatDate(purchaseDate),
            purchasePrice: purchasePrice.toString(),
            soldDate: formatDate(soldDate),
            soldPrice: soldPrice.toString(),
            truxTruckId: truxTruckId.toString(),
            bedConstruction: bedConstruction.toString(),
            fuelType: fuelType.toString(),
            fuelCapacity: fuelCapacity.value.toString(),
            steerTires: steerTires.toString(),
            driveAxleTires: driveAxleTires.toString(),
            dropAxleTires: dropAxleTires.toString(),
            trailerTires: trailerTires.toString(),
            transmission: transmission.toString(),
            engine: engine.toString(),
            rearEnd: rearEnd.toString(),
            dtdTrackerDeviceTypeId: getDefaultVal(dtdTrackerDeviceTypeId, ''),
            dtdTrackerDeviceTypeName: dtdTrackerDeviceTypeName.toString(),
            dtdTrackerServerAddress: dtdTrackerServerAddress.toString(),
            dtdTrackerUniqueId: dtdTrackerUniqueId.toString(),
            dtdTrackerPassword: dtdTrackerPassword.toString(),
            files
        };

        if (canPullTrailer) {
            data.canPullTrailer = canPullTrailer;
        }

        dispatch(onEditTruck(data));
    };

    // Handler to add a new option
    const handleAddOption = () => {
        if (newOption.trim() !== '') {
            const newOptionValue = newOption.trim().toLowerCase();

            openModal(
                <AddOrEditDriver 
                    name={newOptionValue}
                    closeModal={closeModal}
                />,
                500
            );

            // TODO: add new option to dropdown
            //   // Check if the option already exists in the options array
            //   const optionExists = options.some((opt) => opt.value === newOptionValue);
            //   if (!optionExists) {
            //     setOptions((prevOptions) => [
            //       ...prevOptions,
            //       { label: newOption, value: newOptionValue },
            //     ]);
            //   }
            //   setNewOption(''); // Clear the input field after adding the option
        }
    };

    const renderGeneralForm = () => {
        const { features } = pageConfig;
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    paddingBottom: '8px',
                    maxHeight: 'calc(100vh - 300px)',
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
                    autoFocus
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

                <FormControl fullWidth>
                    <Autocomplete
                        options={defaultDriverOptions}
                        getOptionLabel={(option) => option.name}
                        renderOption={(props, option) => (
                            <li {...props}>
                                {option.name}
                            </li>
                        )}
                        onChange={(event, newValue) => handleDefaultDriverIdInputChange(event, newValue)}
                        renderInput={(params) => (
                            <div>
                                <TextField
                                    {...params}
                                    label="Default Driver"
                                    variant="outlined"
                                    value={newOption}
                                    onChange={(e) => {
                                        setNewOption(e.target.value)
                                    }} 
                                    emptyLabel='' 
                                    InputProps={{
                                        ...params.InputProps,
                                        endAdornment: (
                                            <React.Fragment>
                                                {params.InputProps.endAdornment}
                                                <IconButton
                                                    onClick={handleAddOption}
                                                    disabled={newOption.trim() === ''}
                                                >
                                                    <AddCircleOutlineIcon />
                                                </IconButton>
                                            </React.Fragment>
                                        ),
                                    }}
                                />
                            </div>
                        )}
                        disabled={defaultDriverId.disabled}
                    />
                </FormControl>

                { canPullTrailer !== null && canPullTrailer && 
                    <FormControl fullWidth>
                        <InputLabel id='defaultTrailer-label'>Default Trailer</InputLabel>
                        <Select
                            labelId='defaultTrailer-label'
                            id='defaultTrailer'
                            label='Default Trailer' 
                            value={defaultDriverId.value} 
                            defaultValue={truckInfo.currentTrailerId}
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
                    <DatePicker 
                        id='inactivationDate'
                        name='inactivationDate'
                        label={
                            <>
                                Inactivation Date <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                            </>
                        } 
                        value={inactivationDate.value !== null ? renderDate(inactivationDate.value) : moment()} 
                        emptyLabel='' 
                        onChange={handleInactivationDateChange} 
                        error={inactivationDate.error}
                        helperText={inactivationDate.error ? inactivationDate.errorText : ''} 
                        sx={{ flexShrink: 0 }} 
                    />
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

                <DatePicker 
                    id='inServiceDate'
                    name='inServiceDate'
                    label={
                        <>
                            In Service Date <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                        </>
                    } 
                    value={inServiceDate.value !== null ? renderDate(inServiceDate.value) : moment()} 
                    emptyLabel=''
                    onChange={handleInServiceDateChange} 
                    error={inServiceDate.error}
                    helperText={inServiceDate.error ? inServiceDate.errorText : ''} 
                    sx={{ flexShrink: 0 }} 
                    fullWidth
                />

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

                <Stack direction='row' spacing={2}>
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

                    <DatePicker 
                        id='plateExpiration'
                        name='plateExpiration'
                        label='Plate Expiration'
                        value={plateExpiration !== null ? renderDate(plateExpiration) : null } 
                        emptyLabel=''
                        onChange={handlePlateExpirationChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />
                </Stack>

                <Stack direction='row' spacing={2}>
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
                </Stack>

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

                <DatePicker 
                    id='insuranceValidUntil'
                    name='insuranceValidUntil'
                    label='Insurance Valid Until'
                    value={insuranceValidUntil !== null ? renderDate(insuranceValidUntil) : null} 
                    emptyLabel=''
                    onChange={handleInsuranceValidUntilChange} 
                    sx={{ flexShrink: 0 }} 
                    fullWidth
                />

                <Stack direction='row' spacing={2}>
                    <DatePicker 
                        id='purchaseDate'
                        name='purchaseDate'
                        label='Purchase Date'
                        value={purchaseDate !== null ? renderDate(purchaseDate) : null} 
                        emptyLabel=''
                        onChange={handlePurchaseDateChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />

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
                </Stack>

                <Stack direction='row' spacing={2}>
                    <DatePicker 
                        id='soldDate'
                        name='soldDate'
                        label='Sold Date'
                        value={soldDate !== null ? renderDate(soldDate) : null} 
                        emptyLabel=''
                        onChange={handleSoldDateChange} 
                        sx={{ flexShrink: 0 }} 
                        fullWidth
                    />

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
                </Stack>

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
                    paddingBottom: '8px',
                    maxHeight: 'calc(100vh - 300px)',
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
        // TODO: edit truck functionality
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    paddingBottom: '8px',
                    maxHeight: 'calc(100vh - 300px)',
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
        // TODO: edit truck functionality
        return (
            <Stack 
                spacing={2} 
                sx={{
                    paddingTop: '8px',
                    paddingBottom: '8px',
                    maxHeight: 'calc(100vh - 300px)',
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
                    paddingBottom: '8px',
                    maxHeight: 'calc(100vh - 300px)',
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
                        sx={{ p: 2 }} 
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
                        <LocalizationProvider 
                            dateAdapter={AdapterMoment} 
                            adapterLocale={moment.locale()}
                            fullWidth
                        >
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

export default AddOrEditTruckForm;