import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    Avatar,
    Button,
    Box,
    Link,
    Menu,
    MenuItem,
    Paper,
    Typography,
    MenuList,
} from '@mui/material';
import { isEmpty } from 'lodash';
import { theme } from '../../Theme';
import { getUserProfileMenu, getLinkedUsers } from '../../store/actions';
import { baseUrl } from '../../helpers/api_helper';
import { LinkedAccounts } from '../user-link'

export const ProfileMenu = ({
    openModal,
    closeModal
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
        linkedUsers
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu,
        linkedUsers: state.UserLinkReducer.linkedUsers
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

    // Handle showing profile menu
    const handleProfileClick = (event) => {
        setAnchorProfile(event.currentTarget);
    };

    const handleProfileClose = () => {
        setAnchorProfile(null);
    };

    const handleLogout = (e) => {
        e.preventDefault();
        window.location.href = `${baseUrl}/Account/Logout`;
    }

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

    const showLinkedAccounts = () => {
        handleProfileClose();
        openModal(
            <LinkedAccounts 
                openModal={openModal}
                closeModal={closeModal} 
            />
        );
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

                                { profileMenu.isImpersonatedLogin && 
                                    <MenuItem component={Link} sx={{ py: 2 }}>
                                        <i className={`fa-regular fa-angle-left icon`} style={{ marginRight: 6 }}></i>
                                        <Typography>Back to my account</Typography>
                                    </MenuItem>
                                }
                                
                                <MenuItem 
                                    component={Link} 
                                    sx={{ py: 2 }} 
                                    onClick={showLinkedAccounts}
                                >
                                    <i className={`fa-regular fa-users-gear icon`} style={{ marginRight: 6 }}></i>
                                    <Typography>Manage linked accounts</Typography>
                                </MenuItem>

                                { !isEmpty(linkedAccounts) &&
                                    <MenuList sx={{ p: 0 }}>
                                        { linkedAccounts.map((account, index) => (
                                            <MenuItem key={index} component={Link} sx={{ padding: '10px 16px 10px 25px' }}>
                                                <i className={`fa-regular fa-user icon`} style={{ marginRight: 6 }}></i>
                                                <Typography>{`${account.tenancyName}\\${account.username}`}</Typography>
                                            </MenuItem>
                                        ))}
                                    </MenuList>  
                                }

                                <MenuItem 
                                    component={Link} 
                                    sx={{ py: 2 }} 
                                    href='/App/Users/LoginAttempts'
                                >
                                    <i className={`fa-regular fa-list-check icon`} style={{ marginRight: 6 }}></i>
                                    <Typography>Login attempts</Typography>
                                </MenuItem>

                                <MenuItem component={Link} sx={{ py: 2 }}>
                                    <i className={`fa-regular fa-square-user icon`} style={{ marginRight: 6 }}></i>
                                    <Typography>Change profile picture</Typography>
                                </MenuItem>

                                <MenuItem component={Link} sx={{ py: 2 }}>
                                    <i className={`fa-regular fa-signature icon`} style={{ marginRight: 6 }}></i>
                                    <Typography>Upload signature picture</Typography>
                                </MenuItem>

                                <MenuItem component={Link} sx={{ py: 2 }}>
                                    <i className={`fa-regular fa-gear icon`} style={{ marginRight: 6 }}></i>
                                    <Typography>My settings</Typography>
                                </MenuItem>

                                <MenuItem
                                    sx={{
                                        py: 2,
                                        backgroundColor: theme.palette.primary.light,
                                    }}
                                    onClick={(e) => handleLogout(e)}
                                >
                                    <i
                                        className={`fa-regular fa-right-from-line icon`}
                                        style={{ marginRight: 6 }}></i>
                                    <Typography>Logout</Typography>
                                </MenuItem>
                            </Paper>
                        </Menu>
                    </React.Fragment> 
                : null 
            }
        </React.Fragment>
    )
}