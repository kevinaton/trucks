import React, { useEffect, useState, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
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
import { uploadProfilePictureFile as onUploadProfilePictureFile } from '../../store/actions';

const generateGUID = () => {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0,
            v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
};

const ChangeProfilePictureForm = ({
    closeModal
}) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const fileInputRef = useRef(null);
    const [uploadedFileToken, setUploadedFileToken] = useState(null);

    const dispatch = useDispatch();
    const {
        uploadResponse
    } = useSelector(state => ({
        uploadResponse: state.UserProfileReducer.uploadResponse
    }));

    useEffect(() => {
        console.log('uploadResponse: ', uploadResponse);
    }, [uploadResponse]);

    const handleFileChange = async (event) => {
        const selectedFile = event.target.files[0];
    
        if (!selectedFile) {
            return;
        }
    
        const fileType = '|' + selectedFile.type.slice(selectedFile.type.lastIndexOf('/') + 1) + '|';
        if ('|jpg|jpeg|png|gif|'.indexOf(fileType) === -1) {
            alert('Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.');
            return;
        }
    
        const maxSize = 5242880; // 5MB
        if (selectedFile.size > maxSize) {
            alert('File size exceeds the maximum limit of 5MB.');
            return;
        }
    
        const formData = new FormData();
        formData.append('ProfilePicture', selectedFile);
        formData.append('FileType', 'file');
        formData.append('FileName', 'ProfilePicture');
        formData.append('FileToken', generateGUID());
        dispatch(onUploadProfilePictureFile(formData));
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
                    Change profile picture
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box sx={{ p: 2 }}>
                <form id="ChangeProfilePictureModalForm">
                    <div>
                        <Input 
                            ref={fileInputRef}
                            type="file" 
                            name="ProfilePicture" 
                            onChange={handleFileChange}
                            sx={{
                                '&::before, &::after': {
                                    borderBottom: 'none'
                                },
                                '&:hover::before, &:hover::after': {
                                    borderBottom: 'none'
                                }
                            }}
                        />
                        <label htmlFor="upload-profile-picture">
                            <IconButton
                                color="primary"
                                aria-label="upload picture"
                                component="span"
                            >
                                <PhotoCamera />
                            </IconButton>
                        </label>
                    </div>
                    <Typography variant='caption'>You can select a JPG/JPEG/PNG/GIF file with a maximum 5MB size.</Typography>
                </form>
            </Box>

            <Box sx={{ p: 2 }}>
                <Stack 
                    spacing={1}
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

export default ChangeProfilePictureForm;