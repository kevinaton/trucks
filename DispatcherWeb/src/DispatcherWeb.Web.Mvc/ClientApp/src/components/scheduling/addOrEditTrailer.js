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

const AddOrEditTrailer = ({
    data,
    closeModal,
    handleSelectTrailer,
    editTrailer
}) => {
    const [isEditing, setIsEditing] = useState(null);
    const [vehicleCategoryOptions, setVehicleCategoryOptions] = useState([]);
    const [isLoadingBedConstructions, setIsLoadingBedConstructions] = useState(false);
    const [bedConstructionOptions, setBedConstructionOptions] = useState([]);
    const [isLoadingMakes, setIsLoadingMakes] = useState(false);
    const [makeOptions, setMakeOptions] = useState([]);
    const [isLoadingModels, setIsLoadingModels] = useState(false);
    const [modelOptions, setModelOptions] = useState([]);
    const [isLoadingTrailers, setIsLoadingTrailers] = useState(false);
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
    const [defaultTrailerId, setDefaultTrailerId] = useState(null);
    const [hasInitializedTrailerId, setHasInitializedTrailerId] = useState(false);

    const dispatch = useDispatch();

    const {
        vehicleCategories,
        isLoadingBedConstructionsOpts,
        bedConstructions,
        isLoadingMakesOpts,
        makesSelectList,
        isLoadingModelsOpts,
        modelsSelectList,
        isLoadingActiveTrailersOpts,
        activeTrailersSelectList
    } = useSelector((state) => ({
        vehicleCategories: state.TruckReducer.vehicleCategories,
        isLoadingBedConstructionsOpts: state.TruckReducer.isLoadingBedConstructionsOpts,
        bedConstructions: state.TruckReducer.bedConstructions,
        isLoadingMakesOpts: state.TruckReducer.isLoadingMakesOpts,
        makesSelectList: state.TruckReducer.makesSelectList,
        isLoadingModelsOpts: state.TruckReducer.isLoadingModelsOpts,
        modelsSelectList: state.TruckReducer.modelsSelectList,
        isLoadingActiveTrailersOpts: state.TruckReducer.isLoadingActiveTrailersOpts,
        activeTrailersSelectList: state.TruckReducer.activeTrailersSelectList
    }));

    useEffect(() => {
        dispatch(getVehicleCategories({
            maxResultCount: 1000,
            skipCount: 0,
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

        if (editTrailer !== null && editTrailer !== undefined) {
            setIsEditing(true);
        } else {
            setIsEditing(false);
        }
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
        if (!isLoadingBedConstructionsOpts && !isEmpty(bedConstructions)) {
            const { result } = bedConstructions;
            setBedConstructionOptions(result);
        }
    }, [bedConstructions]);

    useEffect(() => {
        if (isLoadingBedConstructions !== isLoadingBedConstructionsOpts) {
            setIsLoadingBedConstructions(isLoadingBedConstructionsOpts);
        }
    }, [isLoadingBedConstructionsOpts]);

    useEffect(() => {
        if (!isLoadingMakesOpts && !isEmpty(makesSelectList)) {
            const { result } = makesSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setMakeOptions(result.items);
            }
        }
    }, [makesSelectList]);

    useEffect(() => {
        if (isLoadingMakes !== isLoadingMakesOpts) {
            setIsLoadingMakes(isLoadingMakesOpts);
        }
    }, [isLoadingMakesOpts]);

    useEffect(() => {
        if (!isLoadingModelsOpts && !isEmpty(modelsSelectList)) {
            const { result } = modelsSelectList;
            if (!isEmpty(result) && !isEmpty(result.items)) {
                setModelOptions(result.items);
            }
        }
    }, [modelsSelectList]);

    useEffect(() => {
        if (isLoadingModels !== isLoadingModelsOpts) {
            setIsLoadingModels(isLoadingModelsOpts);
        }
    }, [isLoadingModelsOpts]);

    useEffect(() => {
        if (!isLoadingActiveTrailersOpts && !isEmpty(activeTrailersSelectList)) {
            const { result } = activeTrailersSelectList;
            if (!result.items.length) {
                setTrailerId('');
            }

            if (!isEmpty(result) && !isEmpty(result.items)) {
                const options = result.items;
                setTrailerOptions(options);

                if (isEditing && defaultTrailerId === null) {
                    setDefaultTrailerId(_.findIndex(options, { id: editTrailer.trailerId.toString() }));
                    setHasInitializedTrailerId(true);
                } else {
                    setHasInitializedTrailerId(true);
                }
            } else {
                setHasInitializedTrailerId(true);
            }
        }
    }, [activeTrailersSelectList]);

    useEffect(() => {
        if (isLoadingTrailers !== isLoadingActiveTrailersOpts) {
            setIsLoadingTrailers(isLoadingActiveTrailersOpts);
        }
    }, [isLoadingActiveTrailersOpts]);

    const resetTrailerOptions = () => {
        if (!trailerId.value) {
            return;
        }

        setTrailerOptions([]);
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
        setTrailerId({
            ...trailerId,
            value: ''
        });
        setDefaultTrailerId(null);
    };
    
    const handleVehicleCategoryChange = (e, newValue) => {
        e.preventDefault();

        setHasInitializedTrailerId(false);
        setTrailerOptions([]);

        const getTrailerOptsByVehicleCategory = () => {
            dispatch(getActiveTrailersSelectList({
                maxResultCount: 1000,
                skipCount: 0,
                vehicleCategoryId: newValue,
                bedConstruction: bedConstructionId,
                make,
                model
            }));
        };

        const getTrailerOptsByVehicleCategoryWithTrailerId = () => {
            dispatch(getActiveTrailersSelectList({
                maxResultCount: 1000,
                skipCount: 0,
                vehicleCategoryId: newValue,
                bedConstruction: bedConstructionId,
                make,
                model,
                id: trailerId.value
            }));
        };

        if (newValue) {
            setMakeOptions([]);
            setModelOptions([]);

            setBedConstructionId('');
            dispatch(getBedConstructions({
                vehicleCategoryId: newValue
            }));

            setMake('');
            dispatch(getMakesSelectList({
                vehicleCategoryId: newValue,
                maxResultCount: 1000,
                skipCount: 0
            }));

            if (isEditing) {
                setModel('');
                setTrailerId({
                    ...trailerId,
                    value: ''
                });

                if (defaultTrailerId !== null) {
                    setDefaultTrailerId(null);
                }

                getTrailerOptsByVehicleCategory();
            } else {
                if (trailerId.value) {
                    getTrailerOptsByVehicleCategoryWithTrailerId();
                } else {
                    getTrailerOptsByVehicleCategory();
                }
            }
        } else {
            setBedConstructionId('');
            setMake('');
            setModel('');

            getTrailerOptsByVehicleCategory();
        }
        
        setVehicleCategoryId(newValue);
    };

    const handleBedConstructionChange = (e, newValue) => {
        e.preventDefault();
        setBedConstructionId(newValue);
        resetTrailerOptions();
    };

    const handleMakeChange = (e, newValue) => {
        e.preventDefault();
        
        if (!modelOptions.length) {
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
                { isEditing && 
                    <Box>
                        <Typography 
                            variant='body2' 
                            component='p' 
                            sx={{ mb: 1 }}
                        >
                            {editTrailer.subTitle}
                        </Typography>
                    </Box>
                }
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
                            { !isLoadingBedConstructions && 
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

                            { !isLoadingMakes && 
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

                            { make && !isLoadingModels && 
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

                    { hasInitializedTrailerId && !isLoadingTrailers && 
                        <Autocomplete
                            id='trailerId'
                            options={trailerOptions} 
                            getOptionLabel={(option) => option.name} 
                            defaultValue={trailerOptions[defaultTrailerId]}
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

export default AddOrEditTrailer;