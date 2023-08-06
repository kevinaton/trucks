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
import _, { isEmpty } from 'lodash';
import { assetType } from '../../common/enums/assetType';
import { getVehicleCategories, getBedConstructions } from '../../store/actions';

const SelectTrailer = ({
    data,
    closeModal
}) => {
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState([]);
    const [isLoadingBedConstruction, setIsLoadingBedConstruction] = useState(false);
    const [bedConstructionOptions, setBedConstructionOptions] = useState([]);
    const [trailerOptions, setTrailerOptions] = useState([]);

    const [vehicleCategoryId, setVehicleCategoryId] = useState('');
    const [bedConstructionId, setBedConstructionId] = useState('');
    const [make, setMake] = useState('');
    const [model, setModel] = useState('');
    const [trailerId, setTrailerId] = useState({
        value: '', 
        required: true,
        error: false,
        errorText: ''
    });

    const dispatch = useDispatch();

    const {
        vehicleCategories,
        bedConstructions,
    } = useSelector((state) => ({
        vehicleCategories: state.TruckReducer.vehicleCategories,
        bedConstructions: state.TruckReducer.bedConstructions
    }));

    useEffect(() => {
        dispatch(getVehicleCategories({
            assetType: assetType.TRAILER
        }));
    }, []);

    useEffect(() => {
        if (!isEmpty(vehicleCategories) && !isEmpty(vehicleCategories.result)) {
            const { result } = vehicleCategories;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setVehicleCategoryOptions(result.items);
            }
        }
    }, [vehicleCategories]);

    useEffect(() => {
        if (!isEmpty(bedConstructions) && !isEmpty(bedConstructions.result)) {
            const { result } = bedConstructions;
            if (!isEmpty(result)) {
                setBedConstructionOptions(result);
            }
        }
    }, [bedConstructions]);
    
    const handleVehicleCategoryChange = (e, newValue) => {
        e.preventDefault();
        if (newValue) {
            dispatch(getBedConstructions({
                vehicleCategoryId: newValue
            }));
        } else {
            setBedConstructionId('');
            setMake('');
        }
        
        setVehicleCategoryId(newValue);
    };

    const handleBedConstructionChange = (e, newValue) => {
        e.preventDefault();
        setBedConstructionId(newValue);
    };

    const handleMakeChange = (e) => {
        e.preventDefault();
        setMake(e.target.value);
    };

    const handleModelChange = (e) => {
        e.preventDefault();
        setModel(e.target.value);
    };

    const handleTrailerChange = (e, newValue) => {
        e.preventDefault();
        setTrailerId(newValue);
    };

    const handleCancel = () => {
        // Reset the form
        
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();
        

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
                <Typography variant='h6' component='h2'>Select trailer</Typography>
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
                <Stack 
                    spacing={2} 
                    sx={{
                        paddingTop: '8px',
                        paddingBottom: '8px',
                        maxHeight: 'calc(100vh - 300px)',
                        overflowY: 'auto'
                    }}
                >
                    { vehicleCategoryOptions && 
                        <Autocomplete
                            id='vehicleCategoryId'
                            options={vehicleCategoryOptions} 
                            getOptionLabel={(option) => option.name} 
                            sx={{ 
                                flex: 1, 
                                flexShrink: 0
                            }}
                            renderInput={(params) => 
                                <TextField 
                                    {...params} 
                                    label='Category'
                                />
                            } 
                            onChange={(e, value) => handleVehicleCategoryChange(e, value.id)} 
                            fullWidth
                        />
                    }

                    { vehicleCategoryId && 
                        <React.Fragment>
                            { bedConstructionOptions && 
                                <Autocomplete 
                                    id='bedConstruction' 
                                    option={bedConstructions}
                                    getOptionLabel={(option) => option.name}
                                    sx={{
                                        flex: 1,
                                        flexShrink: 0
                                    }}
                                    renderInput={(params) => 
                                        <TextField 
                                            {...params} 
                                            label='Bed Construction'
                                        />
                                    }
                                    onChange={(e, value) => handleBedConstructionChange(e, value.id)}
                                    fullWidth
                                />
                            }

                            <TextField 
                                id='make'
                                name='make'
                                type='text'
                                label='Make'
                                value={make} 
                                onChange={handleMakeChange}
                                fullWidth
                            />

                            <TextField
                                id='model'
                                name='model'
                                type='text'
                                label='Model'
                                value={model} 
                                onChange={handleModelChange}
                                fullWidth
                            />
                        </React.Fragment>
                    }

                    { trailerOptions && 
                        <Autocomplete
                            id='trailerId'
                            options={trailerOptions} 
                            getOptionLabel={(option) => option.name} 
                            sx={{ 
                                flex: 1, 
                                flexShrink: 0,
                                "& .Mui-error": {
                                    borderColor: 'red',
                                },
                            }}
                            renderInput={(params) => 
                                <TextField 
                                    {...params} 
                                    label={
                                        <>
                                            Trailer <span style={{ marginLeft: '5px', color: 'red' }}>*</span>
                                        </>
                                    } 
                                    error={trailerId.error} 
                                    helperText={trailerId.error ? trailerId.errorText : ''} 
                                />
                            } 
                            onChange={(e, value) => handleTrailerChange(e, value.id)} 
                            fullWidth
                        />
                    }
                </Stack>
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

export default SelectTrailer;