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
import { 
    getUserProfileSettings, 
    updateUserProfile as onUpdateUserProfile,
    resetUpdateUserProfile as onResetUpdateUserProfile,
    enableGoogleAuthenticator as onEnableGoogleAuthenticator,
} from '../../store/actions';
import { isEmpty } from 'lodash';
import { useSnackbar } from 'notistack';
import { hostEmailPreference } from '../../common/enums/hostEmailPreference';
import { isValidEmail } from '../../utils';

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
    const [name, setName] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [surname, setSurname] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [emailAddress, setEmailAddress] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });
    const [phoneNumber, setPhoneNumber] = useState({
        value: '',
        error: false,
        errorText: ''
    });
    const [timezone, setTimezone] = useState('');
    const [options, setOptions] = useState(null);

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const { 
        userProfileSettings,
        profileUpdateSuccess
    } = useSelector((state) => ({
        userProfileSettings: state.UserProfileReducer.userProfileSettings,
        profileUpdateSuccess: state.UserProfileReducer.profileUpdateSuccess
    }));

    useEffect(() => {
        dispatch(getUserProfileSettings());
    }, [dispatch]);

    useEffect(() => {
        if (!isEmpty(userProfileSettings) && !isEmpty(userProfileSettings.result)) {
            if (isEmpty(profileSettings)) {
                const { result } = userProfileSettings;
                setProfileSettings(result);

                const { name, surname, emailAddress, phoneNumber, timezone, options } = result;

                setName({
                    ...name,
                    value: name
                });

                setSurname({
                    ...surname,
                    value: surname
                });

                setEmailAddress({
                    ...emailAddress,
                    value: emailAddress
                });

                setPhoneNumber({
                    ...phoneNumber,
                    value: phoneNumber
                });

                setTimezone(timezone);
                setOptions(options);
            } else {
                const { result } = userProfileSettings;
                const { 
                    name,
                    surname,
                    emailAddress,
                    phoneNumber,
                    timezone,
                    options,
                    isGoogleAuthenticatorEnabled, 
                    qrCodeSetupImageUrl 
                } = result;

                setProfileSettings((prevSettings) => {
                    let updatedSettings = { ...prevSettings };
                
                    // Check and update each value individually
                    if (name !== prevSettings.name) {
                        updatedSettings.name = name;
                    }

                    if (surname !== prevSettings.surname) {
                        updatedSettings.surname = surname;
                    }

                    if (emailAddress !== prevSettings.emailAddress) {
                        updatedSettings.emailAddress = emailAddress;
                    }

                    if (phoneNumber !== prevSettings.phoneNumber) {
                        updatedSettings.phoneNumber = phoneNumber;
                    }

                    if (timezone !== prevSettings.timezone) {
                        updatedSettings.timezone = timezone;
                    }

                    if (options !== prevSettings.options) {
                        updatedSettings.options = options;
                    }

                    if (isGoogleAuthenticatorEnabled !== prevSettings.isGoogleAuthenticatorEnabled) {
                        updatedSettings.isGoogleAuthenticatorEnabled = isGoogleAuthenticatorEnabled;
                    }

                    if (qrCodeSetupImageUrl !== prevSettings.qrCodeSetupImageUrl) {
                        updatedSettings.qrCodeSetupImageUrl = qrCodeSetupImageUrl;
                    }
                
                    // Update the state if there are any changes
                    if (Object.keys(updatedSettings).length > 0) {
                        return updatedSettings;
                    }
                
                    // If there are no changes, return the previous state
                    return prevSettings;
                });
            }
        }
    }, [userProfileSettings, profileSettings, name, surname, emailAddress, phoneNumber]);

    useEffect(() => {
        if (profileUpdateSuccess) {
            closeModal();
            enqueueSnackbar('Saved successfully', { variant: 'success' });
            dispatch(onResetUpdateUserProfile());
        }
    }, [dispatch, profileUpdateSuccess, enqueueSnackbar, closeModal]);

    const handleChange = (event, newValue) => {
        setValue(newValue);
    };

    const handleNameInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 64) {
            setName({
                ...name,
                value: inputValue,
                error: false,
                errorText: ''
            });
        }
    };

    const handleSurnameInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 64) {
            setSurname({
                ...surname,
                value: inputValue,
                error: false,
                errorText: ''
            });
        }
    };

    const handleEmailInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 256) {
            setEmailAddress({
                ...emailAddress,
                value: inputValue,
                error: false,
                errorText: ''
            });
        }
    };

    const handlePhoneNumberInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 24) {
            setPhoneNumber({
                ...phoneNumber,
                value: inputValue
            });
        }
    };

    const handleTimezoneInputChange = (e) => {
        const inputValue = e.target.value;
        if (inputValue.length <= 256) {
            setTimezone(inputValue);
        }
    };

    const handleDontShowZeroQuantityWarningChange = (e) => {
        setOptions({
            ...options,
            dontShowZeroQuantityWarning: e.target.checked,
        });
    };

    const handlePlaySoundForNotificationsChange = (e) => {
        setOptions({
            ...options,
            playSoundForNotifications: e.target.checked,
        });
    };

    const handleCcMeOnInvoicesChange = (e) => {
        setOptions({
            ...options,
            ccMeOnInvoices: e.target.checked,
        });
    };

    const handleCheckboxChange = (e) => {
        const { value, checked } = e.target;
        const hostEmailPreference = options.hostEmailPreference;
        setOptions({
            ...options,
            hostEmailPreference: !checked 
                ? hostEmailPreference - parseInt(value) 
                : hostEmailPreference + parseInt(value)
        });
    };

    const handleEnableGoogleAuthenticator = () => {
        dispatch(onEnableGoogleAuthenticator());
    };

    // const handleDisableGoogleAuthenticator = () => {
    //     dispatch(onDisableGoogleAuthenticator());
    // };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };
    
    const handleSave = (e) => {
        e.preventDefault();

        // check if name is valid
        if (!name.value) {
            setName({
                ...name,
                error: true,
                errorText: 'Name is required'
            });
        }

        // check if surname is valid
        if (!surname.value) {
            setSurname({
                ...surname,
                error: true,
                errorText: 'Surname is required'
            });
        }

        // check if email address is valid
        if (!emailAddress.value) {
            setEmailAddress({
                ...emailAddress,
                error: true,
                errorText: 'Email address is required'
            });
        }

        if (!isValidEmail(emailAddress.value)) {
            setEmailAddress({
                ...emailAddress,
                error: true,
                errorText: 'Email address is invalid'
            });
        }

        const { ...profileSettingsData } = profileSettings;
        const newProfileSettings = {
            ...profileSettingsData,
            name: name.value,
            surname: surname.value,
            emailAddress: emailAddress.value, 
            phoneNumber: phoneNumber.value,
            timezone: timezone,
            options: options
        };
        dispatch(onUpdateUserProfile(newProfileSettings));
    };

    const renderProfileSettings = () => (
        <div>
            <TextField 
                id='name' 
                name='name'
                type='text' 
                value={name.value} 
                error={name.error} 
                helperText={name.error ? name.errorText : ''}
                defaultValue={profileSettings.name}
                label={
                    <>
                        Name {name.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
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
                value={surname.value} 
                error={surname.error}
                helperText={surname.error ? surname.errorText : ''} 
                defaultValue={profileSettings.surname}
                label={
                    <>
                        Last name {surname.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
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
                value={emailAddress.value}
                error={emailAddress.error} 
                helperText={emailAddress.error ? emailAddress.errorText : ''} 
                defaultValue={profileSettings.emailAddress}
                label={
                    <>
                        Email address {emailAddress.required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
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
                    value={phoneNumber.value} 
                    error={phoneNumber.error} 
                    helperText={phoneNumber.error ? phoneNumber.errorText : ''}
                    defaultValue={profileSettings.phoneNumber}
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
                    <Box display='flex' alignItems='center' justifyContent='center'>
                        <img src={profileSettings.qrCodeSetupImageUrl} alt='QR code' />
                    </Box>
                    <Typography>Not sure what this screen means? You may need to check this: <a href="https://support.google.com/accounts/answer/1066447" target="_blank" rel="noopener noreferrer">Google Authenticator</a></Typography>
                </>
            }

            { !profileSettings.isGoogleAuthenticatorEnabled &&
                <Box sx={{ mt: 1 }}>
                    <Button variant='contained' onClick={handleEnableGoogleAuthenticator}>Enable</Button>
                </Box>
            }

            {/* { profileSettings.isGoogleAuthenticatorEnabled &&
                <Box sx={{ mt: 1 }}>
                    <Button variant='contained' onClick={handleDisableGoogleAuthenticator}>Disable</Button>
                </Box>
            } */}
        </>
    );

    const renderOptionsSettings = () => {
        if (isEmpty(options)){
            return (<></>);
        }
        
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
                                onChange={handlePlaySoundForNotificationsChange}
                            />
                        } 
                        label="Play sound for notifications" 
                    />

                    <FormControlLabel 
                        control={
                            <Checkbox 
                                checked={options.ccMeOnInvoices} 
                                onChange={handleCcMeOnInvoicesChange}
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
                            <Button variant='contained' color='primary' onClick={(e) => handleSave(e)}>
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