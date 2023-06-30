import * as React from 'react';
import {
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    IconButton,
    Modal,
    Stack,
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import moment from 'moment';

const SelectDeliveryDate = ({ isOpen, setIsOpen }) => {
    const [dateStart, setDateStart] = React.useState(moment());
    const [dateEnd, setDateEnd] = React.useState(moment());
    const handleSave = () => {
        setIsOpen(false);
    };

    const handleClose = () => {
        setIsOpen(false);
    };

    return (
        <Modal open={isOpen} onClose={handleClose} aria-labelledby='select-delivery-date'>
            <Card
                sx={{
                    minWidth: 500,
                    position: 'absolute',
                    top: '30%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='Select Delivery Date'
                />
                <CardContent>
                    <Stack direction='column' spacing={2}>
                        <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale='de'>
                            <DatePicker
                                id='deliveryDateStartRange'
                                value={dateStart}
                                onChange={(newVal) => {
                                    setDateStart(newVal);
                                }}
                                format='MM/DD/YYYY'
                                label='Delivery Date start range'
                                slotProps={{ textField: { required: true } }}
                                sx={{ width: '1' }}
                            />
                        </LocalizationProvider>
                        <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale='de'>
                            <DatePicker
                                id='deliveryDateEndRange'
                                value={dateEnd}
                                onChange={(newVal) => {
                                    setDateEnd(newVal);
                                }}
                                format='MM/DD/YYYY'
                                label='Delivery Date end range'
                                slotProps={{ textField: { required: true } }}
                                sx={{ width: '1' }}
                            />
                        </LocalizationProvider>
                    </Stack>
                </CardContent>
                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button
                        variant='contained'
                        onClick={handleSave}
                        startIcon={<i className='fa-regular fa-save'></i>}>
                        Save
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};

export default SelectDeliveryDate;
