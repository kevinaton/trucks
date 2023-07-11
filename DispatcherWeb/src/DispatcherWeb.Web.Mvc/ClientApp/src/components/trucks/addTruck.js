import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import PropTypes from 'prop-types';
import {
    Box,
    Button,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    Stack,
    Tabs,
    Tab,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { isEmpty } from 'lodash';
import { getVehicleCategories } from '../../store/actions';

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
    const [value, setValue] = useState(0);
    const [officeOptions, setOfficeOptions] = useState(null);
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState(null);

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
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [defaultDriverId, setDefaultDriverId] = useState(null);
    const [defaultTrailerId, setDefaultTrailerId] = useState(null);
    const [isActive, setIsActive] = useState(true);
    const [inactivationDate, setInactivationDate] = useState(null);
    const [isOutOfService, setIsOutOfService] = useState(false);
    const [reason, setReason] = useState('');
    const [isApportioned, setIsApportioned] = useState(false);
    const [canPullTrailer, setCanPullTrailer] = useState(false);
    const [year, setYear] = useState('');
    const [make, setMake] = useState('');
    const [model, setModel] = useState('');
    const [inServiceDate, setInServiceDate] = useState('');
    const [vin, setVin] = useState('');
    const [plate, setPlate] = useState('');
    const [plateExpiration, setPlateExpiration] = useState('');
    const [cargoCapacity, setCargoCapacity] = useState('');
    const [cargoCapacityCyds, setCargoCapacityCyds] = useState('');
    const [insurancePolicyNumber, setInsurancePolicyNumber] = useState('');
    const [insuranceValidUntil, setInsuranceValidUntil] = useState('');
    const [purchaseDate, setPurchaseDate] = useState('');
    const [purchasePrice, setPurchasePrice] = useState('');
    const [soldDate, setSoldDate] = useState('');
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

    // gps configuration tab
    const [dtdTrackerDeviceTypeId, setDtdTrackerDeviceTypeId] = useState(null);
    const [dtdTrackerDeviceTypeName, setDtdTrackerDeviceTypeName] = useState('');
    const [dtdTrackerServerAddress, setDtdTrackerServerAddress] = useState('');
    const [dtdTrackerUniqueId, setDtdTrackerUniqueId] = useState('');
    const [dtdTrackerPassword, setDtdTrackerPassword] = useState('');

    const dispatch = useDispatch();
    const { 
        offices,
        vehicleCategories
    } = useSelector((state) => ({
        offices: state.OfficeReducer.offices,
        vehicleCategories: state.TruckReducer.vehicleCategories
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
        if (vehicleCategoryOptions === null) {
            dispatch(getVehicleCategories());
        }
    }, [dispatch, vehicleCategoryOptions]);

    useEffect(() => {
        if (!isEmpty(vehicleCategories) && !isEmpty(vehicleCategories.result)) {
            const { result } = vehicleCategories;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setVehicleCategoryOptions(result.items);
            }
        }
    }, [vehicleCategories]);

    const handleChange = (event, newValue) => {
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
            <div>
                <TextField 
                    id='truck-code'
                    name='truck-code' 
                    type='text' 
                    value={truckCode.value} 
                    error={truckCode.error} 
                    helperText={truckCode.error ? truckCode.errorText : ''} 
                    defaultValue={truckCode.value} 
                    label={
                        <>
                            Truck Code {truckCode.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                        </>
                    }
                    sx={{ marginBottom: '15px' }} 
                    fullWidth 
                    onChange={handleTruckCodeInputChange}
                />

                { features.allowMultiOffice &&
                    <FormControl 
                        fullWidth
                        sx={{ marginBottom: '15px' }} 
                    >
                        <InputLabel id='officeId-label'>
                            Office {officeId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                        </InputLabel>
                        <Select
                            labelId='officeId-label'
                            id='officeId'
                            value={officeId.value} 
                            label={
                                <>
                                    Office {officeId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                                </>
                            }
                            onChange={handleOfficeIdInputChange}
                        >
                            <MenuItem value=''>
                                Select an option
                            </MenuItem>
                            
                            { officeOptions && officeOptions.map((option) => (
                                <MenuItem key={option.id} value={option.id}>
                                    {option.name}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                }

                <FormControl  
                    fullWidth
                    sx={{ marginBottom: '15px' }} 
                >
                    <InputLabel id='vehicleCategoryId-label'>
                        Category {vehicleCategoryId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                    </InputLabel>
                    <Select
                        labelId='vehicleCategoryId-label'
                        id='vehicleCategoryId'
                        value={vehicleCategoryId.value}
                        label={
                            <>
                                Category {vehicleCategoryId.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                            </>
                        }
                        onChange={handleVehicleCategoryIdInputChange}
                    >
                        <MenuItem value=''>
                            Select an option
                        </MenuItem>

                        { vehicleCategoryOptions && vehicleCategoryOptions.map((option) => (
                            <MenuItem key={option.id} value={option.id}>
                                {option.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </div>
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

            <Box 
                component='form'
                noValidate 
                autoComplete='off'
            >
                <Box sx={{ width: '100%' }}>
                    <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                        <Tabs value={value} onChange={handleChange} aria-label="Create new truck tabs">
                            <Tab label="General" {...a11yProps(0)} />
                            <Tab label="Maintenance" {...a11yProps(1)} />
                            <Tab label="GPS Configuration" {...a11yProps(2)} />
                        </Tabs>
                    </Box>
                    <TabPanel value={value} index={0}>
                        {!isEmpty(pageConfig) && renderGeneralForm()}
                    </TabPanel>
                    <TabPanel value={value} index={1}>
                        
                    </TabPanel>
                    <TabPanel value={value} index={2}>
                        
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
            </Box>
        </React.Fragment>
    );
};

export default AddTruckForm;