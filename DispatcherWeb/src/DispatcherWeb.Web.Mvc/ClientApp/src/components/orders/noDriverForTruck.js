import * as React from 'react';
import { Datepicker } from '@mobiscroll/react';
import {
    Box,
    Button,
    Stack,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

const NoDriverForTruck = ({ 
    closeModal
}) => {
    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between' 
                alignItems='center'
                sx={{ p: 2 }}
            >
                <Typography variant='h6' component='h2'>Select dates to mark a truck as having no driver</Typography>
                <Button
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box sx={{ p: 2, width: '100%' }}>
                <Datepicker
                    controls={['calendar']}
                    select='range'
                    touchUi={true}
                    labelStyle='stacked'
                    inputStyle='outline'
                    inputProps={{
                        placeholder:'mm/dd/yyyy - mm/dd/yyyy',
                        label: 'Select start and end date',
                        className: 'mbsc-no-margin',
                    }}
                />
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

export default NoDriverForTruck;
