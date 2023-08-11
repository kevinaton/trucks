import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { 
    Box,
    Chip,
    Divider,
    Menu, 
    MenuItem, 
    Tooltip,
    Typography
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { tooltipClasses } from '@mui/material/Tooltip';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import { theme } from '../../Theme';
import AddOutOfServiceReason from '../../components/trucks/addOutOfServiceReason';
import AddOrEditDriverForTruck from '../../components/scheduling/addOrEditDriverForTruck';
import AddOrEditTrailer from '../../components/scheduling/addOrEditTrailer';
import TruckOrders from './truck-orders';
import { AlertDialog } from '../../components/common/dialogs';
import { assetType } from '../../common/enums/assetType';
import { isPastDate } from '../../helpers/misc_helper';
import { getText } from '../../helpers/localization_helper';
import { isEmpty } from 'lodash';
import { 
    setTruckIsOutOfService as onSetTruckIsOutOfService,
    resetSetTruckIsOutOfService as onResetTruckIsOutOfService,
    hasOrderLineTrucks,
    hasOrderLineTrucksReset as onResetHasOrderLineTrucks,
    setTrailerForTractor as onSetTrailerForTractor,
    setTrailerForTractorReset as onResetSetTrailerForTractor
} from '../../store/actions';
import { useSnackbar } from 'notistack';

const ContextMenuWrapper = styled(Menu)(({ theme }) => ({
    '& .MuiPaper-root': {
        borderRadius: '8px',
        boxShadow: '0 0 6px 6px rgba(69, 65, 78, 0.08)',

        '& .MuiList-root': {
            padding: 0
        }
    },

    '& .MuiBackdrop-root': {
        backgroundColor: 'rgba(214, 234, 239, 0.41)', // Replace 'your-color-here' with your desired color
    }
}));

const HtmlTooltip = styled(({ className, ...props }) => (
    <Tooltip {...props} classes={{ popper: className }} />
))(({ theme }) => ({
    [`& .${tooltipClasses.tooltip}`]: {
        backgroundColor: '#f5f5f9',
        color: 'rgba(0, 0, 0, 0.87)',
        maxWidth: 220,
        fontSize: theme.typography.pxToRem(12),
        border: '1px solid #dadde9',
    },
}));

const TruckBlockItem = ({
    truck,
    truckColors,
    userAppConfiguration,
    dataFilter, 
    truckHasNoDriver, 
    truckCategoryNeedsDriver,
    orders,
    openModal,
    closeModal,
    openDialog,
    setIsUIBusy
}) => {
    const [showMenu, setShowMenu] = useState(false);
    const [menuAnchorPoint, setMenuAnchorPoint] = useState(null);
    const [sessionOfficeId, setSessionOfficeId] = useState(null);
    const [canShowOrderLines, setCanShowOrderLines] = useState(null);
    const [selectedTrailer, setSelectedTrailer] = useState(null);
    const [promptOptions, setPromptOptions] = useState(null);
    const [isToReplace, setIsToReplace] = useState(null);
    const [validationResult, setValidationResult] = useState(null);

    const { enqueueSnackbar } = useSnackbar();
    const dispatch = useDispatch();
    const { 
        userProfileMenu,
        setTruckIsOutOfServiceSuccess,
        hasOrderLineTrucksResponse,
        setTrailerForTractorResponse
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu,
        setTruckIsOutOfServiceSuccess: state.TruckReducer.setTruckIsOutOfServiceSuccess,
        hasOrderLineTrucksResponse: state.DriverAssignmentReducer.hasOrderLineTrucksResponse,
        setTrailerForTractorResponse: state.TrailerAssignmentReducer.setTrailerForTractorResponse
    }));

    useEffect(() => {
        if (sessionOfficeId === null && 
            !isEmpty(userProfileMenu) && 
            !isEmpty(userProfileMenu.result)
        ) {
            const { ...session } = userProfileMenu.result;
            setSessionOfficeId(session.sessionOfficeId);
        }
    }, [sessionOfficeId, userProfileMenu]);

    useEffect(() => {
        if (truck !== null && canShowOrderLines === null) {
            setCanShowOrderLines(truck.utilization > 0);
        }
    }, []);

    useEffect(() => {
        if (setTruckIsOutOfServiceSuccess) {
            dispatch(onResetTruckIsOutOfService());
        }
    }, [setTruckIsOutOfServiceSuccess]);

    useEffect(() => {
        if (!isEmpty(hasOrderLineTrucksResponse)) {
            const { truckId, response } = hasOrderLineTrucksResponse;
            if (truckId !== null && 
                truckId === truck.id && 
                !isEmpty(response.result)
            ) {
                const { hasOrderLineTrucks } = response.result;
                if (isPastDate(dataFilter.date)) {
                    setValidationResult({});
                    setIsUIBusy(false);
                } else if (hasOrderLineTrucks && !isEmpty(promptOptions)) {
                    openDialog({
                        type: 'confirm',
                        content: (
                            <Box
                                display='flex'
                                alignItems='center'
                                flexDirection='column'
                            >
                                <Box 
                                    display='flex' 
                                    alignItems='center' 
                                    justifyContent='center'
                                    sx={{
                                        marginBottom: '15px'
                                    }}
                                >
                                    <ErrorOutlineIcon 
                                        sx={{ 
                                            color: theme.palette.warning.main,
                                            fontSize: '88px !important'
                                        }} 
                                    />
                                </Box>
                                <Typography variant='h6'>{getText('TrailerAlreadyScheduledForTruck{0}Prompt_YesToReplace', promptOptions.truckCode)}</Typography>
                            </Box>
                        ),
                        action: () => handleIsToReplace(true),
                        primaryBtnText: 'Yes',
                        secondaryBtnText: 'No'
                    });
                }
            }
        }
    }, [hasOrderLineTrucksResponse]);

    useEffect(() => {
        if (isToReplace !== null) {
            if (isToReplace) {
                const { hasOpenDispatches } = hasOrderLineTrucksResponse.result;
                if (hasOpenDispatches) {
                    openDialog({
                        type: 'alert',
                        contenxt: (
                            <AlertDialog variant='error' message={getText('CannotChangeTrailerBecauseOfDispatchesError')} />
                        )
                    });
                } else {
                    setValidationResult({
                        updateExistingOrderLineTrucks: true
                    });
                }

                setIsUIBusy(false);
            }

            dispatch(onResetHasOrderLineTrucks());
        }
    }, [isToReplace]);

    useEffect(() => {
        if (validationResult !== null) {
            setTrailerTractor({
                tractorId: truck.id,
                trailerId: selectedTrailer !== null ? selectedTrailer.id : null,
            });

            if (selectedTrailer !== null) {
                setSelectedTrailer(null);
            }

            setValidationResult(null);
            setPromptOptions(null);
            setIsToReplace(null);
        }
    }, [validationResult]);
    
    useEffect(() => {
        if (!isEmpty(setTrailerForTractorResponse) && setTrailerForTractorResponse.success) {
            const { truckId } = setTrailerForTractorResponse;
            if (truckId === truck.id) {
                dispatch(onResetSetTrailerForTractor());
                enqueueSnackbar('Saved successfully', { variant: 'success' });
            }
        }
    }, [setTrailerForTractorResponse]);

    const getCombinedTruckCode = (truck) => {
        const { showTrailersOnSchedule } = userAppConfiguration.settings;
        if (showTrailersOnSchedule) {
            if (truck.canPullTrailer && truck.trailer) {
                return `${truck.truckCode} :: ${truck.trailer.truckCode}`;
            }

            if (truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor) {
                return `${truck.tractor.truckCode} :: ${truck.truckCode}`;
            }
        }

        return truck.truckCode;
    };
    
    const truckHasOrderLineTrucks = truck => {
        const orderLines = orders !== null 
            ? orders
            : [];
        return orderLines.some(o => o.trucks.some(olt => olt.truckId === truck.id));
    };

    const setTrailerTractor = options => {
        dispatch(onSetTrailerForTractor(truck.id, {
            date: dataFilter.date,
            shift: dataFilter.shift,
            officeId: dataFilter.officeId,
            ...options
        }));
    };

    const promptWhetherToReplaceTrailerOnExistingOrderLineTrucks = options => {
        setIsUIBusy(true);
        setPromptOptions(options);

        if (options.truckId) {
            dispatch(hasOrderLineTrucks(truck.id, {
                trailerId: options.trailerId,
                forceTrailerIdFilter: true,
                truckId: options.truckId,
                officeId: dataFilter.officeId,
                date: dataFilter.date,
                shift: dataFilter.shift
            }));
        }
    };

    const handleSelectTrailer = trailerSelection => {
        if (selectedTrailer === null) {
            setSelectedTrailer(trailerSelection);
            promptWhetherToReplaceTrailerOnExistingOrderLineTrucks({
                truckId: truck.id,
                truckCode: truck.truckCode
            });
        }
    };

    const handleIsToReplace = val => setIsToReplace(val);

    const handleShowMenu = (e) => {
        e.preventDefault();

        setMenuAnchorPoint({ mouseX: e.clientX, mouseY: e.clientY });
        setShowMenu(true);
    };

    const handleCloseMenu = () => {
        setShowMenu(false);
    };

    const handlePlaceBackInService = (e) => {
        e.preventDefault();
        dispatch(onSetTruckIsOutOfService({
            isOutOfService: false,
            truckId: truck.id
        }));
        handleCloseMenu();
    };

    const handlePlaceOutOfService = (e) => {
        e.preventDefault();

        const data = {
            truckId: truck.id,
            truckCode: truck.truckCode,
            scheduleDate: dataFilter.date,
            shift: dataFilter.shift,
        };

        openModal(
            <AddOutOfServiceReason 
                data={data} 
                closeModal={closeModal} 
                openDialog={openDialog}
            />,
            400
        );
        handleCloseMenu();
    };

    const handleNoDriverForTruck = () => {
        handleCloseMenu();
    };

    const handleAssignDriver = (e) => {
        e.preventDefault();

        const data = {
            truckId: truck.id,
            truckCode: truck.truckCode,
            leaseHaulerId: truck.leaseHaulerId,
            date: dataFilter.date,
            shift: dataFilter.shift,
            officeId: dataFilter.officeId
        };

        openModal(
            <AddOrEditDriverForTruck 
                userAppConfiguration={userAppConfiguration}
                data={data} 
                closeModal={closeModal} 
            />,
            400
        );
        handleCloseMenu();
    };

    const handleChangeDriver = (e) => {
        e.preventDefault();

        const data = {
            truckId: truck.id,
            truckCode: truck.truckCode,
            leaseHaulerId: truck.leaseHaulerId,
            date: dataFilter.date,
            shift: dataFilter.shift,
            officeId: dataFilter.officeId,
            driverId: truck.driverId,
            driverName: truck.driverName
        };

        openModal(
            <AddOrEditDriverForTruck 
                userAppConfiguration={userAppConfiguration}
                data={data} 
                closeModal={closeModal} 
            />,
            400
        );
        handleCloseMenu();
    };

    const handleAssignDefaultDriverBackToTruck = () => {
        handleCloseMenu();
    };

    const handleRemoveFromSchedule = () => {
        handleCloseMenu();
    };

    const handleAddTrailer = (e) => {
        e.preventDefault();
        openModal(
            <AddOrEditTrailer
                data={{
                    truckId: truck.id,
                    truckCode: truck.truckCode,
                    date: dataFilter.date,
                    shift: dataFilter.shift,
                    officeId: dataFilter.officeId
                }} 
                closeModal={closeModal} 
                handleSelectTrailer={(trailerSelection) => handleSelectTrailer(trailerSelection)}
            />,
            400
        );

        handleCloseMenu();
    };

    const handleChangeTrailer = (e) => {
        e.preventDefault();
        openModal(
            <AddOrEditTrailer
                data={{
                    truckId: truck.id,
                    truckCode: truck.truckCode,
                    date: dataFilter.date,
                    shift: dataFilter.shift,
                    officeId: dataFilter.officeId
                }} 
                closeModal={closeModal} 
                handleSelectTrailer={(trailerSelection) => handleSelectTrailer(trailerSelection)} 
                editTrailer={{
                    trailerId: truck.trailer.id,
                    trailerTruckCode: truck.trailer.truckCode,
                    trailerVehicleCategoryId: truck.trailer.vehicleCategoryId,
                    subTitle: `${truck.truckCode} is currently coupled to 
                        ${truck.trailer.truckCode} - ${truck.trailer.vehicleCategory.name} 
                        ${truck.trailer.make} ${truck.trailer.model} ${truck.trailer.bedConstructionFormatted} bed`
                }}
            />,
            400
        );
        
        handleCloseMenu();
    };

    const handleRemoveTrailer = (e) => {
        e.preventDefault();
        promptWhetherToReplaceTrailerOnExistingOrderLineTrucks({
            trailerId: truck.trailer.id,
            truckId: truck.id,
            truckCode: truck.truckCode
        });
        handleCloseMenu();
    };

    const handleAddTractor = () => {
        handleCloseMenu();
    };

    const handleRemoveTractor = () => {
        handleCloseMenu();
    };

    const handleShare = () => {
        handleCloseMenu();
    };

    const handleRevokeShare = () => {
        handleCloseMenu();
    };

    const handleShowOrderLines = (e) => {
        e.preventDefault();

        const data = {
            truckId: truck.id,
            truckCode: truck.truckCode,
            scheduleDate: dataFilter.date,
            shift: dataFilter.shift,
        };

        openModal(
            <TruckOrders 
                filter={data} 
                closeModal={closeModal}
            />,
            680
        );
    };

    const renderTextValue = (data) => {
        if (data === null || data === undefined) {
            return '';
        }

        return data;
    }

    const renderTruckTitle = truck => {
        return (
            <React.Fragment>
                <Typography color="inherit" display='block'>{getCombinedTruckCode(truck)}</Typography>

                { truck.vehicleCategory.assetType === assetType.TRAILER && 
                    <React.Fragment>
                        <Typography color="inherit" variant='caption' display='block'>{`${renderTextValue(truck.vehicleCategory.name)} ${renderTextValue(truck.bedConstructionFormatted)}`}</Typography>
                        <Typography color="inherit" variant='caption' display='block'>{`${renderTextValue(truck.year)} ${renderTextValue(truck.make)} ${renderTextValue(truck.model)}`}</Typography>
                    </React.Fragment>
                }
                
                { truckCategoryNeedsDriver && 
                    <Typography color="inherit" variant='caption' display='block'>Driver: {truck.driverName}</Typography>
                }
            </React.Fragment>
        );
    };

    const renderContextMenu = () => {
        const { features } = userAppConfiguration;
        return (
            <ContextMenuWrapper
                open={showMenu}
                onClose={handleCloseMenu}
                anchorReference="anchorPosition"
                anchorPosition={ menuAnchorPoint !== null
                    ? { top: menuAnchorPoint.mouseY, left: menuAnchorPoint.mouseX }
                    : undefined
                }
            >
                {/* Plase back in service */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && truck.isOutOfService && 
                    <MenuItem onClick={(e) => handlePlaceBackInService(e)}>Place back in service</MenuItem>
                }
                
                {/* Place out of service */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && !truck.isOutOfService && 
                    <MenuItem onClick={(e) => handlePlaceOutOfService(e)}>Place out of service</MenuItem>
                }

                {/* No driver for truck */}
                { !truck.isExternal && !truckHasNoDriver && truck.vehicleCategory.isPowered && !isPastDate(dataFilter.date) && 
                    <MenuItem onClick={handleNoDriverForTruck}>No driver for truck</MenuItem>
                }

                {/* Assign driver */}
                { !truck.isExternal && truckHasNoDriver && truck.vehicleCategory.isPowered && 
                    <MenuItem onClick={handleAssignDriver}>Assign driver</MenuItem>
                }

                {/* Change driver */}
                { (truck.isExternal || !truckHasNoDriver) && truck.vehicleCategory.isPowered &&
                    <MenuItem onClick={handleChangeDriver}>Change driver</MenuItem>
                }

                {/* Assign default driver back to truck */}
                { !truck.isExternal && truck.hasNoDriver && truck.hasDefaultDriver && !isPastDate(dataFilter.date) && 
                    <MenuItem onClick={handleAssignDefaultDriverBackToTruck}>Assign default driver back to truck</MenuItem>
                }

                {/* Remove from schedule */}
                { truck.isExternal && !truckHasOrderLineTrucks(truck) && 
                    <MenuItem onClick={handleRemoveFromSchedule}>Remove from schedule</MenuItem>
                }

                {/* Add trailer */}
                { truck.canPullTrailer && !truck.trailer &&
                    <MenuItem onClick={(e) => handleAddTrailer(e)}>Add trailer</MenuItem>
                }

                { truck.canPullTrailer && truck.trailer &&
                    <React.Fragment>
                        {/* Change trailer */}
                        <MenuItem onClick={(e) => handleChangeTrailer(e)}>Change trailer</MenuItem>
                        {/* Remove trailer */}
                        <MenuItem onClick={(e) => handleRemoveTrailer(e)}>Remove trailer</MenuItem>
                    </React.Fragment>
                }

                {/* Add tractor */}
                { truck.vehicleCategory.assetType === assetType.TRAILER && !truck.tractor && 
                    <MenuItem onClick={handleAddTractor}>Add tractor</MenuItem>
                }

                { truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor && 
                    <React.Fragment>
                        {/* Change tractor */}
                        <MenuItem onClick={handleAddTractor}>Change tractor</MenuItem>
                        {/* Remove tractor */}
                        <MenuItem onClick={handleRemoveTractor}>Remove tractor</MenuItem>
                    </React.Fragment>
                    
                }

                {/* Separator */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule 
                    && ((features.allowMultiOffice && truck.shareWithOfficeId === null && !truck.isOutOfService) || (truck.sharedWithOfficeId !== null && truck.sharedWithOfficeId !== sessionOfficeId)) && 
                    <Divider />
                }
                
                {/* Share */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && features.allowMultiOffice && truck.sharedWithOfficeId === null && !truck.isOutOfService && 
                    <MenuItem onClick={handleShare}>Share</MenuItem>
                }
                
                {/* Revoke share */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && truck.sharedWithOfficeId !== null && truck.sharedWithOfficeId !== sessionOfficeId && 
                    <MenuItem 
                        onClick={handleRevokeShare} 
                        disabled={truck.officeId !== sessionOfficeId || truck.actualUtilization !== 0}
                    >
                        Revoke Share
                    </MenuItem>
                }
            </ContextMenuWrapper>
        );
    };
    
    return (
        <div onContextMenu={(e) => !showMenu 
            ? handleShowMenu(e) 
            : e.preventDefault()
        }>
            <HtmlTooltip
                title={renderTruckTitle(truck)}
            >
                <Chip
                    label={getCombinedTruckCode(truck)} 
                    onClick={(e) => canShowOrderLines 
                        ? handleShowOrderLines(e) 
                        : e.preventDefault() 
                    }
                    sx={{
                        width: '80px',
                        borderRadius: 0,
                        fontSize: 17,
                        fontWeight: 600,
                        py: 3,
                        backgroundColor: `${truckColors.backgroundColor}`,
                        color: `${truckColors.color}`,
                        border: `${truckColors.border}`,
                        cursor: canShowOrderLines ? 'pointer' : 'default',
                        '&:hover': {
                            backgroundColor: `${truckColors.backgroundColor}`
                        }
                    }}
                />
            </HtmlTooltip>

            { truck !== null && !isEmpty(userAppConfiguration) && renderContextMenu()}
        </div>
    );
};

export default TruckBlockItem;