import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Box, Paper, Typography } from '@mui/material';
import { Helmet, HelmetProvider } from 'react-helmet-async';
import NoContent from '../../components/NoContent';
import { getScheduledTruckCountPartialView } from '../../store/actions';
import { isEmpty } from 'lodash';

const Dashboard = (props) => {
<<<<<<< HEAD
    const pageName = 'Dashboard'
=======
    const pageName = 'Dashboard';
>>>>>>> main
    
    useEffect(() => {
        props.handleCurrentPageName(pageName);
    }, [props]);


    const [partialViewHtml, setPartialViewHtml] = useState('');
    
    const dispatch = useDispatch();
    const { htmlView } = useSelector(state => ({
        htmlView: state.DashboardReducer.htmlView
    }));
    
    useEffect(() => {
        if (isEmpty(htmlView)) {
            dispatch(getScheduledTruckCountPartialView());
        } else {
            const { result } = htmlView;
            setPartialViewHtml(result);
            //console.log('result: ', result)
        }
    }, [dispatch, htmlView]);

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
                    <div dangerouslySetInnerHTML={{ __html: partialViewHtml }} />
                    {/* <NoContent /> */}
                </Paper>
            </div>
        </HelmetProvider>
    );
};

export default Dashboard;