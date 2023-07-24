import * as React from 'react'
import { Datepicker } from '@mobiscroll/react'
import {
    Autocomplete,
    Box,
    Button,
    Checkbox,
    Divider,
    FormControlLabel,
    Paper,
    Stack,
    TextField,
    Typography,
} from '@mui/material'
import moment from 'moment'
import { Helmet, HelmetProvider } from 'react-helmet-async'
import data from '../../common/data/data.json'
import { DataGrid } from '@mui/x-data-grid'
import { Link } from 'react-router-dom'

const { offices, Customers, Items, Addresses, OrderItems } = data

function Order() {
    const pageName = 'Orders'
    const [filterData, setFilterData] = React.useState({
        dateStart: null,
        dateEnd: null,
        office: 'Main',
        customer: null,
        item: null,
        jobNumber: undefined,
        quoteChargeTo: undefined,
        loadAt: null,
        deliverTo: null,
        showPendingOrder: false,
    })

    const orderColumns = [
        {
            field: 'deliveryDate',
            headerName: 'Delivery Date',
            flex: 1
        },
        {
            field: 'office',
            headerName: 'Office',
            flex: 1
        },
        {
            field: 'customer',
            headerName: 'Customer',
            flex: 1
        },
        {
            field: 'quote',
            headerName: 'Quote',
            flex: 1
        },
        {
            field: 'po',
            headerName: 'PO #',
            flex: 1
        },
        {
            field: 'contact',
            headerName: 'Contact',
            flex: 1
        },
        {
            field: 'chargeTo',
            headerName: 'Charge To',
            flex: 1
        },
        {
            field: 'total',
            headerName: 'Total',
            flex: 1
        },
        {
            field: 'noOfTruck',
            headerName: '# of Trucks',
            flex: 1
        }
    ]

    const handleAnswer = (field, event, value) => {
        if (value) {
            setFilterData((prev) => ({
                ...prev,
                [field]: value,
            }))
        } else {
            setFilterData((prev) => ({
                ...prev,
                [field]: event.target.value,
            }))
        }
    }

    const handleDateAnswer = (event, inst) => {
        setFilterData((prev) => ({
            ...prev,
            dateStart: moment(event.value[0]).format(
                'MM/DD/YYYY'
            ),
            dateEnd: moment(event.value[1]).format(
                'MM/DD/YYYY'
            ),
        }))
    }

    const handleCheckBox = () => {
        setFilterData((prev) => ({
            ...prev,
            showPendingOrder: !filterData.showPendingOrder,
        }))
    }

    return (
        <HelmetProvider>
            <div>
                <Helmet>
                    <meta charSet='utf-8' />
                    <title>{pageName}</title>
                    <meta
                        name='description'
                        content='Dumptruckdispatcher app'
                    />
                    <meta content='' name='author' />
                    <meta
                        property='og:title'
                        content={pageName}
                    />
                    <meta
                        property='og:image'
                        content='%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>
                <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
                    <Stack
                        direction='row'
                        spacing={2}
                        sx={{ alignItems: 'center' }}>
                        <Typography
                            variant='h6'
                            component='h2'>
                            {pageName}
                        </Typography>
                        <Divider
                            orientation='vertical'
                            flexItem
                        />
                        <Typography>
                            Manage your orders
                        </Typography>
                    </Stack>
                    <Button
                        component={Link}
                        variant='contained'
                        to='/order/details'
                        size='large'
                        startIcon={
                            <i className='fa-regular fa-plus' style={{ fontSize: '0.8rem' }}></i>
                        }>
                        Add New
                    </Button>
                </Box>
                <Paper>
                    <Box component='form' sx={{ p: 3 }}>
                        <Stack direction='column' spacing={2}>
                            <Stack
                                direction={{
                                    sx: 'column',
                                    sm: 'row',
                                }}
                                spacing={2}>
                                <Datepicker
                                    controls={['calendar']}
                                    select='range'
                                    touchUi={true}
                                    labelStyle='stacked'
                                    inputStyle='outline'
                                    onChange={
                                        handleDateAnswer
                                    }
                                    inputProps={{
                                        placeholder:
                                            'Start date - End date',
                                        label: 'Date',
                                        className:
                                            'mbsc-no-margin',
                                    }}
                                />
                                <Autocomplete
                                    id='office'
                                    value={filterData.office}
                                    options={offices}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'office',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Office'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                                <Autocomplete
                                    id='customer'
                                    value={
                                        filterData.customer
                                    }
                                    options={Customers}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'customer',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Customer'
                                            placeholder='Select Customer'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                                <Autocomplete
                                    id='item'
                                    value={filterData.item}
                                    options={Items}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'item',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Item'
                                            placeholder='Select Item'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                            </Stack>
                            <Stack
                                direction={{
                                    sx: 'column',
                                    sm: 'row',
                                }}
                                spacing={2}>
                                <TextField
                                    id='job-number'
                                    type='number'
                                    size='small'
                                    sx={{ flex: 1 }}
                                    value={
                                        filterData.jobNumber
                                    }
                                    label='Job Number'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'jobNumber',
                                            event,
                                            value
                                        )
                                    }
                                />
                                <TextField
                                    id='quote-charge-to'
                                    size='small'
                                    value={
                                        filterData.quoteChargeTo
                                    }
                                    label='Quote Charge To'
                                    sx={{ flex: 1 }}
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'quoteChargeTo',
                                            event,
                                            value
                                        )
                                    }
                                />
                                <Autocomplete
                                    id='loadAt'
                                    value={filterData.loadAt}
                                    options={Addresses}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'loadAt',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Load At'
                                            placeholder='Select'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                                <Autocomplete
                                    id='deliverTo'
                                    value={
                                        filterData.deliverTo
                                    }
                                    options={Addresses}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleAnswer(
                                            'deliverTo',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Deliver To'
                                            placeholder='Select'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                            </Stack>
                            <Stack
                                direction={{
                                    sx: 'column',
                                    sm: 'row',
                                }}
                                sx={{
                                    justifyContent:
                                        'flex-end',
                                }}
                                spacing={1}>
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            value={
                                                filterData.showPendingOrder
                                            }
                                            onClick={
                                                handleCheckBox
                                            }
                                        />
                                    }
                                    label='Show Pending Orders'
                                />
                                <Button
                                    variant='outlined'
                                    size='small'
                                    startIcon={
                                        <i
                                            className='fa-regular fa-close'
                                            style={{
                                                fontSize:
                                                    '0.8rem',
                                            }}></i>
                                    }>
                                    Clear
                                </Button>
                                <Button
                                    variant='contained'
                                    size='small'
                                    startIcon={
                                        <i
                                            className='fa-regular fa-search'
                                            style={{
                                                fontSize:
                                                    '0.8rem',
                                            }}></i>
                                    }>
                                    Search
                                </Button>
                            </Stack>
                            <Divider />
                            <DataGrid
                                rows={OrderItems}
                                columns={orderColumns}
                            />
                        </Stack>
                    </Box>
                </Paper>
            </div>
        </HelmetProvider>
    )
}

export default Order
