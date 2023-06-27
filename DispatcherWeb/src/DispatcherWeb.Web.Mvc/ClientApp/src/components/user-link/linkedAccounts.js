import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { isEmpty, isEqual } from 'lodash';
import { 
    Box, 
    Button,
    IconButton,
    Stack,
    TableContainer, 
    Table, 
    TableHead, 
    TableRow,
    TableCell,
    TableBody,
    Typography
} from '@mui/material';
import { grey } from '@mui/material/colors';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import LoginIcon from '@mui/icons-material/Login';
import DeleteIcon from '@mui/icons-material/Delete';
import LinkNewAccountForm from './linkNewAccountForm';
import { theme } from '../../Theme';
import { useSnackbar } from 'notistack';
import { 
    unlinkUser as onUnlinnkUser,
    unlinkUserReset as onResetUnlinkState 
} from '../../store/actions';
import { AlertDialog } from '../common/dialogs';

export const LinkedAccounts = ({
    openModal,
    closeModal,
    openDialog,
    closeDialog
}) => {
    const [isLoading, setIsLoading] = useState(true);
    const [hoveredRow, setHoveredRow] = useState(null);
    const [linkedAccounts, setLinkedAccounts] = useState([]);

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const { 
        linkedUsers,
        unlinkSuccess,
        error
    } = useSelector((state) => ({
        linkedUsers: state.UserLinkReducer.linkedUsers,
        unlinkSuccess: state.UserLinkReducer.unlinkSuccess,
        error: state.UserLinkReducer.error
    }));

    useEffect(() => {
        if (!isEmpty(linkedUsers) && !isEmpty(linkedUsers.result)) {
            const { result } = linkedUsers;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setLinkedAccounts(prevAccounts => {
                    if (!isEqual(prevAccounts, result.items)) {
                        return result.items;
                    }
                    return prevAccounts;
                });
            } else {
                setLinkedAccounts([])
            }
        }
    
        setIsLoading(false);
    }, [linkedAccounts, linkedUsers]);

    useEffect(() => {
        if (unlinkSuccess) {
            enqueueSnackbar('Successfully unlinked', { variant: 'success' });
            dispatch(onResetUnlinkState());
        }
    }, [dispatch, enqueueSnackbar, unlinkSuccess]);

    useEffect(() => {
        if (!isEmpty(error) && !isEmpty(error.response)) {
            const { data } = error.response;
            const { message, details } = data.error;
            
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='error'
                        title={message} 
                        message={details} 
                    />
                )
            });
        }
    }, [error, openDialog]);

    // Handle row hover on table
    const handleRowHover = (index) => {
        setHoveredRow(index);
    };
    const handleRowLeave = () => {
        setHoveredRow(null);
    };

    const handleLinkNewAccount = () => {
        openModal(
            (<LinkNewAccountForm closeModal={closeModal} />),
            400
        );
    };

    const handleUnlinkAccount = (linkedUser) => {
        closeDialog();
        const data = {
            userId: linkedUser.id,
            tenantId: linkedUser.tenantId
        };
        dispatch(onUnlinnkUser(data));
    };

    const handleUnlinkAccountClick = (e, linkedUser) => {
        e.preventDefault();
        
        openDialog({
            type: 'confirm',
            title: 'Unlink account',
            content: (
                <Box
                    display='flex'
                    alignItems='center'
                    flexDirection='column'
                >
                    <Box 
                        display='flex' 
                        alignItems='center' 
                        justifyContent='center'
                        sx={{
                            marginBottom: '15px'
                        }}
                    >
                        <ErrorOutlineIcon 
                            sx={{ 
                                color: theme.palette.warning.main,
                                fontSize: '88px !important'
                            }} 
                        />
                    </Box>
                    <Typography variant='h4' sx={{ mb: 1 }}>Are you sure?</Typography>
                    <Typography variant='h6'>Link to {linkedUser.username} will be deleted.</Typography>
                </Box>
            ),
            action: () => handleUnlinkAccount(linkedUser)
        });
    };

    return (
        <React.Fragment>
            <Box 
                sx={{ 
                    p: 2 
                }} 
                display='flex'
                justifyContent='space-between'
                alignItems='center'
            >
                <Typography variant='h6' component='h2'>
                    Linked Accounts
                </Typography>
                <Button onClick={handleLinkNewAccount}>
                    <i className='fa-regular fa-plus' style={{ marginRight: '6px' }} />
                    <Typography>Link New Account</Typography>
                </Button>
            </Box>
            
            { !isLoading && isEmpty(linkedAccounts) &&
                <Box 
                    sx={{ p: 2 }} 
                    display='flex'
                    justifyContent='center'
                    alignItems='center'
                >
                    <Typography>
                        No linked accounts found.
                    </Typography>
                </Box>
            }

            { !isLoading && !isEmpty(linkedAccounts) && 
                <TableContainer 
                    component={Box}
                    sx={{ p: 0 }}
                >
                    <Table stickyHeader aria-label='linked accounts table' size='small'>
                        <TableHead>
                            <TableRow sx={{
                                '& th': {
                                    backgroundColor: grey[200]
                                }
                            }}>
                                <TableCell>Username</TableCell>
                                <TableCell style={{ width: '75px' }}>Actions</TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            { linkedAccounts.map((data, index) => {
                                return (
                                    <TableRow 
                                        key={index} 
                                        hover={true} 
                                        onMouseEnter={() => handleRowHover(index)}
                                        onMouseLeave={() => handleRowLeave}
                                        sx={{
                                            backgroundColor: hoveredRow === index 
                                                ? (theme) => theme.palette.action.hover 
                                                : '#ffffff',
                                            '&.MuiTableRow-root:hover': {
                                                backgroundColor: (theme) => theme.palette.action.hover
                                            }
                                        }}
                                    >
                                        <TableCell>
                                            <Box display='flex' alignItems='center'>
                                                <i className='fa-regular fa-user icon' style={{ marginRight: '6px' }} />
                                                <Typography>{`${data.tenancyName}\\${data.username}`}</Typography>
                                            </Box>
                                        </TableCell>

                                        <TableCell>
                                            <Stack spacing={1} direction='row'>
                                                <Button 
                                                    size='small'
                                                    startIcon={<LoginIcon />} 
                                                >
                                                    Login
                                                </Button>
                                                <IconButton 
                                                    size='small' 
                                                    onClick={(e) => handleUnlinkAccountClick(e, data)}
                                                >
                                                    <DeleteIcon sx={{
                                                        color: theme.palette.error.main
                                                    }} />
                                                </IconButton>
                                            </Stack>
                                        </TableCell>
                                    </TableRow>
                                )
                            })}
                        </TableBody>
                    </Table>
                </TableContainer>
            }

            <Box 
                sx={{ p: 2 }} 
                display='flex'
                justifyContent='flex-end' 
                alignItems='center'
            >
                <Button variant='outlined' onClick={closeModal}>Close</Button>
            </Box>
        </React.Fragment>
    )
}