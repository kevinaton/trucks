import { Box, Button, Menu, MenuItem, Paper  } from '@mui/material'
import { styled } from '@mui/material/styles'
import { theme } from '../../../Theme'

export const NotificationWrapper = styled(Menu)(({ theme }) => ({
    '& .MuiPaper-root': {
        borderRadius: '8px',
        boxShadow: '0 0 6px 6px rgba(69, 65, 78, 0.08)',

        '& .MuiList-root': {
            padding: 0
        }
    },

    '& .MuiBackdrop-root': {
        backgroundColor: 'rgba(214, 234, 239, 0.61)', // Replace 'your-color-here' with your desired color
    },
}))

export const NotificationContent = styled(Paper)(({ theme }) => ({
    width: 380
}))

export const NotificationHeader = styled(Box)(({ theme }) => ({
    display: 'flex',
    justifyContent: 'space-between',
    backgroundColor: '#fff',
    padding: '15px 15px 8px',

    '& h6': { 
        color: '#000',
        fontSize: '17px',
        fontWeight: 600
    },

    '& button': {
        border: '1px solid #f2f2f4',
        color: '#9c9da1',
        minWidth: '32px',
        fontSize: '16px'
    }
}))

export const NotificationItem = styled(MenuItem)(({ theme }) => ({
    flexGrow: 1,
    whiteSpace: 'normal',
    padding: '4px 8px',

    '& a': {
        paddingLeft: '13px',
        paddingRight: '4px',
        fontSize: '13px'
    },

    '&:hover': {
        backgroundColor: 'transparent'
    }
}))

export const NotificationFooter = styled(Box)(({ theme }) => ({
    display: 'flex',
    justifyContent: 'flex-start',
    backgroundColor: '#fff',
    borderTop: '1px solid #f1f1f1',
    padding: '12px 15px',

    '& button:last-child': {
        marginLeft: 'auto'
    }
}))

export const MarkAllAsReadButton = styled(Button)(({ theme }) => ({
    color: theme.palette.primary.main,

    '& i': {
        marginRight: '5px'
    }
}))

export const ViewAllNotificationsButton = styled(Button)(({ theme }) => ({
    backgroundColor: theme.palette.primary.main,
    color: '#fff',
    padding: '3px 6px',
    fontWeight: 400,
    textTransform: 'inherit',
    letterSpacing: '1px'
}))