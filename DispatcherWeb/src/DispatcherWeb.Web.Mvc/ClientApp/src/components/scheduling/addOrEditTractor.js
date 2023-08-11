import React, { useState, useEffect } from 'react';
import {
    Autocomplete,
    Box,
    Button,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';


const AddOrEditTractor = ({
    data,
    closeModal
}) => {
    const [tractorOptions, setTractorOptions] = useState([]);
    const [defaultTractorId, setDefaultTractorId] = useState(null);

    const handleTractorChange = (e, neValue) => {

    };

    const handleCancel = () => {
        // Reset the form
        
        closeModal();
    };

    const handleSave = (e) => {
        
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ p: 2 }} 
            >
                <Typography variant='h6' component='h2'>Change tractor</Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>
            
            <Box sx={{
                p: 2,
                width: '100%'
            }}>
                <Autocomplete
                    id='tractorId'
                    options={tractorOptions} 
                    getOptionLabel={(option) => option.name} 
                    defaultValue={tractorOptions[defaultTractorId]}
                    sx={{ 
                        flex: 1, 
                        flexShrink: 0
                    }}
                    renderInput={(params) => 
                        <TextField 
                            {...params} 
                            label={
                                <>
                                    Tractor <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                </>
                            }
                        />
                    } 
                    onChange={(e, value) => handleTractorChange(e, value.id)} 
                    fullWidth
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
    )
};

export default AddOrEditTractor;