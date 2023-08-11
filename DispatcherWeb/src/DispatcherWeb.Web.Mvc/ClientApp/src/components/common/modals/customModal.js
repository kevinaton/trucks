import { styled } from '@mui/material/styles';
import { Box } from '@mui/material';
import { Modal, Backdrop, Fade } from '@material-ui/core';

const StyledModal = styled(Modal)(({ theme }) => ({
    display: 'flex',
    alignItems: 'flex-start',
    justifyContent: 'center',
    overflowY: 'auto',
    paddingTop: '30px'
}));
  
const StyledBox = styled(Box)(({ theme, size }) => ({
    backgroundColor: theme.palette.background.paper,
    boxShadow: theme.shadows[5],
    padding: 0,
    width: size,
    position: 'relative',
}));

export const CustomModal = ({
    open,
    handleClose, 
    content,
    size
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
                <StyledBox size={size}>
                    {content}
                </StyledBox>
            </Fade>
        </StyledModal>
    );
};