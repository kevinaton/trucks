import * as React from 'react';
import {
    Autocomplete,
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    Divider,
    IconButton,
    Modal,
    Stack,
    TextField,
} from '@mui/material';
import data from '../../../common/data/data.json';

const { Designation, Addresses, Items, FreightUom } = data;

const AddQuoteItem = ({ isAddItem, setIsAddItem, newQuoteItem, setNewQuoteItem }) => {
    const handleAddItemClose = () => {
        setIsAddItem(false);
    };

    const handleItemValues = (field, event, value) => {
        setNewQuoteItem((prev) => ({
            ...prev,
            [field]: value ? value : event.target.value,
        }));
    };

    const handleItemSave = () => {
        console.log(newQuoteItem);
        setIsAddItem(false);
    };

    return (
        <Modal open={isAddItem} onClose={handleAddItemClose} aria-labelledby='add-item'>
            <Card
                sx={{
                    width: '50%',
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    maxHeight: '93vh',
                    overflow: 'auto',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleAddItemClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='Add Quote Item'
                />
                <CardContent>
                    <Stack direction='column' spacing={2}>
                        <Autocomplete
                            id='item-designation'
                            value={newQuoteItem.designation}
                            options={Designation}
                            size='small'
                            onChange={(event, value) =>
                                handleItemValues('designation', event, value)
                            }
                            renderInput={(params) => (
                                <TextField {...params} label='Designation' required />
                            )}
                        />
                        <Autocomplete
                            id='item-load'
                            value={newQuoteItem.load}
                            options={Addresses}
                            size='small'
                            onChange={(event, value) => handleItemValues('load', event, value)}
                            renderInput={(params) => <TextField {...params} label='Load At' />}
                        />
                        <Autocomplete
                            id='item-deliver'
                            value={newQuoteItem.deliver}
                            options={Addresses}
                            size='small'
                            onChange={(event, value) => handleItemValues('deliver', event, value)}
                            renderInput={(params) => <TextField {...params} label='Deliver At' />}
                        />
                        <Autocomplete
                            id='item-item'
                            value={newQuoteItem.item}
                            options={Items}
                            size='small'
                            onChange={(event, value) => handleItemValues('item', event, value)}
                            renderInput={(params) => (
                                <TextField {...params} label='Item' required />
                            )}
                        />
                        <Autocomplete
                            id='item-freight-uom'
                            value={newQuoteItem.freightUom}
                            options={FreightUom}
                            size='small'
                            onChange={(event, value) =>
                                handleItemValues('freightUom', event, value)
                            }
                            renderInput={(params) => (
                                <TextField {...params} label='Freight UOM' required />
                            )}
                        />
                        <TextField
                            id='item-frieght-rate'
                            type='number'
                            size='small'
                            label='Freight Rate'
                            value={newQuoteItem.freightRate}
                            onChange={(event, value) =>
                                handleItemValues('freightRate', event, value)
                            }
                        />
                        <TextField
                            id='item-lh-rate'
                            type='number'
                            size='small'
                            label='LH Rate'
                            value={newQuoteItem.lhRate}
                            onChange={(event, value) => handleItemValues('lhRate', event, value)}
                        />
                        <TextField
                            id='item-freight-qty'
                            type='number'
                            size='small'
                            label='Freight Qty'
                            value={newQuoteItem.freightQty}
                            onChange={(event, value) =>
                                handleItemValues('freightQty', event, value)
                            }
                        />
                        <TextField
                            id='item-job-number'
                            type='number'
                            size='small'
                            label='Job Number'
                            value={newQuoteItem.jobNumber}
                            onChange={(event, value) => handleItemValues('jobNumber', event, value)}
                        />
                        <TextField
                            id='item-note'
                            size='small'
                            label='Notes'
                            multiline
                            rows={3}
                            value={newQuoteItem.note}
                            onChange={(event, value) => handleItemValues('note', event, value)}
                        />
                    </Stack>
                </CardContent>
                <Divider />
                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button size='large' onClick={handleAddItemClose}>
                        Cancel
                    </Button>
                    <Button
                        size='large'
                        variant='contained'
                        onClick={handleItemSave}
                        startIcon={
                            <i className='fa-regular fa-save' style={{ fontSize: '0.8rem' }}></i>
                        }>
                        Save
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};

export default AddQuoteItem;
