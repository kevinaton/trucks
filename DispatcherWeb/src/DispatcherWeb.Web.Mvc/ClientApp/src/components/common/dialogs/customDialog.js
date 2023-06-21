import { 
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogContentText,
    DialogActions
} from '@mui/material';

export const CustomDialog = ({
    open,
    handleClose,
    handleProceed,
    dialogTitle, 
    dialogDescription,
    contentTitle,
    content
}) => {
    const handleProceedClick = () => {
        // Perform custom actions based on the form data
        handleProceed();
    };

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            aria-labelledby={dialogTitle} 
            aria-describedby={dialogDescription} 
            maxWidth='xs'
            fullWidth
        >
            <DialogTitle id={dialogTitle}>{contentTitle}</DialogTitle>
            <DialogContent>
                <DialogContentText id={dialogDescription}>
                    {content}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose}>Cancel</Button>
                <Button onClick={handleProceedClick} autoFocus>
                    Continue
                </Button>
            </DialogActions>
        </Dialog>
    );
};