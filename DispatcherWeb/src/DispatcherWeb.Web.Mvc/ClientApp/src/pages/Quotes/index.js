import * as React from 'react';
import {
    Autocomplete,
    Box,
    Button,
    Collapse,
    Divider,
    IconButton,
    ListItem,
    ListItemButton,
    ListItemText,
    Menu,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TablePagination,
    TableRow,
    TextField,
    Typography,
} from '@mui/material'
import { Helmet, HelmetProvider } from 'react-helmet-async';
import theme from '../../Theme'
import data from '../../common/data/data.json';
import AddQuote from '../../components/quotes/addQuote';
import AddQuoteItem from '../../components/quotes/addQuoteItem';
import { Tablecell } from '../../components/DTComponents'
import moment from 'moment';

const { ScheduleData, Customers, SalesRep, QuoteItems } = data

const defaultQuoteValues = {
    id: 0,
    quoteName: '',
    description: '',
    proposalDate: moment(),
    proposalExpiryDate: moment().add(1, 'months'),
    inactivationDate: undefined,
    customer: null,
    contact: { id: 0, name: '', number: 0 },
    status: 'Pending',
    salesRep: '',
    po: 0,
    comments: '',
    insertCannedText: '',
    notes: '',
}

const quoteColumns = [
    { field: 'expandable', headerName: '', flex: 0.2 },
    { field: 'id', headerName: 'ID', flex: 0.4 },
    {
        field: 'quoteName',
        headerName: 'Quote Name',
        flex: 0.7,
    },
    {
        field: 'customer',
        headerName: 'Customer Name',
        flex: 0.7,
    },
    { field: 'date', headerName: 'Date', flex: 0.5 },
    {
        field: 'description',
        headerName: 'Description',
        flex: 1,
    },
    { field: 'salesRep', headerName: 'Sales Rep', flex: 0.7 },
    {
        field: 'action',
        headerName: 'Action',
        flex: 0.3,
        align: 'right',
    },
]

function Quotes() {
    const pageName = 'Quotes';
    const [isQuote, setIsQuote] = React.useState(false)
    const [isAddItem, setIsAddItem] = React.useState(false)
    const [page, setPage] = React.useState(0)
    const [rowsPerPage, setRowsPerPage] = React.useState(10)
    const [quote, setQuote] = React.useState(defaultQuoteValues)
    const [quoteItems, setQuoteItems] = React.useState([])
    const [newQuoteItem, setNewQuoteItem] = React.useState({
        designation: null,
        load: null,
        deliver: null,
        item: null,
        freightUom: null,
        freightRate: 0,
        lhRate: 0,
        freightQty: 0,
        jobNumber: 0,
        note: '',
    })
    const [filterData, setFilterData] = React.useState({
        id: undefined,
        customer: null,
        salesRep: null,
        multiSearch: '',
    })
    const [hoveredRow, setHoveredRow] = React.useState(null)
    const [isExpand, setIsExpand] = React.useState([])
    const [actionAnchor, setActionAnchor] =
        React.useState(null)
    const actionOpen = Boolean(actionAnchor)

    const handleAddEditQuote = (state, edit) => {
        if (edit.name === '') {
            setQuote(edit)
            setIsQuote(state)
        } else {
            if (!edit.proposalDate) {
                setQuote({
                    ...edit,
                    proposalDate: moment(),
                    proposalExpiryDate: moment().add(
                        1,
                        'months'
                    ),
                })
                setIsQuote(state)
            } else {
                setQuote(edit)
                setIsQuote(state)
            }
        }
    }

    const handleFilter = (field, event, value) => {
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

    React.useEffect(() => {
        setQuoteItems(ScheduleData);
    }, [])

    // Handle row hover on table
    const handleRowHover = (index) => {
        setHoveredRow(index)
    }
    const handleRowLeave = () => {
        setHoveredRow(null)
    }

    // Handle action on table rows
    const handleActionClick = (event) => {
        setActionAnchor(event.currentTarget)
    }
    const handleActionClose = () => {
        setActionAnchor(null)
    }

    const handleRowClick = (index) => {
        setIsExpand((prevExpand) => {
            const updatedExpand = [...prevExpand]
            updatedExpand[index] = !updatedExpand[index]
            return updatedExpand
        })
    }

    return (
        <HelmetProvider>
            <div>
                <Helmet>
                    <meta charSet='utf-8' />
                    <title>{pageName}</title>
                    <meta name='description' content='Dumptruckdispatcher app' />
                    <meta content='' name='author' />
                    <meta property='og:title' content={pageName} />
                    <meta
                        property='og:image'
                        content='%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>

                {/* Modals */}
                <AddQuoteItem
                    isAddItem={isAddItem}
                    setIsAddItem={setIsAddItem}
                    newQuoteItem={newQuoteItem}
                    setNewQuoteItem={setNewQuoteItem}
                />

                <AddQuote
                    isAddQuote={isQuote}
                    setIsAddQuote={setIsQuote}
                    quote={quote}
                    setQuote={setQuote}
                    setIsAddItem={setIsAddItem}
                    quoteItems={quoteItems}
                    setQuoteItems={setQuoteItems}
                />

<Box
                    sx={{
                        mb: 2,
                        display: 'flex',
                        justifyContent: 'space-between',
                    }}>
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
                            Manage your quotes
                        </Typography>
                    </Stack>
                    <Button
                        variant='contained'
                        size='large'
                        onClick={() =>
                            handleAddEditQuote(
                                true,
                                defaultQuoteValues
                            )
                        }
                        startIcon={
                            <i
                                className='fa-regular fa-plus'
                                style={{
                                    fontSize: '0.8rem',
                                }}></i>
                        }>
                        Add New
                    </Button>
                </Box>
                <Paper sx={{ minHeight: 700 }}>
                    <Box component='form' sx={{ p: 3 }}>
                        <Stack direction='column' spacing={2}>
                            <Stack
                                direction={{
                                    sx: 'column',
                                    sm: 'row',
                                }}
                                spacing={2}>
                                <TextField
                                    id='quote-id'
                                    type='number'
                                    size='small'
                                    sx={{ flex: 1 }}
                                    value={filterData.id}
                                    label='Quote Id'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleFilter(
                                            'id',
                                            event,
                                            value
                                        )
                                    }
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
                                        handleFilter(
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
                                    id='sales-rep'
                                    value={
                                        filterData.salesRep
                                    }
                                    options={SalesRep}
                                    size='small'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleFilter(
                                            'salesRep',
                                            event,
                                            value
                                        )
                                    }
                                    renderInput={(params) => (
                                        <TextField
                                            {...params}
                                            label='Sales Rep'
                                            placeholder='Select Sales Rep'
                                        />
                                    )}
                                    sx={{
                                        flex: 1,
                                    }}
                                />
                                <TextField
                                    id='multisearch'
                                    size='small'
                                    placeholder='Search'
                                    value={
                                        filterData.multiSearch
                                    }
                                    label='Multisearch'
                                    onChange={(
                                        event,
                                        value
                                    ) =>
                                        handleFilter(
                                            'multiSearch',
                                            event,
                                            value
                                        )
                                    }
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
                                <Button
                                    variant='outlined'
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
                            <Paper sx={{ width: '100%' }}>
                                <TableContainer
                                    component={Box}>
                                    <Table aria-label='quote table'>
                                        <TableHead>
                                            <TableRow
                                                sx={{
                                                    '& th': {
                                                        backgroundColor:
                                                            theme
                                                                .palette
                                                                .grey[100],
                                                    },
                                                }}>
                                                {quoteColumns.map(
                                                    (
                                                        column,
                                                        index
                                                    ) => (
                                                        <React.Fragment
                                                            key={
                                                                index
                                                            }>
                                                            <Tablecell
                                                                label={
                                                                    column.headerName
                                                                }
                                                                value={
                                                                    column.headerName
                                                                }
                                                                style={{
                                                                    flex: column.flex,
                                                                    textAlign: column.align
                                                                }}
                                                            />
                                                        </React.Fragment>
                                                    )
                                                )}
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {QuoteItems.map(
                                                (
                                                    item,
                                                    index
                                                ) => (
                                                    <React.Fragment
                                                        key={
                                                            index
                                                        }>
                                                        <TableRow
                                                            hover={
                                                                true
                                                            }
                                                            onMouseEnter={() =>
                                                                handleRowHover(
                                                                    index
                                                                )
                                                            }
                                                            onMouseLeave={() =>
                                                                handleRowLeave
                                                            }
                                                            sx={{
                                                                backgroundColor:
                                                                    hoveredRow ===
                                                                    index
                                                                        ? theme
                                                                              .palette
                                                                              .action
                                                                              .hover
                                                                        : '#ffffff',
                                                                '&.MuiTableRow-root:hover':
                                                                    {
                                                                        backgroundColor:
                                                                            theme
                                                                                .palette
                                                                                .action
                                                                                .hover,
                                                                    },
                                                            }}>
                                                            <TableCell
                                                                onClick={() =>
                                                                    handleRowClick(
                                                                        index
                                                                    )
                                                                }
                                                                sx={{
                                                                    borderBottom:
                                                                        'none',
                                                                }}>
                                                                {isExpand[
                                                                    index
                                                                ] ? (
                                                                    <IconButton>
                                                                        <i className='fa-regular fa-chevron-up'></i>
                                                                    </IconButton>
                                                                ) : (
                                                                    <IconButton>
                                                                        <i className='fa-regular fa-chevron-down'></i>
                                                                    </IconButton>
                                                                )}
                                                            </TableCell>
                                                            <Tablecell
                                                                label={
                                                                    item.id
                                                                }
                                                                value={
                                                                    item.id
                                                                }
                                                            />
                                                            <Tablecell
                                                                label={
                                                                    item.quoteName
                                                                }
                                                                value={
                                                                    item.quoteName
                                                                }
                                                            />
                                                            <Tablecell
                                                                label={
                                                                    item.customer
                                                                }
                                                                value={
                                                                    item.customer
                                                                }
                                                            />
                                                            <Tablecell
                                                                label={
                                                                    item.date
                                                                }
                                                                value={
                                                                    item.date
                                                                }
                                                            />
                                                            <Tablecell
                                                                label={
                                                                    item.description
                                                                }
                                                                value={
                                                                    item.description
                                                                }
                                                            />
                                                            <Tablecell
                                                                label={
                                                                    item.salesRep
                                                                }
                                                                value={
                                                                    item.salesRep
                                                                }
                                                            />
                                                            <Tablecell
                                                                label='menu'
                                                                style={{
                                                                    textAlign:
                                                                        'right',
                                                                }}
                                                                value={
                                                                    <div
                                                                        style={{
                                                                            textAlign:
                                                                                'right',
                                                                        }}>
                                                                        <IconButton
                                                                            sx={{
                                                                                width: 25,
                                                                                height: 25,
                                                                            }}
                                                                            onClick={
                                                                                handleActionClick
                                                                            }>
                                                                            <i className='fa-regular fa-ellipsis-vertical'></i>
                                                                        </IconButton>
                                                                        <Menu
                                                                            anchorEl={
                                                                                actionAnchor
                                                                            }
                                                                            id='quote-menu'
                                                                            open={
                                                                                actionOpen
                                                                            }
                                                                            onClose={
                                                                                handleActionClose
                                                                            }>
                                                                            <ListItem
                                                                                disablePadding>
                                                                                <ListItemButton
                                                                                    onClick={() => {
                                                                                        handleAddEditQuote(
                                                                                            true,
                                                                                            item
                                                                                        )
                                                                                    }}>
                                                                                    <ListItemText
                                                                                        primary={
                                                                                            <Typography align='left'>
                                                                                                <i
                                                                                                    className='fa-regular fa-pen-to-square secondary-icon'
                                                                                                    style={{
                                                                                                        marginRight: 5,
                                                                                                    }}></i>
                                                                                                Edit
                                                                                            </Typography>
                                                                                        }
                                                                                    />
                                                                                </ListItemButton>
                                                                            </ListItem>
                                                                            <ListItem
                                                                                disablePadding>
                                                                                <ListItemButton>
                                                                                    <ListItemText
                                                                                        primary={
                                                                                            <Typography align='left'>
                                                                                                <i
                                                                                                    className='fa-regular fa-circle-minus secondary-icon'
                                                                                                    style={{
                                                                                                        marginRight: 5,
                                                                                                    }}></i>
                                                                                                Inactive
                                                                                            </Typography>
                                                                                        }
                                                                                    />
                                                                                </ListItemButton>
                                                                            </ListItem>
                                                                            <ListItem
                                                                                disablePadding>
                                                                                <ListItemButton>
                                                                                    <ListItemText
                                                                                        primary={
                                                                                            <Typography align='left'>
                                                                                                <i
                                                                                                    className='fa-regular fa-trash secondary-icon'
                                                                                                    style={{
                                                                                                        marginRight: 5,
                                                                                                    }}></i>
                                                                                                Delete
                                                                                            </Typography>
                                                                                        }
                                                                                    />
                                                                                </ListItemButton>
                                                                            </ListItem>
                                                                        </Menu>
                                                                    </div>
                                                                }
                                                            />
                                                        </TableRow>
                                                        <TableRow>
                                                            <TableCell
                                                                style={{
                                                                    paddingBottom: 0,
                                                                    paddingTop: 0,
                                                                }}
                                                                colSpan={
                                                                    8
                                                                }>
                                                                <Collapse
                                                                    in={
                                                                        isExpand[
                                                                            index
                                                                        ]
                                                                    }
                                                                    timeout='auto'
                                                                    unmountOnExit>
                                                                    <Box
                                                                        sx={{
                                                                            margin: 1,
                                                                        }}>
                                                                        <Stack
                                                                            direction='row'
                                                                            spacing={
                                                                                3
                                                                            }>
                                                                            <TableContainer>
                                                                                <Table sx={{width:'20%'}}>
                                                                                    <TableBody>
                                                                                    <TableRow>
                                                                                        <Tablecell
                                                                                            label='Contact Name'
                                                                                            value='Contact Name:'
                                                                                            style={{
                                                                                                fontWeight:
                                                                                                    'bold',
                                                                                            }}
                                                                                        />
                                                                                        <Tablecell
                                                                                            label={
                                                                                                item
                                                                                                    .contact
                                                                                                    .name
                                                                                            }
                                                                                            value={
                                                                                                item
                                                                                                    .contact
                                                                                                    .name
                                                                                            }
                                                                                        />
                                                                                    </TableRow>
                                                                                    <TableRow>
                                                                                        <Tablecell
                                                                                            label='PO Number'
                                                                                            value='PO Number:  '
                                                                                            style={{
                                                                                                fontWeight:
                                                                                                    'bold',
                                                                                            }}
                                                                                        />
                                                                                        <Tablecell
                                                                                            label={
                                                                                                item.po
                                                                                            }
                                                                                            value={
                                                                                                item.po
                                                                                            }
                                                                                        />
                                                                                    </TableRow>
                                                                                    </TableBody>
                                                                                </Table>
                                                                            </TableContainer>
                                                                        </Stack>
                                                                    </Box>
                                                                </Collapse>
                                                            </TableCell>
                                                        </TableRow>
                                                    </React.Fragment>
                                                )
                                            )}
                                        </TableBody>
                                    </Table>
                                    <TablePagination
                                        rowsPerPageOptions={[
                                            10, 25, 100,
                                        ]}
                                        component='div'
                                        count={
                                            QuoteItems.length
                                        }
                                        rowsPerPage={
                                            rowsPerPage
                                        }
                                        page={page}
                                        onPageChange={() => {}}
                                        onRowsPerPageChange={() => {}}
                                    />
                                </TableContainer>
                            </Paper>
                        </Stack>
                    </Box>
                </Paper>
            </div>
        </HelmetProvider>
    );
}

export default Quotes;
