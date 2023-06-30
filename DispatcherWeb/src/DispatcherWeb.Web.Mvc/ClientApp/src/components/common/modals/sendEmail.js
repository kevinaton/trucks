import * as React from 'react';
import {
    Autocomplete,
    Button,
    Card,
    CardActions,
    CardContent,
    CardHeader,
    Chip,
    IconButton,
    Modal,
    Stack,
    TextField,
} from '@mui/material';

const SendEmail = ({ isOpen, setIsOpen }) => {
    const [email, setEmail] = React.useState({
        from: '',
        to: '',
        cc: [],
        subject: '',
        body: '',
    });

    const handleClose = () => {
        setIsOpen(false);
    };

    const handleSave = () => {
        setIsOpen(false);
    };

    const handleEmail = (field, value) => {
        setEmail((prevEmail) => ({
            ...prevEmail,
            [field]: value,
        }));
    };

    return (
        <Modal open={isOpen} onClose={handleClose} aria-labelledby='send-email'>
            <Card
                sx={{
                    minWidth: 500,
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}>
                <CardHeader
                    action={
                        <IconButton aria-label='close' onClick={handleClose}>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='Send Email'
                />
                <CardContent>
                    <Stack direction='column' spacing={2}>
                        <TextField
                            id='email-from'
                            type='email'
                            value={email.from}
                            onChange={(e) => {
                                handleEmail('from', e.target.value);
                            }}
                            label='From'
                            required
                            sx={{ width: '1' }}
                        />
                        <TextField
                            id='email-to'
                            type='email'
                            value={email.to}
                            onChange={(e) => {
                                handleEmail('to', e.target.value);
                            }}
                            label='To'
                            required
                            sx={{ width: '1' }}
                        />
                        <Autocomplete
                            id='cc'
                            multiple
                            options={[]}
                            freeSolo
                            value={email.cc}
                            onChange={(event, value) => {
                                handleEmail('cc', value);
                            }}
                            renderTags={(value, getTagProps) =>
                                value.map((option, index) => (
                                    <Chip
                                        variant='outlined'
                                        label={option}
                                        {...getTagProps({ index })}
                                    />
                                ))
                            }
                            renderInput={(params) => (
                                <TextField
                                    {...params}
                                    variant='outlined'
                                    label='CC'
                                    placeholder='Add email by enter'
                                />
                            )}
                        />
                        <TextField
                            id='subject'
                            value={email.subject}
                            onChange={(e) => {
                                handleEmail('subject', e.target.value);
                            }}
                            label='Subject'
                            required
                            sx={{ width: '1' }}
                        />
                        <TextField
                            id='body'
                            multiline
                            rows={3}
                            value={email.body}
                            onChange={(e) => {
                                handleEmail('body', e.target.value);
                            }}
                            label='Body'
                            required
                            sx={{ width: '1' }}
                        />
                    </Stack>
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

export default SendEmail;
