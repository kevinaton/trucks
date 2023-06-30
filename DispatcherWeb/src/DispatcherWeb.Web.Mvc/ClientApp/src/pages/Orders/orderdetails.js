import * as React from 'react';
import {
    Autocomplete,
    Box,
    Button,
    Checkbox,
    Collapse,
    Divider,
    FormControlLabel,
    IconButton,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Tooltip,
    Typography,
} from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers';
import { useState } from 'react';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import moment from 'moment';
import { Tablecell } from '../../components/DTComponents';
import { grey } from '@mui/material/colors';
import data from '../../common/data/data.json';
import theme from '../../Theme';
import InternalNotes from '../../components/common/modals/internalNotes';
import SelectDeliveryDate from '../../components/common/modals/selectDeliveryDate';
import SendEmail from '../../components/common/modals/sendEmail';
import AddOrderItem from '../../components/common/modals/addOrderItem';
import EditNote from '../../components/common/modals/editNote';

const { Customers, offices, Quotes, Contact, ScheduleData, CannedText, Priority } = data;

const OrderDetails = (editData) => {
    const [printAnchor, setPrintAnchor] = useState(null);
    const [orderAnchor, setOrderAnchor] = useState(null);
    const isPrint = Boolean(printAnchor);
    const isOrderMenu = Boolean(orderAnchor);
    const [receiptAnchor, setReceiptAnchor] = useState(null);
    const isReceipt = Boolean(receiptAnchor);
    const [isInternalNotes, setIsInternalNotes] = useState(false);
    const [isCopy, setIsCopy] = useState(false);
    const [isEmail, setIsEmail] = useState(false);
    const [isTable, setIsTable] = useState(Array(ScheduleData.length).fill(false));
    const [isAddOrder, setIsAddOrder] = useState(false);
    const [isNote, setIsNote] = useState(false);
    const [orderData, setOrderdata] = useState({
        id: 90902293,
        accountNumber: 23123123120,
        date: moment(),
        office: 'Main',
        customer: 'ABC Company',
        quote: 'Quote 1',
        contact: { name: 'Michael Fox', number: 1022993484 },
        poNumber: 3,
        salesTaxRate: undefined,
        salesTax: 1,
        total: 800,
        chargeTo: '',
        cannedText: '',
        comments: 'Lorem ipsum dolor',
        priority: 'Medium',
        createdDate: '06/05/2023 9:45PM',
        createdBy: 'Admin',
        lastModified: '06/18/2023 12:56PM',
        lastModifiedBy: 'Admin',
    });

    const handleSave = () => {
        console.log(orderData);
    };

    const handleCollapseToggle = (index) => {
        const updatedIsTable = [...isTable];
        updatedIsTable[index] = !updatedIsTable[index];
        setIsTable(updatedIsTable);
    };

    const handleAnswer = (field, event, value) => {
        if (value) {
            setOrderdata((prev) => ({
                ...prev,
                [field]: value,
            }));
        } else {
            setOrderdata((prev) => ({
                ...prev,
                [field]: event.target.value,
            }));
        }
    };

    return (
        <HelmetProvider>
            <div>
                <Helmet>
                    <meta charSet='utf-8' />
                    <title>Order</title>
                    <meta name='description' content='Dumptruckdispatcher app' />
                    <meta content='' name='author' />
                    <meta property='og:title' content='Add/Edit Order' />
                    <meta
                        property='og:image'
                        content='%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>

                {/* Modals */}
                <InternalNotes
                    isInternalNotes={isInternalNotes}
                    setIsInternalNotes={setIsInternalNotes}
                />
                <SelectDeliveryDate isOpen={isCopy} setIsOpen={setIsCopy} />
                <SendEmail isOpen={isEmail} setIsOpen={setIsEmail} />
                <AddOrderItem isOpen={isAddOrder} setIsOpen={setIsAddOrder} />
                <EditNote isOpen={isNote} setIsOpen={setIsNote} />
                {/* Modals */}

                <Stack direction='row' spacing={2} sx={{ mb: 2, alignItems: 'center' }}>
                    <Typography variant='h6' component='h2'>
                        Orders
                    </Typography>
                    <Divider orientation='vertical' flexItem />
                    <Typography>Add or edit an order</Typography>
                </Stack>
                <Paper>
                    <Box component='form' sx={{ p: 3 }}>
                        <Stack direction='column' spacing={3}>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='order-id'
                                    type='number'
                                    size='small'
                                    value={orderData.id}
                                    disabled={orderData.id ? true : false}
                                    label='ID'
                                    sx={{
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: orderData.id
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                                <Button
                                    variant='outlined'
                                    size='small'
                                    startIcon={<i className='fa-regular fa-copy'></i>}
                                    onClick={() => {
                                        setIsCopy(true);
                                    }}>
                                    Copy
                                </Button>
                                <Button
                                    id='print-button'
                                    variant='outlined'
                                    size='small'
                                    aria-controls={isPrint ? 'print-menu' : undefined}
                                    aria-haspopup='true'
                                    aria-expanded={isPrint ? 'true' : undefined}
                                    startIcon={<i className='fa-regular fa-print'></i>}
                                    endIcon={
                                        isPrint ? (
                                            <i
                                                className='fa-regular fa-chevron-up'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        ) : (
                                            <i
                                                className='fa-regular fa-chevron-down'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        )
                                    }
                                    onClick={(event) => {
                                        setPrintAnchor(event.currentTarget);
                                    }}>
                                    Print
                                </Button>
                                <Menu
                                    id='print-menu'
                                    anchorEl={printAnchor}
                                    open={isPrint}
                                    onClose={() => {
                                        setPrintAnchor(null);
                                    }}
                                    MenuListProps={{
                                        'aria-labelledby': 'print-button',
                                    }}>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-plus icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Order with no prices</ListItemText>
                                    </MenuItem>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-folder icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Order with combined prices</ListItemText>
                                    </MenuItem>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-folders icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Order with separate prices</ListItemText>
                                    </MenuItem>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-building icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Order for Back Office</ListItemText>
                                    </MenuItem>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-file-spreadsheet icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Delivery Report</ListItemText>
                                    </MenuItem>
                                </Menu>
                                <Button
                                    variant='outlined'
                                    size='small'
                                    onClick={() => setIsEmail(true)}
                                    startIcon={<i className='fa-regular fa-envelope'></i>}>
                                    Email Order
                                </Button>
                                <Button
                                    id='receipt-button'
                                    variant='outlined'
                                    size='small'
                                    aria-controls={isReceipt ? 'receipt-menu' : undefined}
                                    aria-haspopup='true'
                                    aria-expanded={isReceipt ? 'true' : undefined}
                                    startIcon={<i className='fa-regular fa-receipt'></i>}
                                    endIcon={
                                        isReceipt ? (
                                            <i
                                                className='fa-regular fa-chevron-up'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        ) : (
                                            <i
                                                className='fa-regular fa-chevron-down'
                                                style={{ fontSize: '0.8rem' }}></i>
                                        )
                                    }
                                    onClick={(event) => {
                                        setReceiptAnchor(event.currentTarget);
                                    }}>
                                    Receipt
                                </Button>
                                <Menu
                                    id='receipt-menu'
                                    anchorEl={receiptAnchor}
                                    open={isReceipt}
                                    onClose={() => {
                                        setReceiptAnchor(null);
                                    }}
                                    MenuListProps={{
                                        'aria-labelledby': 'print-button',
                                    }}>
                                    <MenuItem>
                                        <ListItemIcon>
                                            <i className='fa-regular fa-plus icon'></i>
                                        </ListItemIcon>
                                        <ListItemText>Create New Receipt</ListItemText>
                                    </MenuItem>
                                </Menu>
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='account-number'
                                    type='number'
                                    disabled={true}
                                    size='small'
                                    value={orderData.accountNumber}
                                    label='Account Number'
                                    sx={{
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: theme.palette.secondary.main,
                                    }}
                                />
                                <FormControlLabel control={<Checkbox />} label='Pending Order' />
                            </Stack>
                            <Stack direction={{ sx: 'column', sm: 'row' }} spacing={2}>
                                <LocalizationProvider
                                    dateAdapter={AdapterMoment}
                                    adapterLocale='de'>
                                    <DatePicker
                                        id='deliveryDate'
                                        value={orderData.date}
                                        onChange={(newVal) => {
                                            orderData.date = newVal;
                                        }}
                                        format='MM/DD/YYYY'
                                        label='Delivery date'
                                        slotProps={{ textField: { size: 'small', required: true } }}
                                        sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                    />
                                </LocalizationProvider>
                            </Stack>
                            <Stack direction={{ sx: 'column', sm: 'row' }} spacing={2}>
                                <Autocomplete
                                    id='office'
                                    value={orderData.office}
                                    options={offices}
                                    size='small'
                                    onChange={(event, value) =>
                                        handleAnswer('office', event, value)
                                    }
                                    renderInput={(params) => (
                                        <TextField {...params} label='Office' />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                                <Autocomplete
                                    id='customer'
                                    value={orderData.customer}
                                    size='small'
                                    options={Customers}
                                    onChange={(event, value) =>
                                        handleAnswer('customer', event, value)
                                    }
                                    renderInput={(params) => (
                                        <TextField {...params} label='Customer' required />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                            </Stack>
                            <Stack direction={{ sx: 'column', sm: 'row' }} spacing={2}>
                                <Autocomplete
                                    id='quote'
                                    value={orderData.quote}
                                    options={Quotes}
                                    size='small'
                                    renderInput={(params) => (
                                        <TextField {...params} label='Quote' />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                            </Stack>
                            <Stack direction={{ sx: 'column', sm: 'row' }} spacing={2}>
                                <Autocomplete
                                    id='contact'
                                    value={orderData.contact}
                                    options={Contact}
                                    size='small'
                                    getOptionLabel={(option) => option.name}
                                    onChange={(event, value) =>
                                        handleAnswer('contact', event, value)
                                    }
                                    isOptionEqualToValue={(option, value) =>
                                        option.name === value.name
                                    }
                                    renderInput={(params) => (
                                        <TextField {...params} label='Contact' />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                                <TextField
                                    id='contact-number'
                                    type='number'
                                    disabled
                                    size='small'
                                    value={orderData.contact.number}
                                    label=''
                                    sx={{
                                        flexBasis: {
                                            xs: '100%',
                                            sm: '49%',
                                            bgcolor: theme.palette.secondary.main,
                                        },
                                    }}
                                />
                            </Stack>
                            <Stack direction={{ sx: 'column', sm: 'row' }}>
                                <TextField
                                    id='po'
                                    type='number'
                                    value={orderData.poNumber}
                                    label='PO Number'
                                    size='small'
                                    onChange={(event, value) =>
                                        handleAnswer('poNumber', event, value)
                                    }
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                            </Stack>
                        </Stack>
                        <Divider sx={{ my: 3 }} />
                        <Stack direction='column' spacing={3}>
                            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Typography
                                    variant='subtitle1'
                                    component='h3'
                                    sx={{ fontWeight: 'medium' }}>
                                    Order Items
                                </Typography>
                                <Button
                                    variant='contained'
                                    onClick={() => {
                                        setIsAddOrder(true);
                                    }}
                                    startIcon={<i className='fa-regular fa-plus'></i>}>
                                    Add Order Item
                                </Button>
                            </Box>
                            <TableContainer>
                                <Table sx={{ mb: 3 }} aria-label='order-items'>
                                    <TableHead sx={{ bgcolor: grey[100] }}>
                                        <TableRow>
                                            <Tablecell label='' value='' />
                                            <Tablecell label='Line #' value='Line #' />
                                            <Tablecell label='Load at' value='Load at' />
                                            <Tablecell label='Deliver to' value='Deliver to' />
                                            <Tablecell
                                                label='Comments'
                                                value={<i className='fa-regular fa-message'></i>}
                                            />
                                            <Tablecell label='action' value='' />
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {ScheduleData.map((item, index) => {
                                            return (
                                                <React.Fragment key={index}>
                                                    <TableRow hover={true}>
                                                        <Tablecell
                                                            value={
                                                                <IconButton
                                                                    onClick={() =>
                                                                        handleCollapseToggle(index)
                                                                    }>
                                                                    {isTable[index] ? (
                                                                        <i className='fa-regular fa-chevron-up'></i>
                                                                    ) : (
                                                                        <i className='fa-regular fa-chevron-down'></i>
                                                                    )}
                                                                </IconButton>
                                                            }
                                                        />
                                                        <Tablecell value={item.line} />
                                                        <Tablecell value={item.load} />
                                                        <Tablecell value={item.deliver} />
                                                        <Tablecell
                                                            value={
                                                                <IconButton
                                                                    onClick={() => {
                                                                        setIsNote(true);
                                                                    }}>
                                                                    <i className='fa-regular fa-message'></i>
                                                                </IconButton>
                                                            }
                                                        />
                                                        <Tablecell
                                                            value={
                                                                <IconButton
                                                                    onClick={(event) => {
                                                                        setOrderAnchor(
                                                                            event.currentTarget
                                                                        );
                                                                    }}>
                                                                    <i className='fa-regular fa-ellipsis-vertical'></i>
                                                                </IconButton>
                                                            }
                                                        />
                                                        <Menu
                                                            id='order-menu'
                                                            anchorEl={orderAnchor}
                                                            open={isOrderMenu}
                                                            onClose={() => {
                                                                setOrderAnchor(null);
                                                            }}
                                                            MenuListProps={{
                                                                'aria-labelledby':
                                                                    'order-menu-button',
                                                            }}>
                                                            <MenuItem>
                                                                <ListItemIcon>
                                                                    <i className='fa-regular fa-edit secondary-icon'></i>
                                                                </ListItemIcon>
                                                                <ListItemText>Edit</ListItemText>
                                                            </MenuItem>
                                                            <MenuItem>
                                                                <ListItemIcon>
                                                                    <i className='fa-regular fa-ticket secondary-icon'></i>
                                                                </ListItemIcon>
                                                                <ListItemText>Tickets</ListItemText>
                                                            </MenuItem>
                                                            <MenuItem>
                                                                <ListItemIcon>
                                                                    <i className='fa-regular fa-trash secondary-icon'></i>
                                                                </ListItemIcon>
                                                                <ListItemText>Delete</ListItemText>
                                                            </MenuItem>
                                                        </Menu>
                                                    </TableRow>
                                                    <TableRow>
                                                        <TableCell
                                                            style={{
                                                                padding: 0,
                                                                paddingTop: 0,
                                                                paddingBottom: 0,
                                                                margin: 0,
                                                                backgroundColor:
                                                                    theme.palette.grey[50],
                                                                borderTop: isTable[index]
                                                                    ? `1px solid ${theme.palette.grey[300]}`
                                                                    : 'none',
                                                            }}
                                                            colSpan={6}>
                                                            <Collapse
                                                                in={isTable[index]}
                                                                timeout='auto'
                                                                unmountOnExit>
                                                                <Box>
                                                                    <Stack
                                                                        direction={{
                                                                            xs: 'column',
                                                                            sm: 'row',
                                                                        }}>
                                                                        <Table
                                                                            size='small'
                                                                            sx={{
                                                                                width: {
                                                                                    xs: '100%',
                                                                                    sm: '80%',
                                                                                },
                                                                            }}>
                                                                            <TableBody>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Item
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {item.item}
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Designation
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.designation
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <Tooltip title='Requested number of trucks'>
                                                                                        <TableCell>
                                                                                            <i className='fa-regular fa-truck'></i>
                                                                                        </TableCell>
                                                                                    </Tooltip>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.required
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Run until
                                                                                        stopped
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        <FormControlLabel
                                                                                            control={
                                                                                                <Checkbox
                                                                                                    sx={{
                                                                                                        py: 0,
                                                                                                    }}
                                                                                                    checked={
                                                                                                        item.runUntilStopped
                                                                                                    }
                                                                                                />
                                                                                            }
                                                                                        />
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <Tooltip title='Time on job'>
                                                                                        <TableCell>
                                                                                            <i className='fa-regular fa-clock'></i>
                                                                                        </TableCell>
                                                                                    </Tooltip>
                                                                                    <TableCell>
                                                                                        {item.time}
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <Tooltip title='Is based on production pay'>
                                                                                        <TableCell>
                                                                                            <i className='fa-regular fa-dollar-sign'></i>
                                                                                        </TableCell>
                                                                                    </Tooltip>
                                                                                    <TableCell>
                                                                                        <FormControlLabel
                                                                                            control={
                                                                                                <Checkbox
                                                                                                    sx={{
                                                                                                        py: 0,
                                                                                                    }}
                                                                                                    checked={
                                                                                                        item.cash
                                                                                                    }
                                                                                                />
                                                                                            }
                                                                                        />
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Material UOM
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.materialUom
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                            borderBottom:
                                                                                                'none',
                                                                                        }}>
                                                                                        Freight UOM
                                                                                    </TableCell>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            borderBottom:
                                                                                                'none',
                                                                                        }}>
                                                                                        {
                                                                                            item.freightUom
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                            </TableBody>
                                                                        </Table>
                                                                        <Table size='small'>
                                                                            <TableBody>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Material
                                                                                        Rate
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.materialRate
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Freight Rate
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.freightRate
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        LH Rate
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.lhRate
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Material
                                                                                        Quantity
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.materialQty
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Freight
                                                                                        Quantity
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.frieghtQty
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Material
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.material
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            width: {
                                                                                                xs: '30%',
                                                                                                sm: '40%',
                                                                                            },
                                                                                            fontWeight:
                                                                                                'bold',
                                                                                        }}>
                                                                                        Freight
                                                                                    </TableCell>
                                                                                    <TableCell>
                                                                                        {
                                                                                            item.freight
                                                                                        }
                                                                                    </TableCell>
                                                                                </TableRow>
                                                                                <TableRow
                                                                                    sx={{
                                                                                        height: 32,
                                                                                    }}>
                                                                                    <TableCell
                                                                                        sx={{
                                                                                            height: '30px',
                                                                                            borderBottom:
                                                                                                'none',
                                                                                        }}></TableCell>
                                                                                </TableRow>
                                                                            </TableBody>
                                                                        </Table>
                                                                    </Stack>
                                                                </Box>
                                                            </Collapse>
                                                        </TableCell>
                                                    </TableRow>
                                                </React.Fragment>
                                            );
                                        })}
                                    </TableBody>
                                </Table>
                            </TableContainer>

                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='sales-tax-rate'
                                    type='number'
                                    size='small'
                                    value={orderData.salesTaxRate}
                                    disabled={orderData.salesTaxRate ? true : false}
                                    label='Sales Tax Rate'
                                    onChange={(event, value) =>
                                        handleAnswer('salesTaxRate', event, value)
                                    }
                                    sx={{
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: orderData.salesTaxRate
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                                <TextField
                                    id='sales-tax'
                                    type='number'
                                    size='small'
                                    value={orderData.salesTax}
                                    disabled={orderData.salesTax ? true : false}
                                    label='Sales Tax'
                                    onChange={(event, value) =>
                                        handleAnswer('salesTax', event, value)
                                    }
                                    sx={{
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: orderData.salesTax
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='total'
                                    type='number'
                                    size='small'
                                    value={orderData.total}
                                    disabled={orderData.total ? true : false}
                                    label='TOTAL'
                                    onChange={(event, value) => handleAnswer('total', event, value)}
                                    sx={{
                                        mt: 2,
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: orderData.total
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                            </Stack>

                            <Divider sx={{ my: 3 }} />
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='charge-to'
                                    size='small'
                                    value={orderData.chargeTo}
                                    disabled={orderData.chargeTo ? true : false}
                                    label='Charge To'
                                    onChange={(event, value) =>
                                        handleAnswer('chargeTo', event, value)
                                    }
                                    sx={{
                                        flexBasis: { xs: '100%', sm: '49%' },
                                        bgcolor: orderData.chargeTo
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                                <Autocomplete
                                    id='canned-text'
                                    value={orderData.cannedText}
                                    size='small'
                                    options={CannedText}
                                    onChange={(event, value) =>
                                        handleAnswer('cannedText', event, value)
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Insert Canned Text'
                                            required
                                        />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='comment'
                                    size='small'
                                    value={orderData.comment}
                                    disabled={orderData.comment ? true : false}
                                    multiline
                                    rows={4}
                                    label='Comments'
                                    onChange={(event, value) =>
                                        handleAnswer('comment', event, value)
                                    }
                                    sx={{
                                        width: '100%',
                                        bgcolor: orderData.comment
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <Autocomplete
                                    id='canned-text'
                                    value={orderData.priority}
                                    size='small'
                                    options={Priority}
                                    onChange={(event, value) =>
                                        handleAnswer('priority', event, value)
                                    }
                                    renderInput={(params) => (
                                        <TextField {...params} label='Priority' />
                                    )}
                                    sx={{ flexBasis: { xs: '100%', sm: '49%' } }}
                                />
                                <Button
                                    variant='outlined'
                                    onClick={() => {
                                        setIsInternalNotes(true);
                                    }}>
                                    Internal Notes
                                </Button>
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='created-date'
                                    size='small'
                                    value={orderData.createdDate}
                                    disabled={orderData.createdDate ? true : false}
                                    label='Created'
                                    sx={{
                                        width: '100%',
                                        bgcolor: orderData.createdDate
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                                <TextField
                                    id='created-by'
                                    size='small'
                                    value={orderData.createdBy}
                                    disabled={orderData.createdBy ? true : false}
                                    label='Created By'
                                    sx={{
                                        width: '100%',
                                        bgcolor: orderData.createdBy
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                            </Stack>
                            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                                <TextField
                                    id='last-modified'
                                    size='small'
                                    value={orderData.lastModified}
                                    disabled={orderData.lastModified ? true : false}
                                    label='Last Modified'
                                    sx={{
                                        width: '100%',
                                        bgcolor: orderData.lastModified
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                                <TextField
                                    id='last-modified-by'
                                    size='small'
                                    value={orderData.lastModifiedBy}
                                    disabled={orderData.lastModifiedBy ? true : false}
                                    label='Last Modified By'
                                    sx={{
                                        width: '100%',
                                        bgcolor: orderData.lastModifiedBy
                                            ? theme.palette.secondary.main
                                            : '#ffffff',
                                    }}
                                />
                            </Stack>
                        </Stack>
                    </Box>
                    <Divider />
                    <Box sx={{ p: 2, textAlign: 'right' }}>
                        <Button
                            variant='contained'
                            size='large'
                            onClick={handleSave}
                            startIcon={<i className='fa-regular fa-save'></i>}>
                            Save
                        </Button>
                    </Box>
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default OrderDetails;
