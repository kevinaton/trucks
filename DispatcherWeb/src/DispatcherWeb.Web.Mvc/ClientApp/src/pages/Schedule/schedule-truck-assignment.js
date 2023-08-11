import React, { useState, useEffect, useRef } from 'react';
import { Autocomplete } from '@mui/material';

const sleep = (delay = 0) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay);
    });
}

const ScheduleTruckAssignment = ({
    trucks,
    index,
    data
}) => {
    const ref = useRef();
    const [open, setOpen] = useState(false);
    const [options, setOptions] = useState([]);
    const loading = open && options.length === 0;

    useEffect(() => {
        let active = true;
    
        if (!loading) {
            return undefined;
        }
    
        (async () => {
            await sleep(1e3);
    
            if (active) {
                setOptions([...trucks]);
            }
        })();
    
        return () => {
            active = false;
        };
    }, [loading, trucks]);
    
    useEffect(() => {
        if (!open) {
            setOptions([]);
        }
    }, [open]);

    const handleAssignTruck = (index, selectedId) => {
        // Perform logic with the selected truck ID
        console.log(`Selected truck ID: ${selectedId} for index: ${index}`);
        // ...
    };

    return (
        <Autocomplete
            ref={ref}
            id={`assign-truck-${data.id}`} 
            size='small'
            sx={{ 
                width: 'auto',
                borderColor: 'transparent'
            }}
            open={open}
            onOpen={() => {
                setOpen(true);
            }}
            onClose={() => {
                setOpen(false);
            }}
            isOptionEqualToValue={(option, value) => option.truckCode === value.truckCode}
            getOptionLabel={(option) => option.truckCode}
            options={options}
            loading={loading}
            renderInput={(params) => (
                <div ref={params.InputProps.ref}>
                    <input 
                        type="text" 
                        style={{
                            border: 'none',
                            height: '32px',
                            width: '100%',
                            outline: 'none'
                        }}
                        {...params.inputProps} 
                    />
                </div>
            )}
            onChange={(e, value) => handleAssignTruck(index, value.id)}
        />
    );
};

export default ScheduleTruckAssignment;