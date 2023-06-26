import React, { useState, useEffect } from 'react';
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
import { isEmpty } from 'lodash';
import { baseUrl } from '../../helpers/api_helper';
import {
     uploadSignaturePictureFile as onUploadSignaturePictureFile 
} from '../../store/actions';
import ImageUploader from '../common/images/imageUploader';
import ImageUploaderWithCrop from '../common/images/imageUploaderWithCrop';

const UploadSignaturePictureForm = ({
    closeModal
}) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [src, setSrc] = useState(null);

    const dispatch = useDispatch();
    const {
        signatureUploadResponse
    } = useSelector(state => ({
        signatureUploadResponse: state.UserProfileReducer.signatureUploadResponse
    }));

    useEffect(() => {
        if (!isEmpty(signatureUploadResponse)) {
            const { result } = signatureUploadResponse;
            if (!isEmpty(result)) {
                const { fileName, fileToken } = result;
                const filePath = `${baseUrl}/Temp/Downloads/${fileName}?v=${new Date().valueOf()}`;
                setSrc(filePath);
            }
        }
    }, [signatureUploadResponse]);

    const handleFileChange = async (event) => {
        const selectedFile = event.target.files[0];

        if (!selectedFile) {
            return;
        }
        
        var fileType = '|' + selectedFile.type.slice(selectedFile.type.lastIndexOf('/') + 1) + '|';
        if ('|jpg|jpeg|png|gif|'.indexOf(fileType) === -1) {
            alert('You can only upload JPG/JPEG/PNG/GIF file!');
            return false;
        }

        const maxSize = 1048576; // 1MB
        if (selectedFile.size > maxSize) {
            alert('You can only upload a file with a maximum size of 1MB!');
            return;
        }
    
        const formData = new FormData();
        formData.append('SignaturePicture', selectedFile);
        dispatch(onUploadSignaturePictureFile(formData));
    };

    const handleImageLoad = (e) => {
        const imageElement = e.target;
        const fileReader = new FileReader();

        fileReader.onload = (e) => {
            imageElement.src = e.target.result;
        };

        //fileReader.readAsDataURL(imageElement.src);
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

            <Box sx={{ p: 2 }}>
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

            <Box 
                display='flex'
                alignItems='center' 
                justifyContent='center'
                sx={{ 
                    p: 2 
                }}
            >
                {src && (
                    <img src={src} alt='Signature' onLoad={handleImageLoad} style={{ width: '100%' }} />
                )}
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