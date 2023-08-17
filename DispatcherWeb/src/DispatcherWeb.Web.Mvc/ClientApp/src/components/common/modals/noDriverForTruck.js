import * as React from 'react'
import { Datepicker } from '@mobiscroll/react'
import { Button, Card, CardActions, CardContent, CardHeader, IconButton, Modal } from '@mui/material'

const NoDriverForTruck = ({isOpen, setIsOpen}) => {

    const handleClose = () => [
        setIsOpen(false)
    ]

    return (
        <Modal open={isOpen} onClose={handleClose} aria-labelledby='no-driver-for-truck'>
            <Card
                sx={{
                    minWidth: 500,
                    position: 'absolute',
                    top: '30%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                }}
            >
                <CardHeader
                    action={
                        <IconButton>
                            <i className='fa-regular fa-close'></i>
                        </IconButton>
                    }
                    title='No driver for truck'
                    subheader='Select dates to mark a truck as having no driver'
                />
                <CardContent>
                    <Datepicker
                        controls={['calendar']}
                        select='range'
                        touchUi={true}
                        labelStyle='stacked'
                        inputStyle='outline'
                        inputProps={{
                            placeholder:
                                'mm/dd/yyyy - mm/dd/yyyy',
                            label: 'Select start and end date',
                            className:
                                'mbsc-no-margin',
                        }}
                    />
                </CardContent>
                <CardActions sx={{justifyContent:'end'}}>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button 
                        onClick={handleClose} 
                        variant='contained'
                        startIcon={
                            <i
                                className='fa-regular fa-save'
                                style={{
                                    fontSize:
                                        '0.8rem',
                                }}></i>
                        }    
                    >Save</Button>
                </CardActions>
            </Card>
        </Modal>
    )
}

export default NoDriverForTruck