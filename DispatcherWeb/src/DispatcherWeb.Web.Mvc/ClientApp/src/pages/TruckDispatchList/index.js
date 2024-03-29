import React, { useEffect } from 'react';
import { Box, Typography } from '@mui/material';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import Timeline from '../../components/Timeline';

const TruckDispatchList = (props) => {
    const pageName = 'Truck Dispatch List';

    useEffect(() => {
        props.handleCurrentPageName(pageName);
    }, [props]);

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
                        content='/reactapp/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>
                <Box>
                    <Typography variant='h6' component='h2' sx={{ mb: 1 }}>
                        {pageName}
                    </Typography>
                </Box>
                <Timeline />
            </div>
        </HelmetProvider>
    );
};

export default TruckDispatchList;
