import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    Autocomplete,
    Box,
    Button,
    Stack,
    TextField,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import {
    getActiveTractorsSelectList,
    setTractorForTrailer as onSetTractorForTrailer,
    setTractorForTrailerReset as onResetSetTractorForTrailer
} from '../../store/actions';
import { isEmpty } from 'lodash';
import { useSnackbar } from 'notistack';

const AddOrEditTractor = ({
    data,
    closeModal
}) => {
    const [isLoadingActiveTractors, setIsLoadingActiveTractors] = useState(false);
    const [tractorOptions, setTractorOptions] = useState([]);
    const [defaultTractorId, setDefaultTractorId] = useState(null);
    const [tractorId, setTractorId] = useState({
        value: '',
        required: true,
        error: false,
        errorText: ''
    });

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const {
        isLoadingActiveTractorsOpts,
        activeTractorsSelectList,
        setTractorForTrailerResponse
    } = useSelector((state) => ({
        isLoadingActiveTractorsOpts: state.TruckReducer.isLoadingActiveTractorsOpts,
        activeTractorsSelectList: state.TruckReducer.activeTractorsSelectList,
        setTractorForTrailerResponse: state.TruckReducer.setTractorForTrailerResponse
    }));

    useEffect(() => {
        dispatch(getActiveTractorsSelectList({
            maxResultCount: 1000,
            skipCount: 0
        }));
    }, []);

    useEffect(() => {
        if (!isLoadingActiveTractorsOpts && !isEmpty(activeTractorsSelectList)) {
            const { result } = activeTractorsSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setTractorOptions(result.items);
            }
        }
    }, [activeTractorsSelectList]);
    
    useEffect(() => {
        if (isLoadingActiveTractors !== isLoadingActiveTractorsOpts) {
            setIsLoadingActiveTractors(isLoadingActiveTractorsOpts);
        }
    }, [isLoadingActiveTractorsOpts]);

    useEffect(() => {
        if (!isEmpty(setTractorForTrailerResponse) && setTractorForTrailerResponse.success) {
            const { trailerId } = setTractorForTrailerResponse;
            if (trailerId === data.trailerId) {
                dispatch(onResetSetTractorForTrailer());
                enqueueSnackbar('Saved successfully', { variant: 'success' });
            }
        }
    });

    const resetForm = () => {
        setDefaultTractorId(null);
        setTractorId({
            ...tractorId,
            value: '',
            error: false,
            errorText: ''
        });
    };

    const handleTractorChange = (e, neValue) => {
        e.preventDefault();
        setTractorId({
            ...tractorId,
            value: neValue,
            error: false,
            errorText: ''
        });
    };

    const handleCancel = () => {
        // Reset the form
        resetForm();
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();

        if (tractorId.required && !tractorId.value) {
            setTractorId({
                ...tractorId,
                error: true,
                errorText: 'Tractor is required'
            });
            return;
        }
        
        dispatch(onSetTractorForTrailer(data.trailerId, {
            date: data.date,
            shift: data.shift,
            officeId: data.officeId,
            tractorId: tractorId.value,
            trailerId: data.trailerId
        }));

        resetForm();
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
                            error={tractorId.error}
                            helperText={tractorId.errorText}
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