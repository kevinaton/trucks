import * as React from 'react';
import {
    Box,
    Divider,
    List,
    ListItem,
    ListItemText,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Tooltip,
    Typography,
} from '@mui/material';
import { grey } from '@mui/material/colors';
import theme from '../../Theme';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import data from '../../common/data/data.json';
import { DataGrid } from '@mui/x-data-grid';
import MapView from '../../components/mapView';

const {
    ScheduleData,
    Jobsummarymaincolumns,
    Jobsummary,
    Jobsummarydetails,
    Detailcolumns,
    JobTotal,
} = data;

function JobSummary() {
    const pageName = 'Job Summary';
    const [markers, setMarkers] = React.useState([
        { lat: 40.712776, lng: -74.005974 },
        { lat: 40.7173101836559, lng: -73.99617867014256 },
        { lat: 40.72019992581124, lng: -74.00842135894511 },
        { lat: 40.70386725721366, lng: -74.01038018915354 },
    ]);
    const [center, setCenter] = React.useState({
        lat: 40.712776,
        lng: -74.005974,
    });

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
                <Box>
                    <Typography variant='h6' component='h2' sx={{ mb: 1 }}>
                        {pageName}
                    </Typography>
                </Box>
                <Paper>
                    <Stack spacing={2} sx={{ p: 3 }}>
                        <Box>
                            <Stack direction='row'>
                                <Stack direction='row' sx={{ width: '50%' }}>
                                    <Stack
                                        direction='column'
                                        sx={{
                                            background: grey[100],
                                            border: `1px solid ${theme.palette.grey[200]}`,
                                            width: '20%',
                                        }}>
                                        <List>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Customer</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Load at</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Item</Typography>
                                                </ListItemText>
                                            </ListItem>
                                        </List>
                                    </Stack>
                                    <Stack
                                        direction='column'
                                        sx={{
                                            border: `1px solid ${theme.palette.grey[200]}`,
                                            width: '80%',
                                        }}>
                                        <List>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography
                                                        sx={{
                                                            fontWeight: 'bold',
                                                            color: theme.palette.primary.main,
                                                        }}>
                                                        {ScheduleData[0].customer}
                                                    </Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>{ScheduleData[0].load}</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>{ScheduleData[0].item}</Typography>
                                                </ListItemText>
                                            </ListItem>
                                        </List>
                                    </Stack>
                                </Stack>
                                <Stack direction='row' sx={{ width: '50%' }}>
                                    <Stack
                                        direction='column'
                                        sx={{
                                            background: grey[100],
                                            border: `1px solid ${theme.palette.grey[200]}`,
                                            width: '20%',
                                        }}>
                                        <List>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Job Number</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Deliver to</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>Qty Ordered</Typography>
                                                </ListItemText>
                                            </ListItem>
                                        </List>
                                    </Stack>
                                    <Stack
                                        direction='column'
                                        sx={{
                                            border: `1px solid ${theme.palette.grey[200]}`,
                                            width: '80%',
                                        }}>
                                        <List>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>{ScheduleData[0].job}</Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>
                                                        {ScheduleData[0].deliver}
                                                    </Typography>
                                                </ListItemText>
                                            </ListItem>
                                            <ListItem>
                                                <ListItemText>
                                                    <Typography>
                                                        {ScheduleData[0].quantity}
                                                    </Typography>
                                                </ListItemText>
                                            </ListItem>
                                        </List>
                                    </Stack>
                                </Stack>
                            </Stack>
                        </Box>

                        <TableContainer>
                            <Table
                                aria-label='main-table'
                                sx={{
                                    borderTop: `1px solid ${grey[300]}`,
                                    borderLeft: `1px solid ${grey[300]}`,
                                    borderRight: `1px solid ${grey[300]}`,
                                }}>
                                <TableHead>
                                    <TableRow>
                                        <TableCell colSpan={3} />
                                        <TableCell
                                            sx={{ borderLeft: `1px solid ${grey[300]}` }}
                                            align='center'
                                            colSpan={9}>
                                            CYCLECHART
                                        </TableCell>
                                    </TableRow>
                                    <TableRow sx={{ bgcolor: grey[100] }}>
                                        {Jobsummarymaincolumns.map((column, index) => (
                                            <TableCell
                                                key={index}
                                                align={
                                                    column.headerName === 'Truck'
                                                        ? 'left'
                                                        : column.headerName === 'Loads'
                                                        ? 'left'
                                                        : column.headerName === 'Tons'
                                                        ? 'left'
                                                        : 'center'
                                                }
                                                sx={{
                                                    flex: column.flex,
                                                    borderRight:
                                                        column.headerName === 'Tons'
                                                            ? `1px solid ${grey[300]}`
                                                            : null,
                                                }}>
                                                {column.headerName}
                                            </TableCell>
                                        ))}
                                        <TableRow>
                                            <TableCell>
                                                <Typography sx={{ fontWeight: 'bold' }}>
                                                    Truck Loaded
                                                </Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Typography sx={{ fontWeight: 'bold' }}>
                                                    {JobTotal.loads}
                                                </Typography>
                                            </TableCell>
                                            <TableCell
                                                sx={{ borderRight: `1px solid ${grey[300]}` }}>
                                                <Typography
                                                    sx={{
                                                        fontWeight: 'bold',
                                                    }}>
                                                    {JobTotal.tons}
                                                </Typography>
                                            </TableCell>
                                            <TableCell colSpan={9} />
                                        </TableRow>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {Jobsummary.map((row, index) => (
                                        <TableRow key={index}>
                                            <TableCell>{row.truck}</TableCell>
                                            <TableCell>{row.load}</TableCell>
                                            <TableCell
                                                sx={{ borderRight: `1px solid ${grey[300]}` }}>
                                                {row.tons}
                                            </TableCell>

                                            {Object.keys(row).map((key, index) => {
                                                if (
                                                    key.startsWith('am6') ||
                                                    key.startsWith('am8') ||
                                                    key.startsWith('am10') ||
                                                    key.startsWith('nn12') ||
                                                    key.startsWith('pm2') ||
                                                    key.startsWith('pm4') ||
                                                    key.startsWith('pm6') ||
                                                    key.startsWith('pm8') ||
                                                    key.startsWith('pm10')
                                                ) {
                                                    return (
                                                        <Tooltip
                                                            title={row[key]}
                                                            key={index}
                                                            enterNextDelay={2000}>
                                                            <TableCell
                                                                label={row[key]}
                                                                key={index}
                                                                style={{
                                                                    background:
                                                                        row[key] === 'deliver'
                                                                            ? theme.palette.success
                                                                                  .main
                                                                            : row[key] === 'load'
                                                                            ? theme.palette.info
                                                                                  .main
                                                                            : grey[100],
                                                                }}
                                                            />
                                                        </Tooltip>
                                                    );
                                                }
                                                return null;
                                            })}
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                        <Divider />
                        <Box
                            sx={{
                                height: '480px',
                                width: '100%',
                                margin: 'auto',
                                display: 'flex',
                                position: 'relative',
                                overflow: 'hidden',
                            }}>
                            <MapView markers={markers} center={center} />
                        </Box>
                        <Box>
                            <Typography variant='h6' component='h3' sx={{ mb: 1 }}>
                                Details
                            </Typography>
                            <DataGrid columns={Detailcolumns} rows={Jobsummarydetails} />
                        </Box>
                    </Stack>
                </Paper>
            </div>
        </HelmetProvider>
    );
}

export default JobSummary;
