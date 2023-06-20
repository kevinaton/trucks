import React, { useState } from 'react';
import {
    Box,
    Button,
    IconButton,
    Input,
    Stack,
    Typography
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { PhotoCamera } from '@mui/icons-material';

const UploadSignaturePictureForm = ({
    closeModal
}) => {
    const [selectedFile, setSelectedFile] = useState(null);

    const handleFileChange = (event) => {
        setSelectedFile(event.target.files[0]);
    };

    const handleCancel = () => {
        // Reset the form
        closeModal();
    };

    const handleUpload = () => {
        // Implement your upload logic here
        if (selectedFile) {
            console.log('Uploading file:', selectedFile);
        // Perform the necessary actions such as sending the file to a server
        } else {
            console.log('No file selected');
        }
    };

    return (
        <React.Fragment>
            <Box
                display='flex'
                justifyContent='space-between'
                alignItems='center'
                sx={{ 
                    p: 2 
                }} 
            >
                <Typography variant='h6' component='h2'>
                    Upload signature picture
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box
                component='form'
                noValidate
                autoComplete="off"
                sx={{ p: 2 }}
            >
                <div style={{ marginBottom: '10px' }}>
                    <Input type="file" onChange={handleFileChange} />
                    <label htmlFor="upload-signature-picture">
                        <IconButton
                            color="primary"
                            aria-label="upload picture"
                            component="span"
                        >
                            <PhotoCamera />
                        </IconButton>
                    </label>
                </div>
                <Typography variant='caption'>You can select a JPG/JPEG/PNG/GIF file with a maximum 1MB size.</Typography>
            </Box>

            <Box sx={{ p: 2 }}>
                <Stack 
                    spacing={2}
                    direction='row' 
                    justifyContent='flex-end'
                >
                    <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
                    <Button variant='contained' color='primary' onClick={handleUpload}>
                        <i className='fa-regular fa-save' style={{ marginRight: '6px' }}></i>
                        <Typography>Save</Typography>
                    </Button>
                </Stack>
            </Box>
        </React.Fragment>
    );
};

export default UploadSignaturePictureForm;