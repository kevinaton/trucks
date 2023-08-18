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
import { getUserProfilePicturePath } from '../../helpers';
import { baseUrl } from '../../helpers/api_helper';
import ReactCrop, { centerCrop, makeAspectCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';
import { 
    updateProfilePicture as onUpdateProfilePicture,
    uploadProfilePictureFile as onUploadProfilePictureFile,
    resetUpdateProfilePicture as onResetUpdateProfilePicture
} from '../../store/actions';
import { getText } from '../../helpers/localization_helper';
import { AlertDialog } from '../common/dialogs';

const centerAspectCrop = (mediaWidth, mediaHeight, aspect) => {
    return centerCrop(
        makeAspectCrop(
            {
                unit: '%',
                width: 90
            },
            aspect,
            mediaWidth,
            mediaHeight
        ),
        mediaWidth,
        mediaHeight
    );
};

const ChangeProfilePictureForm = ({
    closeModal,
    openDialog
}) => {
    const containerRef = useRef(null);
    const fileInputRef = useRef(null);
    const imgRef = useRef(null);
    const [selectedFile, setSelectedFile] = useState(null);
    const [imgSrc, setImgSrc] = useState('');
    const [imgWidth, setImgWidth] = useState(0);
    const [imgHeight, setImgHeight] = useState(0);
    const [crop, setCrop] = useState();
    const [completedCrop, setCompletedCrop] = useState();
    const [uploadedFileToken, setUploadedFileToken] = useState(null);

    const dispatch = useDispatch();
    const {
        uploadResponse,
        profilePictureUpdateSuccess
    } = useSelector(state => ({
        uploadResponse: state.UserProfileReducer.uploadResponse,
        profilePictureUpdateSuccess: state.UserProfileReducer.profilePictureUpdateSuccess
    }));

    useEffect(() => {
        if (!isEmpty(uploadResponse)) {
            const { result } = uploadResponse;
            if (!isEmpty(result)) {
                const { fileName, fileToken, fileType, height, width } = result;
                const filePath = `${baseUrl}/File/DownloadTempFile?fileToken=${fileToken}&fileName=${fileName}&fileType=${fileType}&v=${new Date().valueOf()}`;
                setUploadedFileToken(fileToken);
                setImgSrc(filePath);
                setImgWidth(width);
                setImgHeight(height);
            }
        }
    }, [uploadResponse]);

    useEffect(() => {
        if (profilePictureUpdateSuccess) {
            dispatch(onResetUpdateProfilePicture());
            document.querySelector('.header-profile-picture img').setAttribute('src', getUserProfilePicturePath());
            closeModal();
        }
    }, [dispatch, profilePictureUpdateSuccess, closeModal]);

    const handleFileChange = async (event) => {
        const file = event.target.files[0];
        if (!file) {
            return;
        }

        setSelectedFile(file);
    
        const fileType = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
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
        }
    
        const maxSize = 5242880; // 5MB
        if (file.size > maxSize) {
            openDialog({
                type: 'alert',
                content: (
                    <AlertDialog 
                        variant='warning'
                        message={getText('ProfilePicture_Warn_SizeLimit', 5)}
                    />
                )
            });
            return;
        }
    
        const mimeType = await getFileMimeType(file);
        const formData = new FormData();
        formData.append('ProfilePicture', file);
        formData.append('FileType', mimeType);
        formData.append('FileName', 'ProfilePicture');
        formData.append('FileToken', newId());
        dispatch(onUploadProfilePictureFile(formData));

        setCrop(undefined);
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
    
    const onImageLoad = (e) => {
        const { width, height } = e.currentTarget;
        setCrop(centerAspectCrop(width, height, 1));
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
        
        if (!uploadedFileToken) {
            return;
        }

        const containerWidth = containerRef.current.clientWidth;
        const containerHeight = containerRef.current.clientHeight;

        const widthRatio = imgWidth / containerWidth;
        const heightRatio = imgHeight / containerHeight;

        const cropX = completedCrop.x * widthRatio;
        const cropY = completedCrop.y * heightRatio;
        const cropWidth = completedCrop.width * widthRatio;
        const cropHeight = completedCrop.height * heightRatio;

        dispatch(onUpdateProfilePicture({
            fileToken: uploadedFileToken,
            x: parseInt(cropX),
            y: parseInt(cropY),
            width: parseInt(cropWidth),
            height: parseInt(cropHeight),
        }));
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
                    Change Profile Picture
                </Typography>
                <Button 
                    onClick={closeModal} 
                    sx={{ minWidth: '32px' }}
                >
                    <CloseIcon />
                </Button>
            </Box>

            <Box sx={{ p: 2 }} className='ImageCropper'>
                <div className='Crop-Controls'>
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

                {!!imgSrc && (
                    <Box sx={{ mt: 1 }} ref={containerRef}>
                        <ReactCrop
                            crop={crop} 
                            onChange={(_, percentCrop) => setCrop(percentCrop)} 
                            onComplete={(c) => setCompletedCrop(c)} 
                            aspect={1}
                        >
                            <img 
                                ref={imgRef}
                                alt='Crop me'
                                src={imgSrc} 
                                style={{ 
                                    transform: `scale(1) rotate(0deg)` 
                                }} 
                                onLoad={onImageLoad}
                            />
                        </ReactCrop>
                    </Box>
                )}
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