import { Box, Typography } from '@mui/material'
import { grey } from '@mui/material/colors'

const NoContent = () => {
    return (
        <Box
            sx={{
                textAlign: 'center',
                height: '80vh',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
            }}
        >
            <Typography variant='h6' color={grey[400]}>
                This page has no content yet.
            </Typography>
        </Box>
    )
}

export default NoContent