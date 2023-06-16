import { styled } from '@mui/material/styles';
import { Box, Modal, Backdrop, Fade } from '@material-ui/core';

const StyledModal = styled(Modal)(({ theme }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
}));
  
const StyledBox = styled(Box)(({ theme }) => ({
    backgroundColor: theme.palette.background.paper,
    boxShadow: theme.shadows[5],
    padding: theme.spacing(2, 4, 3),
    width: 400
}));

export const CustomModal = ({
    open,
    handleClose, 
    headerContent,
    bodyContent
}) => {

    return (
        <StyledModal
            open={open}
            onClose={handleClose}
            closeAfterTransition
            BackdropComponent={Backdrop}
            BackdropProps={{
                timeout: 500,
            }} 
            disableEnforceFocus
        >
            <Fade in={open}>
                <StyledBox>
                    {headerContent}
                    {bodyContent}
                </StyledBox>
            </Fade>
        </StyledModal>
    );
};