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
    uploadSignaturePictureFile as onUploadSignaturePictureFile,
    updateSignaturePicture as onUpdateSignaturePicture,
    resetUpdateSignaturePicture as onResetUpdateSignaturePicture
} from '../../store/actions';
import { getText } from '../../helpers/localization_helper';
import { AlertDialog } from '../common/dialogs';

const UploadSignaturePictureForm = ({
    closeModal,
    openDialog
}) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [src, setSrc] = useState(null);
    const [uploadedFileName, setUploadedFileName] = useState(null);

    const dispatch = useDispatch();
    const {
        signatureUploadResponse,
        signatureUpdateSuccess
    } = useSelector(state => ({
        signatureUploadResponse: state.UserProfileReducer.signatureUploadResponse,
        signatureUpdateSuccess: state.UserProfileReducer.signatureUpdateSuccess
    }));

    useEffect(() => {
        if (!isEmpty(signatureUploadResponse)) {
            const { result } = signatureUploadResponse;
            if (!isEmpty(result)) {
                const { fileName } = result;
                const filePath = `${baseUrl}/Temp/Downloads/${fileName}?v=${new Date().valueOf()}`;
                setUploadedFileName(fileName);
                setSrc(filePath);
            }
        }
    }, [signatureUploadResponse]);

    useEffect(() => {
        if (signatureUpdateSuccess) {
            dispatch(onResetUpdateSignaturePicture());
            closeModal();
        }
    }, [dispatch, signatureUpdateSuccess, closeModal]);

    const handleFileChange = async (event) => {
        const file = event.target.files[0];
        if (!file) {
            return;
        }

        setSelectedFile(file);
        
        var fileType = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
        if ('|jpg|jpeg|png|gif|'.indexOf(fileType) === -1) {
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='warning'
                        message={getText('ProfilePicture_Warn_FileType')}
                    />
                )
            });
            return false;
        }

        const maxSize = 1048576; // 1MB
        if (file.size > maxSize) {
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='warning'
                        message={getText('ProfilePicture_Warn_SizeLimit', 1)}
                    />
                )
            });
            return;
        }
    
        const formData = new FormData();
        formData.append('SignaturePicture', file);
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
        if (!selectedFile) {
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='error' 
                        message={getText('File_Empty_Error')}
                    />
                )
            });
        }
        
        if (!uploadedFileName) {
            return;
        }
        
        dispatch(onUpdateSignaturePicture({ fileName: uploadedFileName }));
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