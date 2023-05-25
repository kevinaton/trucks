import React, { useEffect } from 'react';
import { Box, Paper, Typography } from '@mui/material';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import NoContent from '../../components/NoContent';

const Drivers = (props) => {
    const pageName = 'Drivers';
    
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
                        content='%PUBLIC_URL%/assets/dumptruckdispatcher-logo-mini.png'
                    />
                </Helmet>
                <Box>
                    <Typography variant='h6' component='h2' sx={{ mb: 1 }}>
                        {pageName}
                    </Typography>
                </Box>
                <Paper>
                    <NoContent />
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default Drivers;