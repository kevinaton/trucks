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
import { newId } from '../../utils';
import { isEmpty } from 'lodash';
import { baseUrl } from '../../helpers/api_helper';
import ImageCropper from '../common/images/imageCropper';
import { uploadProfilePictureFile as onUploadProfilePictureFile } from '../../store/actions';

const ChangeProfilePictureForm = ({
    closeModal
}) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const fileInputRef = useRef(null);
    const [uploadedFileToken, setUploadedFileToken] = useState(null);
    const [imagePath, setImagePath] = useState('');
    const [src, setSrc] = useState('');

    const dispatch = useDispatch();
    const {
        uploadResponse
    } = useSelector(state => ({
        uploadResponse: state.UserProfileReducer.uploadResponse
    }));

    useEffect(() => {
        console.log('uploadResponse: ', uploadResponse);
        if (!isEmpty(uploadResponse)) {
            const { result } = uploadResponse;
            if (!isEmpty(result)) {
                const { fileName, fileToken, fileType, height, width } = result;
                const filePath = `${baseUrl}/File/DownloadTempFile?fileToken=${fileToken}&fileName=${fileName}&fileType=${fileType}&v=${new Date().valueOf()}`;
                setSrc(filePath)
                downloadImage(filePath);

                setUploadedFileToken(fileToken);
                //setImagePath(filePath);
            }
        }
    }, [uploadResponse]);

    const downloadImage = async (filePath) => {
        try {
            const response = await fetch(filePath);
            if (response.ok) {
                console.log('response: ', response)
                const blob = await response.blob();
                console.log('blob: ', blob)
                const imgUrl = URL.createObjectURL(blob);
                setImagePath(imgUrl);
            } else {
                console.error('Error downloading image:', response.statusText);
            }
        } catch (error) {
            console.error('Error downloading image:', error);
        }
    };

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

        const mimeType = await getFileMimeType(selectedFile);
        formData.append('FileType', mimeType);
        formData.append('FileName', 'ProfilePicture');
        formData.append('FileToken', newId());
        dispatch(onUploadProfilePictureFile(formData));
    };

    const getFileMimeType = (file) => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                const arr = new Uint8Array(reader.result).subarray(0, 4);
                let header = '';
                
                for (let i = 0; i < arr.length; i++) {
                    header += arr[i].toString(16);
                }

                let mimeType;
                switch (header) {
                    case '89504e47':
                        mimeType = 'image/png';
                        break;
                    case '47494638':
                        mimeType = 'image/gif';
                        break;
                    case 'ffd8ffe0':
                    case 'ffd8ffe1':
                    case 'ffd8ffe2':
                    case 'ffd8ffe3':
                    case 'ffd8ffe8':
                        mimeType = 'image/jpeg';
                        break;
                    default:
                        mimeType = 'unknown';
                        break;
                }
                resolve(mimeType);
            };
            reader.onerror = reject;
            reader.readAsArrayBuffer(file);
        });
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
                {/* <form id="ChangeProfilePictureModalForm">
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
                </form> */}
                <ImageCropper />
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