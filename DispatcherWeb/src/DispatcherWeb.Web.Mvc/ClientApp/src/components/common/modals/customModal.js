import { styled } from '@mui/material/styles';
import { Modal, Backdrop, Fade } from '@material-ui/core';

const StyledModal = styled(Modal)(({ theme }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
  }));
  
  const StyledPaper = styled('div')(({ theme }) => ({
    backgroundColor: theme.palette.background.paper,
    boxShadow: theme.shadows[5],
    padding: theme.spacing(2, 4, 3),
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
            }}
        >
            <Fade in={open}>
                <StyledPaper>
                    {content}
                </StyledPaper>
            </Fade>
        </StyledModal>
    );
};