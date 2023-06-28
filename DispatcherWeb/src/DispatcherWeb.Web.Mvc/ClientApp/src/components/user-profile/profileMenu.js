import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Avatar,
    Button,
    Box,
    Link,
    Menu,
    MenuItem,
    MenuList,
    Paper,
    Typography,
} from '@mui/material';
import { isEmpty } from 'lodash';
import { theme } from '../../Theme';
import { 
    getUserProfileMenu, 
    getLinkedUsers, 
    downloadCollectedData as onDownloadCollectedData,
    resetDownloadCollectedDataState as onResetDownloadCollectedDataState, 
    backToImpersonator as onBackToImpersonator,
    switchToUser as onSwitchToUser,
} from '../../store/actions';
import { baseUrl } from '../../helpers/api_helper';
import { LinkedAccounts } from '../user-link';
import ChangePasswordForm from '../user-profile/changePasswordForm';
import ChangeProfilePictureForm from '../user-profile/changeProfilePictureForm';
import UploadSignaturePictureForm from '../user-profile/uploadSignaturePictureForm';
import { MyProfileSettings } from '../user-profile/myProfileSettings';
import { AlertDialog } from '../common/dialogs';

export const ProfileMenu = ({
    openModal,
    closeModal,
    openDialog,
    closeDialog
}) => {
    const [anchorProfile, setAnchorProfile] = React.useState(null);
    const isProfile = Boolean(anchorProfile);
    const [profileMenu, setProfileMenu] = useState(null);
    const [user, setUser] = useState(null);
    const [tenant, setTenant] = useState(null);
    const [linkedAccounts, setLinkedAccounts] = useState([]);

    const dispatch = useDispatch();
    const { 
        userProfileMenu,
        linkedUsers,
        downloadSuccess, 
        backToImpersonatorResponse,
        switchAccountResponse
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu,
        linkedUsers: state.UserLinkReducer.linkedUsers,
        downloadSuccess: state.UserProfileReducer.downloadSuccess, 
        backToImpersonatorResponse: state.AccountReducer.backToImpersonatorResponse,
        switchAccountResponse: state.AccountReducer.switchAccountResponse
    }));

    useEffect(() => {
        dispatch(getUserProfileMenu());
        dispatch(getLinkedUsers());
    }, [dispatch]);

    useEffect(() => {
        if (isEmpty(profileMenu) && !isEmpty(userProfileMenu) && !isEmpty(userProfileMenu.result)) {
            setProfileMenu(userProfileMenu.result);

            const { loginInformations } = userProfileMenu.result;
            if (!isEmpty(loginInformations)) {
                const { user, tenant } = loginInformations;
                setUser(user);
                setTenant(tenant);
            }
        }
    }, [profileMenu, userProfileMenu]);

    useEffect(() => {
        if (isEmpty(linkedAccounts) && 
            !isEmpty(linkedUsers) && 
            !isEmpty(linkedUsers.result)) {
                const { result } = linkedUsers;
                if (!isEmpty(result) && !isEmpty(result.items)) {
                    setLinkedAccounts(result.items);
                }
        }
    }, [linkedAccounts, linkedUsers]);

    useEffect(() => {
        if (downloadSuccess) {
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='success'
                        message='We are preparing your data. You will be notified when your data is prepared.'
                    />
                )
            });
            dispatch(onResetDownloadCollectedDataState());
        }
    }, [dispatch, downloadSuccess, openDialog]);

    useEffect(() => {
        if (backToImpersonatorResponse) {
            const { targetUrl } = backToImpersonatorResponse;
            if (targetUrl) {
                window.location.href = targetUrl;
            }
        }
    }, [backToImpersonatorResponse]);

    useEffect(() => {
        if (switchAccountResponse) {
            const { targetUrl } = switchAccountResponse;
            if (targetUrl) {
                window.location.href = targetUrl;
            }
        }
    }, [switchAccountResponse]);

    // Handle showing profile menu
    const handleProfileClick = (event) => {
        setAnchorProfile(event.currentTarget);
    };

    const handleProfileClose = () => {
        setAnchorProfile(null);
    };

    const showLoginName = () => {
        if (!isEmpty(profileMenu)) {
            let username;
            if (!isEmpty(user)) {
                username = user.userName;
            }
    
            if (!profileMenu.isMultiTenancyEnabled) {
                return (<>{username}</>)
            }

            return isEmpty(tenant) 
                ? (<>{`.\\${username}`}</>)
                : (<>{`${tenant.tenancyName}\\${username}`}</>);
        }
    };

    const handleBackToMyAccount = () => {
        dispatch(onBackToImpersonator());
    };

    const handleLinkedAccounts = () => {
        handleProfileClose();
        openModal(
            (
                <LinkedAccounts 
                    openModal={openModal}
                    closeModal={closeModal} 
                    openDialog={openDialog} 
                    closeDialog={closeDialog}
                />
            ),
            400
        );
    };

    const handleSwitchToUser = (account) => {
        const { id, tenantId } = account;
        if (id) {
            dispatch(onSwitchToUser({
                targetUserId: id,
                targetTenantId: tenantId
            }));
        }
    };

    const handleChangePassword = () => {
        handleProfileClose();
        openModal(
            (
                <ChangePasswordForm 
                    openModal={openModal}
                    closeModal={closeModal} 
                    openDialog={openDialog} 
                />
            ),
            400
        );
    };

    const handleChangeProfilePicture = () => {
        handleProfileClose();
        openModal(
            (
                <ChangeProfilePictureForm 
                    openModal={openModal}
                    closeModal={closeModal} 
                />
            ),
            400
        )
    };

    const handleUploadSignaturePicture = () => {
        handleProfileClose();
        openModal(
            (
                <UploadSignaturePictureForm 
                    openModal={openModal}
                    closeModal={closeModal} 
                    openDialog={openDialog}
                />
            ),
            400
        )
    };

    const handleMySettings = () => {
        handleProfileClose();
        openModal(
            (
                <MyProfileSettings
                    openModal={openModal}
                    closeModal={closeModal} 
                />
            ),
            500
        );
    }

    const handleDownloadCollectedData = () => {
        handleProfileClose();
        dispatch(onDownloadCollectedData());
    };

    const handleLogout = (e) => {
        e.preventDefault();
        window.location.href = `${baseUrl}/Account/Logout`;
    };
    
    const renderAvatar = () => {
        if (!isEmpty(tenant) && !isEmpty(tenant.logoId)) {
            return (
                <Avatar
                    alt='account'
                    src={`${baseUrl}/TenantCustomization/GetLogo?id=${tenant.logoId}`}
                    sx={{ mr: 1, width: 24, height: 24 }}
                />
            );
        }
        
        return (
            <Avatar
                alt='account'
                src='/reactapp/assets/images/app-logo-dump-truck-130x35.gif'
                sx={{ mr: 1, width: 24, height: 24 }}
            />
        );
    };

    return (
        <React.Fragment>
            { !isEmpty(profileMenu) 
                ? 
                    <React.Fragment>
                        <Button 
                            id='profile'
                            aria-haspopup='true'
                            aria-expanded={isProfile ? 'true' : undefined}
                            onClick={handleProfileClick}
                            aria-label='profileSettings' 
                            sx={{ mr: 2, px: 4 }}
                        >
                            { profileMenu.isImpersonatedLogin && <i className={`fa-regular fa-reply icon`} style={{ marginRight: 6 }}></i>}
                            <Typography sx={{ fontWeight: 600, fontSize: 12, marginRight: '8px' }}>
                                {showLoginName()}
                            </Typography>
                            {renderAvatar()}
                        </Button>

                        <Menu
                            id='profile-list'
                            disable
                            anchorEl={anchorProfile}
                            open={isProfile}
                            onClose={handleProfileClose}
                            MenuListProps={{ sx: { py: 0 } }}
                        >
                            <Paper sx={{ width: 1 }}>
                                <Box
                                    sx={{
                                        background: theme.palette.gradient.main,
                                        px: 2,
                                        py: 2,
                                        display: 'flex',
                                        maxWidth: '300px',
                                    }}
                                >
                                    {renderAvatar()}

                                    <Typography
                                        sx={{
                                            overflow: 'hidden',
                                            textOverflow: 'ellipsis',
                                            whiteSpace: 'nowrap',
                                        }}
                                        variant='body1'
                                        fontWeight={700}
                                        color='white'
                                    >
                                        {showLoginName()}
                                    </Typography>
                                </Box>

                                <MenuList dense>
                                    { profileMenu.isImpersonatedLogin && 
                                        <MenuItem 
                                            component={Link} 
                                            sx={{ py: 1 }} 
                                            onClick={handleBackToMyAccount}
                                        >
                                            <i className={`fa-regular fa-angle-left icon`} style={{ marginRight: 6 }}></i>
                                            <Typography>Back to my account</Typography>
                                        </MenuItem>
                                    }
                                    
                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        onClick={handleLinkedAccounts}
                                    >
                                        <i className={`fa-regular fa-users-gear icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Manage linked accounts</Typography>
                                    </MenuItem>

                                    { !isEmpty(linkedAccounts) &&
                                        <MenuList sx={{ p: 0 }}>
                                            { linkedAccounts.map((account, index) => (
                                                <MenuItem 
                                                    key={index} 
                                                    component={Link} 
                                                    sx={{ padding: '8px 16px 8px 35px' }} 
                                                    onClick={() => handleSwitchToUser(account)}
                                                >
                                                    <i className={`fa-regular fa-period icon`} style={{ marginTop: '-7px' }}></i>
                                                    <Typography>{`${account.tenancyName}\\${account.username}`}</Typography>
                                                </MenuItem>
                                            ))}
                                        </MenuList>  
                                    }

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        onClick={handleChangePassword}
                                    >
                                        <i className={`fa-regular fa-ellipsis-stroke icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Change password</Typography>
                                    </MenuItem>

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        href='/App/Users/LoginAttempts'
                                    >
                                        <i className={`fa-regular fa-list-check icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Login attempts</Typography>
                                    </MenuItem>

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        onClick={handleChangeProfilePicture}
                                    >
                                        <i className={`fa-regular fa-square-user icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Change profile picture</Typography>
                                    </MenuItem>

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        onClick={handleUploadSignaturePicture}
                                    >
                                        <i className={`fa-regular fa-signature icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Upload signature picture</Typography>
                                    </MenuItem>

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }}
                                        onClick={handleMySettings}
                                    >
                                        <i className={`fa-regular fa-gear icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>My settings</Typography>
                                    </MenuItem>

                                    <MenuItem 
                                        component={Link} 
                                        sx={{ py: 1 }} 
                                        onClick={handleDownloadCollectedData}
                                    >
                                        <i className={`fa-regular fa-arrow-down-to-bracket icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Download collected data</Typography>
                                    </MenuItem>

                                    <MenuItem
                                        sx={{
                                            py: 1,
                                            backgroundColor: theme.palette.primary.light,
                                        }}
                                        onClick={(e) => handleLogout(e)}
                                    >
                                        <i
                                            className={`fa-regular fa-right-from-line icon`}
                                            style={{ marginRight: 6 }}></i>
                                        <Typography>Logout</Typography>
                                    </MenuItem>
                                </MenuList>
                            </Paper>
                        </Menu>
                    </React.Fragment> 
                : null 
            }
        </React.Fragment>
    )
}