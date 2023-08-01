import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    Box,
    Button,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography
} from '@mui/material';
import { grey } from '@mui/material/colors';
import CloseIcon from '@mui/icons-material/Close';

const TruckOrders = ({
    filter,
    closeModal
}) => {

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ p: 2 }} 
            >
                <Typography variant='h6' component='h2'>Order lines</Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>
            
            <Box sx={{ width: '100%' }}>
                <TableContainer>
                    <Table
                        aria-label='main-table'
                        sx={{
                            borderTop: `1px solid ${grey[300]}`,
                            borderLeft: `1px solid ${grey[300]}`,
                            borderRight: `1px solid ${grey[300]}`,
                        }}
                    >
                        <TableHead>
                            <TableRow>
                                <TableCell>Driver</TableCell>
                                <TableCell>Time On Job</TableCell>
                                <TableCell>Customer</TableCell>
                                <TableCell>Load At</TableCell>
                                <TableCell>Deliver To</TableCell>
                                <TableCell>Portion of Day</TableCell>
                                <TableCell>Item</TableCell>
                                <TableCell>Quantity</TableCell>
                                <TableCell>Shared</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            <TableRow>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                                <TableCell></TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </TableContainer>
            </Box>

            <Box sx={{ p: 2 }}>
                <Stack 
                    spacing={2}
                    direction='row' 
                    justifyContent='flex-end'
                >
                    <Button variant='outlined' onClick={handleCancel}>Close</Button>
                </Stack>
            </Box>
        </React.Fragment>
    );
}

export default TruckOrders;