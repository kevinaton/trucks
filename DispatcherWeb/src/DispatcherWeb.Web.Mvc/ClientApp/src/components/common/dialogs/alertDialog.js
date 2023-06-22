import {
    Box,
    Typography
} from '@mui/material';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { theme } from '../../../Theme';

export const AlertDialog = ({
    title,
    message
}) => {
    return (
        <Box
            display='flex'
            alignItems='center'
            flexDirection='column'
        >
            <Box 
                display='flex' 
                alignItems='center' 
                justifyContent='center'
                sx={{
                    marginBottom: '15px'
                }}
            >
                <HighlightOffIcon 
                    sx={{ 
                        color: theme.palette.error.main,
                        fontSize: '88px !important'
                    }} 
                />
            </Box>

            { title && 
                <Typography variant='h4' sx={{ mb: 1 }}>{title}</Typography>
            }
            <Typography variant='h6'>{message}</Typography>
        </Box>
    );
};