import React, { useState, useEffect } from 'react';
import {
    Box,
    Button,
    Stack,
    Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

const AddOrEditService = ({
    closeModal
}) => {
    const handleSave = (e) => {
        e.preventDefault();
    };

    const handleCancel = () => {
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
                <Typography variant='h6' component='h2'>Create New Product/Service</Typography>
                <Button
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box sx={{ width: '100%' }}>
                
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
    );
};

export default AddOrEditService;