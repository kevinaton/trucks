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
    type,
    handleClose,
    handleProceed,
    title,
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
            maxWidth='xs'
            fullWidth
        >
            { title && 
                <DialogTitle>{title}</DialogTitle>
            }

            <DialogContent>
                <DialogContentText>
                    {content}
                </DialogContentText>
            </DialogContent>
            
            <DialogActions>
                { type && type === 'confirm' 
                    ? 
                        <>
                            <Button onClick={handleClose}>Cancel</Button>
                            <Button onClick={handleProceedClick} autoFocus>
                                Continue
                            </Button>
                        </>
                    : <Button onClick={handleClose}>Ok</Button>
                }
            </DialogActions>
        </Dialog>
    );
};