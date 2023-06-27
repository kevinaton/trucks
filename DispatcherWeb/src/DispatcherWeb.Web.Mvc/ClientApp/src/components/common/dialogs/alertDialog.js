import {
    Box,
    Typography
} from '@mui/material';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { theme } from '../../../Theme';

export const AlertDialog = ({
    variant,
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
                { variant && variant === 'success' 
                    ?
                        <CheckCircleOutlineIcon 
                            sx={{ 
                                color: theme.palette.success.main,
                                fontSize: '88px !important'
                            }} 
                        /> 
                    : 
                        <HighlightOffIcon 
                            sx={{ 
                                color: theme.palette.error.main,
                                fontSize: '88px !important'
                            }} 
                        />
                }
            </Box>

            { title && 
                <Typography variant='h4' sx={{ mb: 1 }}>{title}</Typography>
            }
            <Typography variant='h6'>{message}</Typography>
        </Box>
    );
};