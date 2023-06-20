import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { isEmpty } from 'lodash';
import { 
    Box, 
    Button,
    IconButton,
    Menu,
    ListItem,
    ListItemButton,
    ListItemText,
    TableContainer, 
    Table, 
    TableHead, 
    TableRow,
    TableCell,
    TableBody,
    Typography
} from '@mui/material';
import { grey } from '@mui/material/colors';
import LinkNewAccountForm from './linkNewAccountForm';

export const LinkedAccounts = ({
    openModal,
    closeModal
}) => {
    const [isLoading, setIsLoading] = useState(true);
    const [actionAnchor, setActionAnchor] = useState(null);
    const actionOpen = Boolean(actionAnchor);
    const [hoveredRow, setHoveredRow] = useState(null);
    const [linkedAccounts, setLinkedAccounts] = useState([]);
    const { linkedUsers } = useSelector((state) => ({
        linkedUsers: state.UserLinkReducer.linkedUsers
    }));

    useEffect(() => {
        if (isEmpty(linkedAccounts) && 
            !isEmpty(linkedUsers) && 
            !isEmpty(linkedUsers.result)) {
                const { result } = linkedUsers;
                if (!isEmpty(result) && !isEmpty(result.items)) {
                    setLinkedAccounts(result.items);
                }

                setIsLoading(false);
        }
    }, [linkedAccounts, linkedUsers]);

    // Handle action on table rows
    const handleActionClick = (event) => {
        setActionAnchor(event.currentTarget);
    };
    const handleActionClose = () => {
        setActionAnchor(null);
    };

    // Handle row hover on table
    const handleRowHover = (index) => {
        setHoveredRow(index);
    };
    const handleRowLeave = () => {
        setHoveredRow(null);
    };

    const handleLinkNewAccount = () => {
        openModal(<LinkNewAccountForm closeModal={closeModal} />);
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
                                        <TableCell>{`${data.tenancyName}\\${data.username}`} </TableCell>
                                        <TableCell>
                                            <div>
                                                <IconButton 
                                                    sx={{ width: 25, height: 25}}
                                                    onClick={handleActionClick}
                                                >
                                                    <i className='fa-regular fa-ellipsis-vertical'></i>
                                                </IconButton>
                                                <Menu
                                                    anchorEl={actionAnchor}
                                                    id='actions-menu' 
                                                    open={actionOpen} 
                                                    onClose={handleActionClose}
                                                >
                                                    <ListItem disablePadding>
                                                        <ListItemButton onClick={handleActionClose}>
                                                            <ListItemText 
                                                                primary={
                                                                    <Typography align='left'>Login</Typography>
                                                                } 
                                                            />
                                                        </ListItemButton>
                                                    </ListItem>

                                                    <ListItem disablePadding>
                                                        <ListItemButton>
                                                            <ListItemText 
                                                                primary={
                                                                    <Typography align='left'>Delete</Typography>
                                                                }
                                                            />
                                                        </ListItemButton>
                                                    </ListItem>
                                                </Menu>
                                            </div>
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