import React, { useState, useEffect } from 'react';
import ReactCrop from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';
import { isEmpty } from 'lodash';

export const ImageCropComponent = ({ imageUrl }) => {
    const [src, setSrc] = useState(null);
    const [image, setImage] = useState(null);
    const [crop, setCrop] = useState({ unit: '%', width: 30, aspect: 16 / 9 });
    const [croppedImageUrl, setCroppedImageUrl] = useState('');
    
    useEffect(() => {
        if (!isEmpty(imageUrl)) {
            setSrc(imageUrl);
        }
    }, [imageUrl]);

    const downloadImage = async (filePath) => {
        try {
            const response = await fetch(filePath);
            if (response.ok) {
                const blob = await response.blob();
                const imgUrl = URL.createObjectURL(blob);
                setSrc(imgUrl);
            } else {
                console.error('Error downloading image:', response.statusText);
            }
        } catch (error) {
            console.error('Error downloading image:', error);
        }
    };
    
    // useEffect(() => {
    //     // Cleanup the created object URLs when component unmounts
    //     return () => {
    //         if (croppedImageUrl) {
    //             URL.revokeObjectURL(croppedImageUrl);
    //         }
    //     };
    // }, [croppedImageUrl]);

    // const handleCropChange = (newCrop) => {
    //     console.log('newCrop: ', newCrop)
    //     setCrop(newCrop);
    // };

    // const handleCropComplete = (cropResult) => {
    //     console.log('cropResult: ', cropResult)
    //     if (cropResult.width && cropResult.height) {
    //         getCroppedImage(imageUrl, cropResult, (croppedImage) => {
    //             console.log('croppedImage: ', croppedImage)
    //             setCroppedImageUrl(URL.createObjectURL(croppedImage));
    //         });
    //     }
    // };

    // const getCroppedImage = (imageSrc, crop, callback) => {
    //     console.log('getCroppedImage: ', imageSrc)
    //     const image = new Image();
    //     image.src = imageSrc;
    
    //     image.onload = () => {
    //         const canvas = document.createElement('canvas');
    //         const scaleX = image.naturalWidth / image.width;
    //         const scaleY = image.naturalHeight / image.height;
    //         const ctx = canvas.getContext('2d');
        
    //         canvas.width = crop.width;
    //         canvas.height = crop.height;
        
    //         ctx.drawImage(
    //             image,
    //             crop.x * scaleX,
    //             crop.y * scaleY,
    //             crop.width * scaleX,
    //             crop.height * scaleY,
    //             0,
    //             0,
    //             crop.width,
    //             crop.height
    //         );
        
    //         canvas.toBlob((croppedImage) => {
    //             callback(croppedImage);
    //         }, 'image/jpeg');
    //     };
    // };

    const cropImageNow = () => {
        const canvas = document.createElement('canvas');
        const scaleX = image.naturalWidth / image.width;
        const scaleY = image.naturalHeight / image.height;
        canvas.width = crop.width;
        canvas.height = crop.height;
        const ctx = canvas.getContext('2d');
 
        const pixelRatio = window.devicePixelRatio;
        canvas.width = crop.width * pixelRatio;
        canvas.height = crop.height * pixelRatio;
        ctx.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
        ctx.imageSmoothingQuality = 'high';
 
        ctx.drawImage(
            image,
            crop.x * scaleX,
            crop.y * scaleY,
            crop.width * scaleX,
            crop.height * scaleY,
            0,
            0,
            crop.width,
            crop.height,
        );
 
        // Converting to base64
        const base64Image = canvas.toDataURL('image/jpeg');
        setCroppedImageUrl(base64Image);
    };

    if (!src) {
        return <div>Loading...</div>;
    }

    return (
        <div> 
            { src && (
                <div>
                    <ReactCrop
                        src={src} 
                        onImageLoaded={setImage}
                        crop={crop} 
                        onChange={setCrop}
                    />
                    <br />
                    <button onClick={cropImageNow}>Crop</button>
                    {croppedImageUrl && <img src={croppedImageUrl} alt="Cropped" />}
                </div>
            )}
        </div>
      );
};