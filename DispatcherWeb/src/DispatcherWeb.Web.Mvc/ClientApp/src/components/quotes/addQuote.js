import * as React from 'react';
import data from '../../common/data/data.json';
import theme from '../../Theme';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import { DataGrid, GridToolbar } from '@mui/x-data-grid';
import {
    Autocomplete,
    Box,
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    Divider,
    IconButton,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
    Modal,
    Stack,
    TextField,
    Typography,
} from '@mui/material';

const { Customers, Contact, Status, HistoryItems } = data;

const columns = [
    {
        field: 'xpand',
        headerName: '',
        width: 50,
        renderCell: (params) => (
            <div>
                {params.getRowParams?.isExpanded ? (
                    <IconButton onClick={() => params.rowParams.onRowCollapse(params)}>
                        <i className='fa-regular fa-chevron-up'></i>
                    </IconButton>
                ) : (
                    <IconButton onClick={() => params.rowParams.onRowExpand(params)}>
                        <i className='fa-regular fa-chevron-down'></i>
                    </IconButton>
                )}
            </div>
        ),
    },
    { field: 'load', headerName: 'Load At', width: 200 },
    { field: 'deliver', headerName: 'Deliver To', width: 200 },
    { field: 'item', headerName: 'Item', width: 100 },
    { field: 'materialUom', headerName: 'Material UOM', width: 100 },
    { field: 'freightUom', headerName: 'Freight UOM', width: 100 },
    { field: 'designation', headerName: 'Designation', width: 100 },
    { field: 'materialRate', headerName: 'Material Rate', width: 100 },
    { field: 'freightRate', headerName: 'Freight Rate', width: 100 },
    { field: 'lhRate', headerName: 'LH Rate', width: 100 },
    { field: 'materialQty', headerName: 'Material Qty', width: 100 },
    { field: 'freightQty', headerName: 'Freight Qty', width: 100 },
    {
        field: 'actions',
        headerName: '',
        width: 50,
        renderCell: (params) => <ActionsMenuRow {...params} />,
    },
];

const historyColumns = [
    { field: 'whenChanged', headerName: 'When Changed', flex: 0.5 },
    { field: 'changedBy', headerName: 'Changed By', flex: 0.5 },
    { field: 'typeOfChange', headerName: 'Type of Change', flex: 1 },
];

const ActionsMenuRow = ({ row }) => {
    const [anchorEl, setAnchorEl] = React.useState(null);
    const handleMenuOpen = (event) => {
        setAnchorEl(event.currentTarget);
    };
    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    return (
        <div>
            <IconButton
                aria-controls={`actions-menu-${row.id}`}
                aria-haspopup='true'
                onClick={handleMenuOpen}>
                <i className='fa-regular fa-ellipsis-vertical'></i>
            </IconButton>
            <Menu
                id={`actions-menu-${row.id}`}
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleMenuClose}>
                <MenuItem onClick={handleMenuClose}>
                    <ListItemIcon>
                        <i className='fa-regular fa-edit'></i>
                    </ListItemIcon>
                    <ListItemText>Edit</ListItemText>
                </MenuItem>
                <MenuItem onClick={handleMenuClose}>
                    <ListItemIcon>
                        <i className='fa-regular fa-truck-ramp-box'></i>
                    </ListItemIcon>
                    <ListItemText>Show Deliveries</ListItemText>
                </MenuItem>
                <MenuItem onClick={handleMenuClose}>
                    <ListItemIcon>
                        <i className='fa-regular fa-trash'></i>
                    </ListItemIcon>
                    <ListItemText>Delete</ListItemText>
                </MenuItem>
            </Menu>
        </div>
    );
};

const AddQuote = ({
    quote,
    setQuote,
    isAddQuote,
    setIsAddQuote,
    setIsAddItem,
    quoteItems,
    setQuoteItems,
}) => {
    const [isDeleteItem, setIsDeleteItem] = React.useState(false);
    const [selected, setSelected] = React.useState([]);
    const [expandedRows, setExpandedRows] = React.useState([]);

    React.useEffect(() => {
        if (selected.length === 0) {
            setIsDeleteItem(true);
        } else setIsDeleteItem(false);
    }, [selected]);

    const handleClose = () => {
        setIsAddQuote(false);
    };

    const handleSelect = (newSelect) => {
        setSelected(newSelect);
    };

    const handleValues = (field, event, value) => {
        setQuote((prev) => ({
            ...prev,
            [field]: value ? value : event.target.value,
        }));
    };

    const handleTableEdit = (params) => {
        const updatedRows = quoteItems.map((quote) => {
            if (quote.id === params.id) {
                return {
                    ...quote,
                    [params.field]: params.value,
                };
            }
            return quote;
        });

        setQuoteItems(updatedRows);
    };

    const handleRowExpand = (params) => {
        setExpandedRows((prev) => [...prev, params.id]);
    };

    const handleRowCollapse = (params) => {
        setExpandedRows((prev) => {
            prev.filter((id) => id !== params.id);
        });
    };

    const isRowExpandable = (params) => {
        return params.line !== 1;
    };

    const getRowId = (params) => {
        return params.id;
    };

    const getRowParams = (params) => {
        return {
            isExpanded: expandedRows.includes(params.id),
            onRowExpand: handleRowExpand,
            onRowCollapse: handleRowCollapse,
        };
    };

    return (
        <Modal open={isAddQuote} onClose={handleClose} aria-labelledby='add-quote'>
            <Card
                sx={{
                    width: '80%',
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    maxHeight: '93vh',
                    overflow: 'auto',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title={quote.name === '' ? 'Add New Quote' : 'Edit Quote'}
                />
                <CardContent sx={{ w: '1' }}>
                    <Stack direction='column' spacing={2}>
                        <Stack direction='row' spacing={1} sx={{ justifyContent: 'end' }}>
                            <Button
                                variant='outlined'
                                size='small'
                                startIcon={
                                    <i
                                        className='fa-regular fa-copy'
                                        style={{ fontSize: '0.8rem' }}></i>
                                }>
                                Copy Quote
                            </Button>
                            <Button
                                variant='outlined'
                                size='small'
                                startIcon={
                                    <i
                                        className='fa-regular fa-print'
                                        style={{ fontSize: '0.8rem' }}></i>
                                }>
                                Print Quote
                            </Button>
                            <Button
                                variant='outlined'
                                size='small'
                                startIcon={
                                    <i
                                        className='fa-regular fa-envelope'
                                        style={{ fontSize: '0.8rem' }}></i>
                                }>
                                Email Quote
                            </Button>
                        </Stack>
                        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                            <TextField
                                id='quote-name'
                                size='small'
                                value={quote.name}
                                required
                                label='Name'
                                onChange={(event, value) => handleValues('name', event, value)}
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                }}
                            />
                            <TextField
                                id='quote-id'
                                type='number'
                                disabled={true}
                                size='small'
                                value={quote.id}
                                onChange={(event, value) => handleValues('id', event, value)}
                                label='Quote ID'
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                    bgcolor: quote.id ? '#ffffff' : theme.palette.secondary.main,
                                }}
                            />
                        </Stack>
                        <TextField
                            id='quote-description'
                            size='small'
                            value={quote.description}
                            required
                            label='Description'
                            multiline
                            onChange={(event, value) => handleValues('description', event, value)}
                        />
                        <Stack direction='row' spacing={2}>
                            <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale='de'>
                                <DatePicker
                                    label='Proposal Date'
                                    value={quote.proposalDate}
                                    onChange={(event, value) =>
                                        handleValues('proposalDate', event, value)
                                    }
                                    slotProps={{ textField: { size: 'small' } }}
                                />
                                <DatePicker
                                    label='Proposal Expiry Date'
                                    value={quote.proposalExpiryDate}
                                    onChange={(event, value) =>
                                        handleValues('proposalExpiryDate', event, value)
                                    }
                                    slotProps={{ textField: { size: 'small' } }}
                                />
                                <DatePicker
                                    label='Inactivation Date'
                                    value={quote.inactivationDate}
                                    onChange={(event, value) =>
                                        handleValues('inactivationDate', event, value)
                                    }
                                    slotProps={{ textField: { size: 'small' } }}
                                />
                            </LocalizationProvider>
                        </Stack>
                        <Stack
                            direction={{ xs: 'column', sm: 'row' }}
                            spacing={2}
                            sx={{ justifyContent: 'space-between' }}>
                            <Autocomplete
                                id='customer'
                                value={quote.customer}
                                options={Customers}
                                size='small'
                                onChange={(event, value) => handleValues('customer', event, value)}
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                }}
                                renderInput={(params) => <TextField {...params} label='Customer' />}
                            />
                            <Autocomplete
                                id='contact'
                                value={quote.contact}
                                options={Contact}
                                size='small'
                                getOptionLabel={(option) => option.name}
                                onChange={(event, value) => handleValues('contact', event, value)}
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                }}
                                renderInput={(params) => <TextField {...params} label='Contact' />}
                            />
                        </Stack>
                        <Stack
                            direction={{ xs: 'column', sm: 'row' }}
                            spacing={2}
                            sx={{ justifyContent: 'space-between' }}>
                            <Autocomplete
                                id='status'
                                value={quote.status}
                                options={Status}
                                size='small'
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                }}
                                onChange={(event, value) => handleValues('status', event, value)}
                                renderInput={(params) => <TextField {...params} label='Status' />}
                            />
                            <Autocomplete
                                id='sales-person'
                                value={quote.salesPerson}
                                options={[]}
                                size='small'
                                sx={{
                                    flexBasis: { xs: '100%', sm: '50%' },
                                }}
                                onChange={(event, value) =>
                                    handleValues('salesPerson', event, value)
                                }
                                renderInput={(params) => (
                                    <TextField {...params} label='Sales Person' />
                                )}
                            />
                        </Stack>
                        <TextField
                            id='quote-po-number'
                            size='small'
                            value={quote.poNumber}
                            required
                            label='PO Number'
                            onChange={(event, value) => handleValues('poNumber', event, value)}
                        />
                        <Divider sx={{ my: 3 }} />
                        <Box>
                            <Stack
                                direction='row'
                                sx={{
                                    alignItems: 'center',
                                    justifyContent: 'space-between',
                                    mb: 1,
                                }}>
                                <Typography
                                    variant='subtitle1'
                                    component='h3'
                                    sx={{ fontWeight: 'medium' }}>
                                    Quote Items
                                </Typography>
                                <Stack direction='row' spacing={1}>
                                    <Button
                                        variant='outlined'
                                        size='small'
                                        onClick={() => setIsAddItem(true)}
                                        startIcon={
                                            <i
                                                className='fa-regular fa-plus'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        }>
                                        Add Quote Item
                                    </Button>
                                    <Button
                                        variant='outlined'
                                        sx={{ display: isDeleteItem ? 'none' : 'inherit' }}
                                        size='small'
                                        startIcon={
                                            <i
                                                className='fa-regular fa-trash'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        }>
                                        Delete
                                    </Button>
                                </Stack>
                            </Stack>
                            <DataGrid
                                rows={quoteItems}
                                columns={columns}
                                selectionModel={selected}
                                onRowSelectionModelChange={handleSelect}
                                onCellEditCommit={handleTableEdit}
                                initialState={{
                                    ...data.initialState,
                                    pagination: { paginationModel: { pageSize: 5 } },
                                }}
                                pageSizeOptions={[5, 10, 12]}
                                checkboxSelection
                                isRowExpandable={isRowExpandable}
                                getRowId={getRowId}
                                getRowParams={getRowParams}
                                components={{
                                    Toolbar: GridToolbar,
                                }}
                            />
                        </Box>
                        <TextField
                            id='quote-comment'
                            size='small'
                            value={quote.comments}
                            multiline
                            rows={3}
                            label='Comments'
                            sx={{ width: '100%' }}
                            onChange={(event, value) => handleValues('comments', event, value)}
                        />
                        <Stack direction='row'>
                            <Autocomplete
                                id='quote-insert-canned-text'
                                value={quote.insertCannedText}
                                options={[]}
                                size='small'
                                onChange={(event, value) =>
                                    handleValues('insertCannedText', event, value)
                                }
                                sx={{
                                    flexBasis: { xs: '100%', sm: '49%' },
                                }}
                                renderInput={(params) => (
                                    <TextField {...params} label='Insert Canned Text' />
                                )}
                            />
                        </Stack>
                        <TextField
                            id='quote-notes'
                            size='small'
                            value={quote.notes}
                            multiline
                            rows={3}
                            label='Notes'
                            sx={{ width: '100%' }}
                            onChange={(event, value) => handleValues('notes', event, value)}
                        />
                    </Stack>
                    {quote.id !== 0 ? (
                        <Stack spacing={2} sx={{ mt: 3 }}>
                            <Divider />
                            <Typography
                                variant='subtitle1'
                                component='h3'
                                sx={{ fontWeight: 'medium' }}>
                                History
                            </Typography>
                            <DataGrid
                                rows={HistoryItems}
                                columns={historyColumns}
                                initialState={{
                                    ...data.initialState,
                                    pagination: { paginationModel: { pageSize: 5 } },
                                }}
                                pageSizeOptions={[5, 10, 12]}
                            />
                        </Stack>
                    ) : null}
                </CardContent>
                <Divider />
                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button size='large' onClick={handleClose}>
                        Cancel
                    </Button>
                    <Button
                        size='large'
                        variant='contained'
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

export default AddQuote;
