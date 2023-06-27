import React, { useState } from 'react';
import Dropzone from 'react-dropzone';
import ReactCrop from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

const ImageUploaderWithCrop = () => {
    const [selectedImage, setSelectedImage] = useState(null);
    const [crop, setCrop] = useState({});
    const [croppedImage, setCroppedImage] = useState(null);
    const [imageObject, setImageObject] = useState(null);

    const handleImageUpload = (acceptedFiles) => {
        const file = acceptedFiles[0];
        const reader = new FileReader();
        reader.onload = () => {
            setSelectedImage(reader.result);
        };
        reader.readAsDataURL(file);
    };

    const handleCropComplete = (cropResult) => {
        // cropResult contains the cropped image coordinates
        console.log(cropResult);
        // You can perform further actions with the cropped image here

        // Generate a blob URL for the cropped image
        const croppedImageUrl = URL.createObjectURL(cropResult.croppedImage);
        setCroppedImage(croppedImageUrl);
    };

    const handleImageLoaded = (image) => {
        setImageObject(image);
        setCrop({ aspect: 1 / 1 });
    };

    return (
        <div>
            <Dropzone onDrop={handleImageUpload}>
                {({ getRootProps, getInputProps }) => (
                    <div {...getRootProps()} className='dropzone'>
                        <input {...getInputProps()} />
                        <p>Drag and drop a file here, or click to select a file</p>
                    </div>
                )}
            </Dropzone>
            {selectedImage && (
                <ReactCrop 
                    src={selectedImage}
                    onImageLoaded={handleImageLoaded} 
                    crop={crop}
                    onChange={(newCrop) => setCrop(newCrop)}
                    onComplete={handleCropComplete}
                />
            )}
            {croppedImage && (
                <div>
                    <h2>Cropped Image Preview</h2>
                    <img src={croppedImage} alt="Cropped" />
                </div>
            )}
        </div>
    );
};

export default ImageUploaderWithCrop;