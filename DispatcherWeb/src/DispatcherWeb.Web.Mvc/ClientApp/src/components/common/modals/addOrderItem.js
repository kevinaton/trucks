import {
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    IconButton,
    Modal,
} from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import * as React from 'react';
import { grey } from '@mui/material/colors';
import data from '../../../common/data/data.json';

const { dispatches } = data;

const AddOrderItem = ({ isOpen, setIsOpen }) => {
    const [selected, setSelected] = React.useState([]);
    const handleClose = () => {
        setIsOpen(false);
    };

    const handleSave = () => {
        setIsOpen(false);
        console.log(selected);
    };

    const columns = [
        { field: 'loadat', headerName: 'Load At', width: 280 },
        { field: 'deliverto', headerName: 'Deliver To', width: 280 },
        { field: 'item', headerName: 'Item', width: 120 },
    ];

    const handleSelect = (newSelect) => {
        setSelected(newSelect);
    };

    return (
        <Modal open={isOpen} onClose={handleClose} aria-labelledby='internal-notes'>
            <Card
                sx={{
                    minWidth: 800,
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='Add Order Item'
                    subheader='Check all of the quoted line items you want to have added to this order'
                />
                <CardContent>
                    <DataGrid
                        rows={dispatches}
                        columns={columns}
                        selectionModel={selected}
                        onRowSelectionModelChange={handleSelect}
                        initialState={{
                            ...data.initialState,
                            pagination: { paginationModel: { pageSize: 5 } },
                        }}
                        pageSizeOptions={[5, 10, 12]}
                        checkboxSelection
                    />
                </CardContent>
                <CardActions
                    sx={{ justifyContent: 'end', borderTop: `1px solid ${grey[300]}`, py: 2 }}>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button
                        variant='contained'
                        onClick={handleSave}
                        startIcon={<i className='fa-regular fa-add'></i>}>
                        Add Selected Items
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};

export default AddOrderItem;
