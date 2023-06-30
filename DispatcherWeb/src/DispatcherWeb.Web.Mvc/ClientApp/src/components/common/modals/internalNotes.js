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

const InternalNotes = ({ isInternalNotes, setIsInternalNotes }) => {
    const [internalNotes, setInternalNotes] = React.useState('');

    const handleSave = () => {
        setIsInternalNotes(false);
    };

    const handleClose = () => {
        setIsInternalNotes(false);
    };

    return (
        <Modal open={isInternalNotes} onClose={handleClose} aria-labelledby='internal-notes'>
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
                    title='Internal Notes'
                />
                <CardContent>
                    <TextField
                        id='internalNotes'
                        value={internalNotes}
                        onChange={(event) => {
                            setInternalNotes(event.target.value);
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

export default InternalNotes;
