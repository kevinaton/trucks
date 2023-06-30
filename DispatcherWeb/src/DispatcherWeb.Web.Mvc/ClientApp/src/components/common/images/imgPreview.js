import { canvasPreview } from './canvasPreview';

let previewUrl = '';

const toBlob = (canvas) => {
    return new Promise((resolve) => {
        canvas.toBlob(resolve);
    });
};

export const imgPreview = async (image, crop, scale = 1, rotate = 0) => {
    const canvas = document.createElement('canvas');
    await canvasPreview(image, canvas, crop, scale, rotate);

    const blob = await toBlob(canvas);

    if (!blob) {
        console.error('Failed to create blob');
        return '';
    }

    if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
    }

    previewUrl = URL.createObjectURL(blob);
    return previewUrl;
};