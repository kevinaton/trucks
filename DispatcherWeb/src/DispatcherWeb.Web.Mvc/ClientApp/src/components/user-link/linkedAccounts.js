import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { isEmpty } from 'lodash';
import { 
    Box, 
    IconButton,
    TableContainer, 
    Table, 
    TableHead, 
    TableRow,
    TableBody
} from '@mui/material';
import { grey } from '@mui/material/colors';
import { Tablecell } from '../DTComponents';

export const LinkedAccounts = () => {
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

    // Handle row hover on table
    const handleRowHover = (index) => {
        setHoveredRow(index);
    };
    const handleRowLeave = () => {
        setHoveredRow(null);
    };

    return (
        <TableContainer component={Box}>
            <Table stickyHeader aria-label='linked accounts table' size='small'>
                <TableHead>
                    <TableRow sx={{
                        '& th': {
                            backgroundColor: grey[200]
                        }
                    }}>
                        <Tablecell label='Username' value='User name' />
                        <Tablecell label='' value='' />
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
                                        <Tablecell label='Action' value={
                                            <div>
                                                <IconButton sx={{ width: 25, height: 25}}>
                                                    <i className='fa-regular fa-ellipsis-vertical'></i>
                                                </IconButton>
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
    )
}