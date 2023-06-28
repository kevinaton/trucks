import React from 'react';
import { 
    Stack, 
    TextField,
    Typography, 
    Button 
} from '@mui/material';

const FormField = ({ 
    name, 
    label, 
    type, 
    value, 
    onChange, 
    required,
    error,
    errorText,
    helpText
}) => {
    const handleInputChange = (e) => {
        onChange(e.target.value);
    };

    return (
        <div>
            <TextField 
                id={name} 
                name={name} 
                type={type} 
                label={
                    <>
                        {label}
                        {required && <span style={{ marginLeft: '5px', color: 'red' }}>*</span>}
                    </>
                }
                value={value} 
                error={error}
                helperText={error ? errorText : ''} 
                sx={{ marginBottom: helpText ? '7px' : '15px' }} 
                fullWidth 
                onChange={(e) => handleInputChange(e)} 
            />
            { helpText && helpText }
        </div>
    );
};

export const DynamicForm = ({ fields, invalidFields, onChange, onSave, onCancel }) => {
    const handleInputChange = (field, value) => {
        onChange(field, value);
    };

    const handleSave = () => {
        // Perform custom actions based on the form data
        onSave();
    };

    const handleCancel = () => {
        onCancel();
    };

    return (
        <React.Fragment>
            <div>
                {Object.keys(fields).map((field, index) => (
                    <FormField 
                        key={index} 
                        name={field}
                        label={fields[field].label} 
                        type={fields[field].type} 
                        value={fields[field].value} 
                        required={fields[field].required} 
                        error={invalidFields.includes(field)} 
                        errorText={fields[field].errorText} 
                        onChange={(value) => handleInputChange(field, value)} 
                        helpText={fields[field].helpText}
                    />
                ))}
            </div>

            <Stack 
                spacing={1}
                direction='row' 
                justifyContent='flex-end'
            >
                <Button variant='outlined' onClick={handleCancel}>Cancel</Button>
                <Button variant='contained' color='primary' onClick={handleSave}>
                    <i className='fa-regular fa-save' style={{ marginRight: '6px' }}></i>
                    <Typography>Save</Typography>
                </Button>
            </Stack>
        </React.Fragment>
    );
};