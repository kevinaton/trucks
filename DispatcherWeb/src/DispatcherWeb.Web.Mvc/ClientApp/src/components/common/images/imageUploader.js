import React, { useState } from 'react';
import { useDropzone } from 'react-dropzone';

const ImageUploader = () => {
    const [selectedImage, setSelectedImage] = useState(null);

    const onDrop = (acceptedFiles) => {
        // handle the dropped file(s) here
        const file = acceptedFiles[0];
        console.log('file: ', file)
        const reader = new FileReader();
        reader.onload = () => {
            setSelectedImage(reader.result);
        };
        reader.readAsDataURL(file);
    };

    const {
        getRootProps,
        getInputProps,
        isDragActive,
    } = useDropzone({ onDrop });

    return (
        <div {...getRootProps()} className='dropzone'>
            <input {...getInputProps()} />
            {isDragActive ? (
                <p>Drop the files here ...</p>
            ) : (
                <p>Drag and drop a file here, or click to select a file</p>
            )}
            {selectedImage && (
                <img 
                    src={selectedImage} 
                    alt='Selected' 
                />
            )}
        </div>
    );
};

export default ImageUploader;