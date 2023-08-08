import * as React from 'react';
import { Box, LinearProgress } from '@mui/material';

export const ProgressBar = () => {
    return (
        <Box sx={{ width: '100%' }}>
            <LinearProgress sx={{ zIndex: 9999 }} />
        </Box>
    );
};