import React from 'react';
import { TextField, Button } from '@mui/material';

const FormField = ({ label, value, onChange }) => {
    const handleInputChange = (e) => {
        onChange(e.target.value);
    };

    return (
        <TextField
            label={label}
            value={value}
            onChange={handleInputChange}
            fullWidth 
        />
    );
};

export const DynamicForm = ({ fields, onSave, onCancel }) => {
    const handleSave = () => {
        // Perform custom actions based on the form data
        onSave();
    };

    const handleCancel = () => {
        onCancel();
    };

    return (
        <form style={{ padding: '20px' }}>
            {fields.map((field, index) => (
                <FormField 
                    key={field.name}
                    label={field.label}
                    value={field.value}
                    onChange={field.onChange}
                />
            ))}

            <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
            <Button variant='contained' color='primary' onClick={handleSave}>
                <i className='fa-regular fa-save'></i> Save
            </Button>
        </form>
    );
};