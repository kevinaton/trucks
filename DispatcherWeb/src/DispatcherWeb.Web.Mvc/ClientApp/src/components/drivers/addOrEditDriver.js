import React, { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import PropTypes from 'prop-types';
import {
    Autocomplete,
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
import { getDriverForEdit } from '../../store/actions';

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

const AddOrEditDriver = ({
    name,
    closeModal
}) => {
    const today = moment();
    const [value, setValue] = useState(0);
    const [driverInfo, setDriverInfo] = useState(null);
    const [id, setId] = useState(null);

    // general tab
    const [firstName, setFirstName] = useState({
        value: name, 
        required: true,
        error: false,
        errorText: ''
    });
    const [lastName, setLastName] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [officeId, setOfficeId] = useState({
        value: null,
        required: true,
        error: false,
        errorText: ''
    });
    const [inActive, setInactive] = useState(false);
    const [emailAddress, setEmailAddress] = useState('');
    const [cellPhoneNumber, setCellPhoneNumber] = useState('');
    const [orderNotifyPreferredFormat, setOrderNotifyPreferredFormat] = useState(0);
    const [address, setAddress] = useState('');
    const [city, setCity] = useState('');
    const [state, setState] = useState('');
    const [zipCode, setZipCode] = useState('');

    // status tab
    const [licenseNumber, setLicenseNumber] = useState('');
    const [typeOfLicense, setTypeOfLicense] = useState(null);
    const [licenseExpirationDate, setLicenseExpirationDate] = useState(null);
    const [lastPhysicalDate, setLastPhysicalDate] = useState(null);
    const [nextPhysicalDueDate, setNextPhysicalDueDate] = useState(null);
    const [lastMvrDate, setLastMvrDate] = useState(null);
    const [dateOfHire, setDateOfHire] = useState(null);
    const [terminationDate, setTerminationDate] = useState(null);
    
    const dispatch = useDispatch();
    const {
        driverForEdit
    } = useSelector((state) => ({
        driverForEdit: state.DriverReducer.driverForEdit
    }));

    useEffect(() => {
        dispatch(getDriverForEdit());
    }, []);

    useEffect(() => {
        if (driverInfo === null && !isEmpty(driverForEdit) && !isEmpty(driverForEdit.result)) {
            const { result } = driverForEdit;
            if (!isEmpty(result)) {
                setDriverInfo(result);
                setId(result.id);
            }
        }
    }, [driverInfo, driverForEdit]);
    
    // handle change tab
    const handleChange = (event, newValue) => {
        setValue(newValue);
    };

    const handleFirstNameInputChange = (e) => {
        const { value } = e.target;
        setFirstName({
            value: value,
            error: false,
            helperText: ''
        });
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = () => {
        // Save the form
        closeModal();
    };
    
    const renderGeneralForm = () => {
        return (
            <Stack spacing={2} sx={{
                paddingTop: '8px',
                paddingBottom: '8px'
            }}>
                <TextField 
                    id='firstName'
                    name='firstName' 
                    type='text' 
                    label={
                        <>
                            First Name { firstName.required && <span style={{ color: 'red' }}>*</span> }
                        </>
                    } 
                    value={firstName.value} 
                    defaultValue={driverInfo.firstName} 
                    onChange={handleFirstNameInputChange} 
                    error={firstName.error}
                    helperText={firstName.errorText}
                    fullWidth
                />

                <TextField 
                    id='lastName'
                    name='lastName'
                    type='text'
                    label={
                        <>
                            Last Name { lastName.required && <span style={{ color: 'red' }}>*</span> }
                        </>
                    } 
                    value={lastName.value} 
                    defaultValue={driverInfo.lastName} 
                    error={lastName.error}
                    helperText={lastName.errorText}
                    fullWidth
                />

                <FormControl fullWidth>
                    <InputLabel id='officeId-label'>
                        Office { officeId.required && <span style={{ color: 'red' }}>*</span> }
                    </InputLabel> 
                    <Select
                        labelId='officeId-label' 
                        id='officeId'
                        label={
                            <>
                                Office { officeId.required && <span style={{ color: 'red' }}>*</span> }
                            </>
                        } 
                        value={officeId.value}
                        defaultValue={driverInfo.officeId}
                        error={officeId.error}
                        helperText={ officeId.error ? officeId.errorText : ''}
                    >
                        <MenuItem value=''>Select an option</MenuItem>
                    </Select>
                </FormControl>

                <FormControlLabel 
                    control={
                        <Checkbox 
                            checked={inActive}
                            defaultChecked={driverInfo.inActive}
                        />
                    } 
                    label='Inactive'
                    fullWidth 
                />

                <TextField 
                    id='emailAddress'
                    name='emailAddress' 
                    type='email'
                    label='Email Address' 
                    value={emailAddress} 
                    defaultValue={driverInfo.emailAddress}
                    fullWidth
                />

                <TextField 
                    id='cellPhoneNumber'
                    name='cellPhoneNumber'
                    type='text'
                    label='Cell Phone Number' 
                    value={cellPhoneNumber} 
                    defaultValue={driverInfo.cellPhoneNumber} 
                    fullWidth 
                />

                <FormControl fullWidth>
                    <InputLabel id='orderNotifyPreferredFormat-label'>
                        Preferred Format
                    </InputLabel>
                    <Select
                        labelId='orderNotifyPreferredFormat-label'
                        id='orderNotifyPreferredFormat'
                        label='Preferred Format'
                        value={orderNotifyPreferredFormat}
                        defaultValue={driverInfo.orderNotifyPreferredFormat}
                    >
                    </Select>
                </FormControl>

                <TextField 
                    id='address'
                    name='address'
                    type='text'
                    label='Address'
                    value={address}
                    defaultValue={driverInfo.address}
                />

                <TextField 
                    id='city'
                    name='city'
                    type='text'
                    label='City'
                    value={city}
                    defaultValue={driverInfo.city}
                />

                <TextField 
                    id='state'
                    name='state'
                    type='text'
                    label='State'
                    value={state}
                    defaultValue={driverInfo.state}
                />

                <TextField 
                    id='zipCode'
                    name='zipCode'
                    type='text'
                    label='Zip Code'
                    value={zipCode}
                    defaultValue={driverInfo.zipCode}
                />
            </Stack>
        );
    };

    const renderStatusForm = () => (<></>);

    const renderPayForm = () => (<></>);

    return (
        <React.Fragment>
            { !isEmpty(driverInfo) && 
                <React.Fragment>
                    <Box
                        display='flex'
                        justifyContent='space-between' 
                        alignItems='center'
                        sx={{ p: 2 }}
                    >
                        <Typography variant='h6' component='h2'>
                            { driverInfo.id > 0 ? <>Edit Driver</> : <>Create New Driver</> }
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
                            <Tabs value={value} onChange={handleChange} aria-label="Create new driver tabs">
                                <Tab label="General" {...a11yProps(0)} />
                                <Tab label="Status" {...a11yProps(1)} />
                                <Tab label="Pay" {...a11yProps(2)} />
                            </Tabs>
                        </Box>
        
                        <TabPanel value={value} index={0}>
                            {renderGeneralForm()}
                        </TabPanel>
        
                        <TabPanel value={value} index={1}>
                            {renderStatusForm()}
                        </TabPanel>
        
                        <TabPanel value={value} index={2}>
                            {renderPayForm()}
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
}

export default AddOrEditDriver;