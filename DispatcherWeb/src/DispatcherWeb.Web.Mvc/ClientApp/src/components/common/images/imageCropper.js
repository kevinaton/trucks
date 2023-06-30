import React, { useState, useRef } from 'react';
import ReactCrop, { centerCrop, makeAspectCrop } from 'react-image-crop';
import { canvasPreview } from './canvasPreview';
import { useDebounceEffect } from './useDebounceEffect';
import 'react-image-crop/dist/ReactCrop.css';

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

const ImageCropper = () => {
    const [imgSrc, setImgSrc] = useState('');
    const previewCanvasRef = useRef(null);
    const imgRef = useRef(null);
    const hiddenAnchorRef = useRef(null);
    const blobUrlRef = useRef('');
    const [crop, setCrop] = useState();
    const [completedCrop, setCompletedCrop] = useState();
    const [scale, setScale] = useState(1);
    const [rotate, setRotate] = useState(0);
    const [aspect, setAspect] = useState(1);

    const onSelectFile = (e) => {
        if (e.target.files && e.target.files.length > 0) {
            setCrop(undefined);
            const reader = new FileReader();
            reader.addEventListener('load', () => 
                setImgSrc(reader.result ? reader.result.toString() : ''),
            );
            reader.readAsDataURL(e.target.files[0]);
        }
    };

    const onImageLoad = (e) => {
        if (aspect) {
            const { width, height } = e.currentTarget;
            setCrop(centerAspectCrop(width, height, aspect));
        }
    };

    // const onDownloadCropClick = () => {
    //     if (!previewCanvasRef.current) {
    //         throw new Error('Crop canvas does not exist');
    //     }

    //     previewCanvasRef.current.toBlob((blob) => {
    //         if (!blob) {
    //             throw new Error('Failed to create blob');
    //         }

    //         if (blobUrlRef.current) {
    //             URL.revokeObjectURL(blobUrlRef.current);
    //         }

    //         blobUrlRef.current = URL.createObjectURL(blob);
    //         hiddenAnchorRef.current.href = blobUrlRef.current;
    //         hiddenAnchorRef.current.click();
    //     });
    // };

    useDebounceEffect(
        async () => {
            if (completedCrop?.width && 
                completedCrop?.height && 
                imgRef.current && 
                previewCanvasRef.current
            ) {
                canvasPreview(
                    imgRef.current,
                    previewCanvasRef.current,
                    completedCrop,
                    scale,
                    rotate
                );
            }
        },
        100,
        [completedCrop, scale, rotate]
    );

    // const handleToggleAspectClick = () => {
    //     if (aspect) {
    //         setAspect(undefined);
    //     } else if (imgRef.current) {
    //         const { width, height } = imgRef.current;
    //         setAspect(1);
    //         setCrop(centerAspectCrop(width, height, 1));
    //     }
    // };

    return (
        <div className='ImageCropper'>
            <div className='Crop-Controls'>
                <input type='file' accept='image/*' onChange={onSelectFile} />
                {/* <div>
                    <button onClick={handleToggleAspectClick}>
                        Toggle aspect { aspect ? 'off' : 'on'}
                    </button>
                </div> */}
            </div>
            {!!imgSrc && (
                <ReactCrop
                    crop={crop} 
                    onChange={(_, percentCrop) => setCrop(percentCrop)} 
                    onComplete={(c) => setCompletedCrop(c)} 
                    aspect={aspect}
                >
                    <img 
                        ref={imgRef}
                        alt='Crop me'
                        src={imgSrc} 
                        style={{ transform: `scale(${scale}) rotate(${rotate}deg)` }} 
                        onLoad={onImageLoad}
                    />
                </ReactCrop>
            )} 
            {!!completedCrop && (
                <>
                    <div>
                        <canvas 
                            ref={previewCanvasRef}
                            style={{
                                border: '1px solid black',
                                objectFit: 'contain',
                                width: completedCrop.width,
                                height: completedCrop.height
                            }}
                        />
                    </div>
                    {/* <div>
                        <button onClick={onDownloadCropClick}>Download crop</button>
                        <a
                            ref={hiddenAnchorRef}
                            download 
                            style={{
                                position: 'absolute',
                                top: '-200vh',
                                visibility: 'hidden'
                            }}
                        >
                            Hidden download
                        </a>
                    </div> */}
                </>
            )}
        </div>
    );
};

export default ImageCropper;