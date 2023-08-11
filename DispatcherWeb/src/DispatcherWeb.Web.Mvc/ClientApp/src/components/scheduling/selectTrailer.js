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
import { 
    getVehicleCategories, 
    getBedConstructions,
    getMakesSelectList,
    getModelsSelectList,
    getActiveTrailersSelectList
} from '../../store/actions';

const SelectTrailer = ({
    data,
    closeModal,
    handleSelectTrailer
}) => {
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState([]);
    const [isLoadingBedConstructions, setIsLoadingBedConstructions] = useState(false);
    const [bedConstructionOptions, setBedConstructionOptions] = useState([]);
    const [isLoadingMakes, setIsLoadingMakes] = useState(false);
    const [makeOptions, setMakeOptions] = useState([]);
    const [isLoadingModels, setIsLoadingModels] = useState(false);
    const [modelOptions, setModelOptions] = useState([]);
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
        makesSelectList,
        modelsSelectList,
        activeTrailersSelectList
    } = useSelector((state) => ({
        vehicleCategories: state.TruckReducer.vehicleCategories,
        bedConstructions: state.TruckReducer.bedConstructions,
        makesSelectList: state.TruckReducer.makesSelectList,
        modelsSelectList: state.TruckReducer.modelsSelectList,
        activeTrailersSelectList: state.TruckReducer.activeTrailersSelectList
    }));

    useEffect(() => {
        dispatch(getVehicleCategories({
            assetType: assetType.TRAILER
        }));

        dispatch(getActiveTrailersSelectList({
            maxResultCount: 1000,
            skipCount: 0,
            vehicleCategoryId,
            bedConstruction: bedConstructionId,
            make,
            model
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
            setBedConstructionOptions(result);
            setIsLoadingBedConstructions(false);
        }
    }, [bedConstructions]);

    useEffect(() => {
        if (!isEmpty(makesSelectList) && !isEmpty(makesSelectList.result)) {
            const { result } = makesSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setMakeOptions(result.items);
                setIsLoadingMakes(false);
            }
        }
    }, [makesSelectList]);

    useEffect(() => {
        if (!isEmpty(modelsSelectList) && !isEmpty(modelsSelectList.result)) {
            const { result } = modelsSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setModelOptions(result.items);
                setIsLoadingModels(false);
            }
        }
    }, [modelsSelectList]);

    useEffect(() => {
        if (!isEmpty(activeTrailersSelectList) && !isEmpty(activeTrailersSelectList.result)) {
            const { result } = activeTrailersSelectList;
            if (!result.items.length) {
                setTrailerId('');
            }

            if (!isEmpty(result) && !isEmpty(result.items)) {
                setTrailerOptions(result.items);
            }
        }
    }, [activeTrailersSelectList]);

    const resetTrailerOptions = () => {
        if (!trailerId.value) {
            return;
        }

        dispatch(getActiveTrailersSelectList({
            maxResultCount: 1000,
            skipCount: 0,
            vehicleCategoryId,
            bedConstruction: bedConstructionId,
            make,
            model,
            id: trailerId.value
        }));
    };

    const getSelectedTrailerOption = () => {
        return _.find(trailerOptions, (item) => item.id === trailerId.value);
    };

    const resetForm = () => {
        setVehicleCategoryId('');
        setBedConstructionId('');
        setMake('');
        setModel('');
        setTrailerId('');
    };
    
    const handleVehicleCategoryChange = (e, newValue) => {
        e.preventDefault();
        if (newValue) {
            dispatch(getBedConstructions({
                vehicleCategoryId: newValue
            }));
            setIsLoadingBedConstructions(true);
        } else {
            setBedConstructionId('');
            setMake('');
        }

        if (!makeOptions.length) {
            setIsLoadingMakes(true);
            dispatch(getMakesSelectList({
                vehicleCategoryId: newValue,
                maxResultCount: 1000,
                skipCount: 0
            }));
        }
        
        setVehicleCategoryId(newValue);
        resetTrailerOptions();
    };

    const handleBedConstructionChange = (e, newValue) => {
        e.preventDefault();
        setBedConstructionId(newValue);
        resetTrailerOptions();
    };

    const handleMakeChange = (e, newValue) => {
        e.preventDefault();
        
        if (!modelOptions.length) {
            setIsLoadingModels(true);
            dispatch(getModelsSelectList({
                vehicleCategoryId,
                make: newValue,
                maxResultCount: 1000,
                skipCount: 0
            }));
        }

        setMake(newValue);
        resetTrailerOptions();
    };

    const handleModelChange = (e, newValue) => {
        e.preventDefault();
        setModel(e.target.value);
        resetTrailerOptions();
    };

    const handleTrailerChange = (e, newValue) => {
        e.preventDefault();
        setTrailerId({
            ...trailerId,
            value: newValue
        });
    };

    const handleCancel = () => {
        // Reset the form
        
        closeModal();
    };

    const handleSave = (e) => {
        e.preventDefault();

        if (trailerId.required && !trailerId.value) {
            setTrailerId({
                ...trailerId,
                error: true,
                errorText: 'Trailer is required'
            });
            return;
        }
        
        const selectedTrailer = getSelectedTrailerOption();
        
        const result = trailerId.value ? {
            id: Number(trailerId.value),
            truckCode: selectedTrailer.name
        } : null;

        if (result) {
            if (selectedTrailer) {
                result.vehicleCategory = {
                    id: selectedTrailer.item.vehicleCategoryId,
                };
            } else {
                result.vehicleCategory = {
                    id: data.vehicleCategoryId
                };
            }
        }

        handleSelectTrailer(result);
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
                            { !isLoadingBedConstructions && bedConstructionOptions && 
                                <Autocomplete 
                                    id='bedConstruction' 
                                    options={bedConstructionOptions}
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

                            { !isLoadingMakes && makeOptions && 
                                <Autocomplete 
                                    id='make' 
                                    options={makeOptions}
                                    getOptionLabel={(option) => option.name}
                                    sx={{
                                        flex: 1,
                                        flexShrink: 0
                                    }}
                                    renderInput={(params) => 
                                        <TextField 
                                            {...params} 
                                            label='Make' 
                                        />
                                    }
                                    onChange={(e, value) => handleMakeChange(e, value.id)}
                                    fullWidth
                                />
                            }

                            { make && !isLoadingModels && modelOptions && 
                                <Autocomplete 
                                    id='model' 
                                    options={modelOptions}
                                    getOptionLabel={(option) => option.name}
                                    sx={{
                                        flex: 1,
                                        flexShrink: 0
                                    }}
                                    renderInput={(params) => 
                                        <TextField 
                                            {...params} 
                                            label='Model' 
                                        />
                                    }
                                    onChange={(e, value) => handleModelChange(e, value.id)}
                                    fullWidth
                                />
                            }
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