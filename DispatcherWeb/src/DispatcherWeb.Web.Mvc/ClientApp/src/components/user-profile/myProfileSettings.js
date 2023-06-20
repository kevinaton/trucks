import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Box,
    Button, 
    Checkbox,
    FormGroup, 
    FormControlLabel,
    MenuItem,
    Stack,
    Tabs,
    Tab,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import PropTypes from 'prop-types';
import { getUserProfileSettings } from '../../store/actions';
import { isEmpty } from 'lodash';
import { hostEmailPreference } from '../../common/enums/hostEmailPreference';

const TabPanel = (props) => {
    const { children, value, index, ...other } = props;
  
    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`my-settings-tabpanel-${index}`}
            aria-labelledby={`simple-tab-${index}`}
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
        id: `settings-tab-${index}`,
        'aria-controls': `settings-tabpanel-${index}`,
    };
};

export const MyProfileSettings = ({
    closeModal
}) => {
    const [value, setValue] = useState(0);
    const [profileSettings, setProfileSettings] = useState(null);
    const [name, setName] = useState('');
    const [surname, setSurname] = useState('');
    const [emailAddress, setEmailAddress] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [timezone, setTimezone] = useState('');

    const dispatch = useDispatch();
    const { userProfileSettings } = useSelector((state) => ({
        userProfileSettings: state.UserProfileReducer.userProfileSettings,
    }));

    useEffect(() => {
        dispatch(getUserProfileSettings());
    }, [dispatch]);

    useEffect(() => {
        if (!isEmpty(userProfileSettings) && isEmpty(profileSettings)) {
            const { result } = userProfileSettings;
            if (!isEmpty(result)) {
                setProfileSettings(result);
                setName(result.name);
                setSurname(result.surname);
                setEmailAddress(result.emailAddress);
                setPhoneNumber(result.phoneNumber);
                setTimezone(result.timezone);
            }
        }
    }, [userProfileSettings, profileSettings]);

    const handleChange = (event, newValue) => {
        setValue(newValue);
    };

    const handleNameInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 64) {
            setName(inputValue);
        }
    };

    const handleSurnameInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 64) {
            setSurname(inputValue);
        }
    };

    const handleEmailInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 256) {
            setEmailAddress(inputValue);
        }
    };

    const handlePhoneNumberInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 24) {
            setPhoneNumber(inputValue);
        }
    };

    const handleTimezoneInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 256) {
            setTimezone(inputValue);
        }
    };

    const handleDontShowZeroQuantityWarningChange = (e) => {
        let { options } = profileSettings;
        setProfileSettings({
            ...profileSettings,
            options: {
                ...options,
                dontShowZeroQuantityWarning: e.target.checked,
            }
        })
    };

    const handleCheckboxChange = (e) => {
        const { value, checked } = e.target;
        const { options } = profileSettings;
        const hostEmailPreference = options.hostEmailPreference;
        setProfileSettings({
            ...profileSettings,
            options: {
                ...options,
                hostEmailPreference: !checked 
                    ? hostEmailPreference - parseInt(value) 
                    : hostEmailPreference + parseInt(value)
            }
        });
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };
    
    const handleSave = () => {

    };

    const renderProfileSettings = () => (
        <div>
            <TextField 
                id='name' 
                name='name'
                type='text' 
                value={name} 
                defaultValue={profileSettings.name}
                label={
                    <>
                        Name <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                    </>
                } 
                sx={{ marginBottom: '15px' }} 
                fullWidth 
                onChange={handleNameInputChange}
            />

            <TextField
                id='surname'
                name='surname'
                type='text'
                value={surname} 
                defaultValue={profileSettings.surname}
                label={
                    <>
                        Last name <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                    </>
                } 
                sx={{ marginBottom: '15px' }} 
                fullWidth 
                onChange={handleSurnameInputChange}
            />

            <TextField 
                id='emailAddress'
                name='emailAddress'
                type='email'
                value={emailAddress} 
                defaultValue={profileSettings.emailAddress}
                label={
                    <>
                        Email address <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                    </>
                } 
                sx={{ marginBottom: '15px' }} 
                fullWidth 
                onChange={handleEmailInputChange}
            />

            { profileSettings.canVerifyPhoneNumber && 
                <TextField 
                    id='phoneNumber'
                    name='phoneNumber'
                    type='text'
                    value={phoneNumber}
                    label='Phone number'
                    sx={{ marginBottom: '15px' }} 
                    fullWidth 
                    onChange={handlePhoneNumberInputChange}
                />
            }

            <div style={{ marginBottom: '15px' }}>
                <TextField
                    id='username'
                    name='username'
                    type='text'
                    value={profileSettings.userName}
                    label='Username' 
                    fullWidth 
                    readonly 
                />
                <Typography variant='caption'>
                    Changing the username of the admin is not allowed.
                </Typography>
            </div>

            <TextField
                id="timezone"
                select 
                value={timezone}
                label="Timezone"
                defaultValue={profileSettings.timezone}
                fullWidth 
                onChange={handleTimezoneInputChange}
            >
                {profileSettings.timezoneItems.map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                        {option.displayText}
                    </MenuItem>
                ))}
            </TextField>
        </div>
    );

    const renderTwoFactorLoginSettings = () => (
        <>
            <Typography variant='h6' component='h4'>
                Google Authenticator
            </Typography>

            { profileSettings.isGoogleAuthenticatorEnabled && 
                <>
                    <Typography>Scan this QR code with your mobile app</Typography>
                    <Box>
                        <img src={profileSettings.qrCodeSetupImageUrl} alt='QR code' />
                    </Box>
                    <Typography>Not sure what this screen means? You may need to check this: <a href="https://support.google.com/accounts/answer/1066447" target="_blank" rel="noopener noreferrer">Google Authenticator</a></Typography>
                </>
            }

            <Box sx={{ mt: 1 }}>
                <Button variant='contained'>Enable</Button>
            </Box>
        </>
    );

    const renderOptionsSettings = () => {
        const { options } = profileSettings;

        const hostEmailPreferenceCheckboxes = [
            { value: hostEmailPreference.RELEASE_NOTES, label: 'Release Notes' },
            { value: hostEmailPreference.TRANSACTIONAL, label: 'Transactional Emails' },
            { value: hostEmailPreference.SERVICE_STATUS, label: 'Service Status Emails' },
            { value: hostEmailPreference.MARKETING, label: 'Marketing Emails' }
        ];

        const hasFlag = (flag) => {
            if (options && options.hostEmailPreference) {
                return (options.hostEmailPreference & flag) === flag;
            }
            return false;
        };

        return (
            <>
                <FormGroup sx={{ marginBottom: '15px' }}>
                    <FormControlLabel 
                        control={
                            <Checkbox 
                                checked={options.dontShowZeroQuantityWarning} 
                                onChange={handleDontShowZeroQuantityWarningChange}
                            />
                        } 
                        label="Don't show zero quantity warning" 
                    />

                    <FormControlLabel 
                        control={
                            <Checkbox 
                                checked={options.playSoundForNotifications} 
                            />
                        } 
                        label="Play sound for notifications" 
                    />

                    <FormControlLabel 
                        control={
                            <Checkbox 
                                checked={options.ccMeOnInvoices} 
                            />
                        } 
                        label="CC me on invoices" 
                    />
                </FormGroup>

                <Typography variant='h6' component='h2'>Email subscriptions</Typography>

                <FormGroup>
                    { hostEmailPreferenceCheckboxes.map((checkbox) => (
                        <FormControlLabel 
                            key={checkbox.value}
                            control={
                                <Checkbox 
                                    value={checkbox.value}
                                    checked={hasFlag(checkbox.value)} 
                                    onChange={handleCheckboxChange}
                                />
                            } 
                            label={checkbox.label} 
                        />
                    ))}
                </FormGroup>
            </>
        )
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
                    My settings
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            { !isEmpty(profileSettings) && 
                <Box 
                    component='form'
                    noValidate
                    autoComplete="off"
                >
                    <Box sx={{ width: '100%' }}>
                        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                            <Tabs value={value} onChange={handleChange} aria-label="my settings tabs">
                                <Tab label="Profile" {...a11yProps(0)} />
                                <Tab label="Two Factor Login" {...a11yProps(1)} />
                                <Tab label="Options" {...a11yProps(2)} />
                            </Tabs>
                        </Box>
                        <TabPanel value={value} index={0}>
                            {renderProfileSettings()}
                        </TabPanel>
                        <TabPanel value={value} index={1}>
                            {renderTwoFactorLoginSettings()}
                        </TabPanel>
                        <TabPanel value={value} index={2}>
                            {renderOptionsSettings()}
                        </TabPanel>
                    </Box>

                    <Box sx={{ p: 2 }}>
                        <Stack 
                            spacing={2}
                            direction='row' 
                            justifyContent='flex-end'
                        >
                            <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
                            <Button variant='contained' color='primary' onClick={handleSave}>
                                <i className='fa-regular fa-save' style={{ marginRight: '6px' }}></i>
                                <Typography>Save</Typography>
                            </Button>
                        </Stack>
                    </Box>
                </Box>
            }
        </React.Fragment>
    )
};