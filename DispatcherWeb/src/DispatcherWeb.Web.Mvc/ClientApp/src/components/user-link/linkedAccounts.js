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
    TableBody,
    Typography
} from '@mui/material';
import { grey } from '@mui/material/colors';
import { Tablecell } from '../DTComponents';

export const LinkedAccounts = ({
    openModal,
    closeModal
}) => {
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

    const testClick = () => {
        const body = (
            <>
                <h1>Another test here</h1>
            </>
        );
        openModal(body);
    };

    return (
        <React.Fragment>
            <Box 
                sx={{ 
                    display: 'flex', 
                    p: 2 
                }} 
                justifyContent='space-between'
                alignItems='center'
            >
                <Typography variant='h6' component='h2'>
                    Linked Accounts
                </Typography>
                <Button onClick={testClick}>
                    <i className='fa-regular fa-plus' style={{ marginRight: '6px' }} />
                    <Typography>Link New Account</Typography>
                </Button>
            </Box>

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
                            <Tablecell label='Username' value='User name' />
                            <Tablecell label='' value='Actions' style={{ width: '70px' }} />
                        </TableRow>
                    </TableHead>

                    <TableBody>
                        { !isEmpty(linkedAccounts) && 
                            <React.Fragment>
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
                                            <Tablecell label='Username' value={`${data.tenancyName}\\${data.username}`} />
                                            <Tablecell label='Action' style={{ width: '70px' }} value={
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
                                                                        <Typography align='left'>
                                                                            Login
                                                                        </Typography>
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
                                            } />
                                        </TableRow>
                                    )
                                })}
                            </React.Fragment>
                        }
                    </TableBody>
                </Table>
            </TableContainer>

            <Box 
                sx={{ display: 'flex', p: 2 }} 
                justifyContent='flex-end' 
                alignItems='center'
            >
                <Button variant='outlined' onClick={closeModal}>Close</Button>
            </Box>
        </React.Fragment>
    )
}