import { styled } from '@mui/material/styles';
import { Box } from '@mui/material';
import { Modal, Backdrop, Fade } from '@material-ui/core';

const StyledModal = styled(Modal)(({ theme }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
}));
  
const StyledBox = styled(Box)(({ theme }) => ({
    backgroundColor: theme.palette.background.paper,
    boxShadow: theme.shadows[5],
    padding: 0,
    width: 400
}));

export const CustomModal = ({
    open,
    handleClose, 
    content
}) => {

    return (
        <StyledModal
            open={open}
            onClose={handleClose}
            closeAfterTransition
            BackdropComponent={Backdrop}
            BackdropProps={{
                timeout: 500,
                onClick: (event) => event.stopPropagation(), // Prevent backdrop click from closing the modal
            }} 
            disableEnforceFocus
        >
            <Fade in={open}>
                <StyledBox>
                    {content}
                </StyledBox>
            </Fade>
        </StyledModal>
    );
};