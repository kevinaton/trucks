import * as React from 'react';
import {
    Autocomplete,
    Box,
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    Checkbox,
    FormControl,
    FormControlLabel,
    IconButton,
    InputAdornment,
    InputLabel,
    Modal,
    OutlinedInput,
    Stack,
    TextField,
} from '@mui/material';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { DemoContainer } from '@mui/x-date-pickers/internals/demo';
import { DatePicker, LocalizationProvider, MobileTimePicker } from '@mui/x-date-pickers';
import data from '../../../common/data/data.json';
import moment from 'moment';
import { grey } from '@mui/material/colors';
import { theme } from '../../../Theme';

const { Customers, offices, Designation, Addresses, Items, FreightUom } = data;

const AddEditJob = ({ state, setJob, title, data }) => {
    const [newDate, setNewDate] = React.useState(moment());
    const [selectedDate, setSelectedDate] = React.useState(moment('06/12/2023', 'MM/DD/YYYY'));
    const [isLock, setIsLock] = React.useState(true);
    const [isCustomer, setIsCustomer] = React.useState(false);
    const [isDesignation, setIsDesignation] = React.useState(false);
    const [isLoadAt, setIsLoadAt] = React.useState(false);
    const [isDeliverTo, setIsDeliverTo] = React.useState(false);
    const [isItem, setIsItem] = React.useState(false);
    const [isFreightUom, setIsFreightUom] = React.useState(false);

    React.useEffect(() => {
        if (data.customer) setIsCustomer(true);
        if (data.designation) setIsDesignation(true);
        if (data.load) setIsLoadAt(true);
        if (data.deliver) setIsDeliverTo(true);
        if (data.item) setIsItem(true);
        if (data.freightUom) setIsFreightUom(true);
    }, [data]);

    const handleClose = () => {
        setJob(false);
        state = false;
    };

    const handleFreightLock = () => {
        setIsLock(!isLock);
    };

    const priorityTypes = ['High', 'Medium', 'Low'];

    return (
        <Modal open={state} onClose={handleClose}>
            <Card
                sx={{
                    width: { xs: '80%', sm: '50%' },
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    maxHeight: '90vh',
                    overflow: 'auto',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title={title}
                />

                <CardContent>
                    {title === 'Add Job' ? (
                        <Box component='form' autoComplete='true' sx={{ w: 1 }}>
                            <Stack direction='column' spacing={2}>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <LocalizationProvider
                                        dateAdapter={AdapterMoment}
                                        adapterLocale='de'>
                                        <DatePicker
                                            id='deliveryDate'
                                            value={newDate}
                                            onChange={(newVal) => setNewDate(newVal)}
                                            format='MM/DD/YYYY'
                                            label='Delivery date'
                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                        />
                                    </LocalizationProvider>
                                    <Autocomplete
                                        id='customer'
                                        value={data.customer}
                                        options={Customers}
                                        renderInput={(params) => (
                                            <TextField {...params} label='Customer' required />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='offices'
                                        value='Main'
                                        options={offices}
                                        renderInput={(params) => (
                                            <TextField {...params} label='Office' />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <TextField
                                        id='jobNo'
                                        type='number'
                                        variant='outlined'
                                        label='Job Number'
                                        required
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                    <Autocomplete
                                        id='designation'
                                        options={Designation}
                                        renderInput={(params) => (
                                            <TextField {...params} label='Designation' required />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                            </Stack>
                        </Box>
                    ) : (
                        <Box component='form' autoComplete='true'>
                            <Stack direction='column' spacing={2}>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <LocalizationProvider
                                        dateAdapter={AdapterMoment}
                                        adapterLocale='de'>
                                        <DatePicker
                                            value={selectedDate}
                                            onChange={(newVal) => setSelectedDate(newVal)}
                                            id='deliveryDate'
                                            label='Delivery date'
                                            required
                                            sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                        />
                                    </LocalizationProvider>
                                    <Autocomplete
                                        id='customer'
                                        value='ABC Company'
                                        options={Customers}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Customer'
                                                disabled={isCustomer}
                                                InputProps={{
                                                    readOnly: true,
                                                }}
                                                required
                                                sx={{
                                                    backgroundColor:
                                                        isCustomer === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='offices'
                                        options={offices}
                                        renderInput={(params) => (
                                            <TextField {...params} label='Office' />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <TextField
                                        id='jobNo'
                                        type='number'
                                        variant='outlined'
                                        label='Job Number'
                                        required
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                    <Autocomplete
                                        id='designation'
                                        disabled={isDesignation}
                                        value={data.designation}
                                        options={Designation}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Designation'
                                                required
                                                InputProps={{
                                                    readOnly: true,
                                                }}
                                                sx={{
                                                    backgroundColor:
                                                        isDesignation === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='loadAt'
                                        value={data.load}
                                        options={Addresses}
                                        disabled={isLoadAt}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Load At'
                                                required
                                                sx={{
                                                    backgroundColor:
                                                        isLoadAt === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                    <Autocomplete
                                        id='deliverTo'
                                        value={data.deliver}
                                        options={Addresses}
                                        disabled={isDeliverTo}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Deliver To'
                                                sx={{
                                                    backgroundColor:
                                                        isDeliverTo === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='item'
                                        value={data.item}
                                        disabled={isItem}
                                        options={Items}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Item'
                                                required
                                                sx={{
                                                    backgroundColor:
                                                        isItem === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='freightUom'
                                        value={data.freightUom}
                                        options={FreightUom}
                                        disabled={isFreightUom}
                                        renderInput={(params) => (
                                            <TextField
                                                {...params}
                                                label='Freight UOM'
                                                required
                                                sx={{
                                                    backgroundColor:
                                                        isFreightUom === true
                                                            ? theme.palette.secondary.main
                                                            : '#ffffff',
                                                }}
                                            />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <TextField
                                        id='freightRate'
                                        type='number'
                                        variant='outlined'
                                        label='Freight Rate'
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                    <TextField
                                        id='freightQty'
                                        type='number'
                                        variant='outlined'
                                        label='Freight Qty'
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <FormControl
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                        variant='outlined'>
                                        <InputLabel htmlFor='freightInput'>Freight</InputLabel>
                                        <OutlinedInput
                                            id='freightInput'
                                            disabled={isLock}
                                            type='number'
                                            variant='outlined'
                                            label='Freight'
                                            endAdornment={
                                                <InputAdornment position='end'>
                                                    <IconButton
                                                        aria-label='toggle-lock-freight'
                                                        onClick={handleFreightLock}
                                                        edge='end'>
                                                        {isLock ? (
                                                            <i className='fa-regular fa-lock'></i>
                                                        ) : (
                                                            <i className='fa-regular fa-lock-open'></i>
                                                        )}
                                                    </IconButton>
                                                </InputAdornment>
                                            }
                                            sx={{
                                                backgroundColor:
                                                    isLock === true
                                                        ? theme.palette.secondary.main
                                                        : '#ffffff',
                                            }}
                                        />
                                    </FormControl>
                                    <TextField
                                        id='subContractorRate'
                                        type='number'
                                        variant='outlined'
                                        label='Sub-contractor Rate'
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <TextField
                                        id='salesTaxRate'
                                        type='number'
                                        variant='outlined'
                                        label='Sales Tax Rate'
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <TextField
                                        id='requestedNumberOfTrucks'
                                        type='number'
                                        variant='outlined'
                                        label='Requested Number of Trucks'
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                    <FormControlLabel
                                        label='Run Until Stopped'
                                        control={<Checkbox />}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <LocalizationProvider dateAdapter={AdapterMoment}>
                                        <DemoContainer
                                            sx={{ p: 0, flexBasis: { xs: '100%', sm: '50%' } }}
                                            components={['MobileTimePicker']}>
                                            <MobileTimePicker
                                                label='Time on Job'
                                                sx={{ flexBasis: '100%' }}
                                            />
                                        </DemoContainer>
                                    </LocalizationProvider>
                                    <TextField
                                        id='chargeTo'
                                        variant='outlined'
                                        label='Charge To'
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                </Stack>
                                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                    <Autocomplete
                                        id='priority'
                                        options={priorityTypes}
                                        renderInput={(params) => (
                                            <TextField {...params} label='Priority' />
                                        )}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </Stack>
                                <Stack direction='column' spacing={1}>
                                    <TextField
                                        id='note'
                                        multiline
                                        variant='outlined'
                                        label='Note'
                                        sx={{ flexBasis: { xs: '100%', sm: '50%' } }}
                                    />
                                    <FormControlLabel
                                        label='Requires Notification'
                                        control={<Checkbox />}
                                    />
                                </Stack>
                            </Stack>
                        </Box>
                    )}
                </CardContent>

                <CardActions
                    sx={{ justifyContent: 'end', borderTop: `1px solid ${grey[300]}`, py: 2 }}>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button
                        onClick={handleClose}
                        variant='contained'
                        startIcon={<i className='fa-regular fa-save'></i>}>
                        {title === 'Add Job' ? 'Add Job' : 'Update Job'}
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};

export default AddEditJob;
