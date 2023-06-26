import * as React from 'react';
import { Box, Button, Divider, Paper, Stack, Typography } from '@mui/material';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import NoContent from '../../components/NoContent';
import data from '../../common/data/data.json';
import AddQuote from '../../components/common/modals/addQuote';
import AddQuoteItem from '../../components/common/modals/addQuoteItem';
import moment from 'moment'

const { ScheduleData, Quote } = data
const defaultQuoteValues = {
    id: 0,
    name: '',
    description: '',
    proposalDate: moment(),
    proposalExpiryDate: moment().add(1, 'months'),
    inactivationDate: undefined,
    customer: null,
    contact: null,
    status: 'Pending',
    salesPerson: '',
    poNumber: 0,
    comments: '',
    insertCannedText: '',
    notes: '',
}

function Quotes() {
    const pageName = 'Quotes'
    const [isQuote, setIsQuote] = React.useState(false)
    const [isAddItem, setIsAddItem] = React.useState(false)
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

    const handleAddEditQuote = (state, edit) => {
        if (edit.name === '') {
            setQuote(edit)
            setIsQuote(state)
        } else {
            if (!edit.proposalDate) {
                setQuote({
                    ...edit,
                    proposalDate: moment(),
                    proposalExpiryDate: moment().add(1, 'months'),
                })
                setIsQuote(state)
            } else {
                setQuote(edit)
                setIsQuote(state)
            }
        }
    }

    React.useEffect(() => {
        setQuoteItems(ScheduleData)
    }, [])

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

                <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between' }}>
                    <Stack direction='row' spacing={2} sx={{ alignItems: 'center' }}>
                        <Typography variant='h6' component='h2'>
                            {pageName}
                        </Typography>
                        <Divider orientation='vertical' flexItem />
                        <Typography>Manage your quotes</Typography>
                    </Stack>
                    <Button
                        variant='contained'
                        size='large'
                        onClick={() => handleAddEditQuote(true, defaultQuoteValues)}
                        startIcon={
                            <i className='fa-regular fa-plus' style={{ fontSize: '0.8rem' }}></i>
                        }>
                        Add New
                    </Button>
                </Box>
                <Paper>
                    <Button onClick={() => handleAddEditQuote(true, Quote)}>
                        Edit quote -temporary
                    </Button>
                    <NoContent />
                </Paper>
            </div>
        </HelmetProvider>
    )
}

export default Quotes
