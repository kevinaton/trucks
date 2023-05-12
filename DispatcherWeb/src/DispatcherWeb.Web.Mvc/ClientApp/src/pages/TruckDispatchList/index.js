import { Box, Typography } from '@mui/material'
import { Helmet, HelmetProvider } from 'react-helmet-async'
import Timeline from '../../components/Timeline'

const TruckDispatchList = () => {
    return (
        <HelmetProvider>
            <div>
                <Helmet>
                    <meta charSet="utf-8" />
                    <title>Truck Dispatch List</title>
                    <meta name="description" content="Dumptruckdispatcher app" />
                    <meta content="" name="author" />
                    <meta property="og:title" content="Truck Dispatch List" />
                    <meta
                        property="og:image"
                        content="/reactapp/assets/dumptruckdispatcher-logo-mini.png"
                    />
                </Helmet>
                <Box>
                    <Typography variant="h6" component="h2" sx={{ mb: 1 }}>
                        Truck Dispatch List
                    </Typography>
                </Box>
                <Timeline />
            </div>
        </HelmetProvider>
    )
}

export default TruckDispatchList