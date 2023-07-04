import * as React from 'react';
import {
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    IconButton,
    Modal,
    TextField,
} from '@mui/material';

const EditNote = ({ isOpen, setIsOpen }) => {
    const [note, setNote] = React.useState('');
    const handleClose = () => {
        setIsOpen(false);
    };
    const handleSave = () => {
        setIsOpen(false);
    };

    return (
        <Modal open={isOpen} onClose={handleClose} aria-labelledby='edit-note'>
            <Card
                sx={{
                    minWidth: 500,
                    position: 'absolute',
                    top: '30%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='Edit Note'
                />
                <CardContent>
                    <TextField
                        id='internalNotes'
                        value={note}
                        onChange={(event) => {
                            setNote(event.target.value);
                        }}
                        multiline
                        rows={4}
                        label='Internal Notes'
                        sx={{
                            width: '100%',
                        }}
                    />
                </CardContent>
                <CardActions sx={{ justifyContent: 'end' }}>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button
                        variant='contained'
                        onClick={handleSave}
                        startIcon={<i className='fa-regular fa-save'></i>}>
                        Save
                    </Button>
                </CardActions>
            </Card>
        </Modal>
    );
};

export default EditNote;
