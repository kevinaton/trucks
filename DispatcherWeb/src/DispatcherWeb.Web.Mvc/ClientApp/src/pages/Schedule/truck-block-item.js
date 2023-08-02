import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { 
    Chip,
    Divider,
    Menu, 
    MenuItem, 
    Tooltip,
    Typography
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { tooltipClasses } from '@mui/material/Tooltip';
import AddOutOfServiceReason from '../../components/trucks/addOutOfServiceReason';
import TruckOrders from './truck-orders';
import { assetType } from '../../common/enums/assetType';
import { isPastDate } from '../../helpers/misc_helper';
import { isEmpty } from 'lodash';
import { 
    setTruckIsOutOfService as onSetTruckIsOutOfService,
    resetSetTruckIsOutOfService as onResetTruckIsOutOfService
} from '../../store/actions';

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
    pageConfig,
    dataFilter, 
    truckHasNoDriver, 
    truckCategoryNeedsDriver,
    orders,
    openModal,
    closeModal
}) => {
    const [showMenu, setShowMenu] = useState(false);
    const [menuAnchorPoint, setMenuAnchorPoint] = useState(null);
    const [sessionOfficeId, setSessionOfficeId] = useState(null);
    const [canShowOrderLines, setCanShowOrderLines] = useState(null);

    const dispatch = useDispatch();
    const { 
        userProfileMenu,
        setTruckIsOutOfServiceSuccess
    } = useSelector((state) => ({
        userProfileMenu: state.UserReducer.userProfileMenu,
        setTruckIsOutOfServiceSuccess: state.TruckReducer.setTruckIsOutOfServiceSuccess
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
    }, [dispatch, setTruckIsOutOfServiceSuccess]);

    const getCombinedTruckCode = (truck) => {
        const { showTrailersOnSchedule } = pageConfig.settings;
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
            />,
            400
        );
        handleCloseMenu();
    };

    const handleNoDriverForTruck = () => {
        handleCloseMenu();
    };

    const handleAssignDriver = () => {
        handleCloseMenu();
    };

    const handleChangeDriver = () => {
        handleCloseMenu();
    };

    const handleAssignDefaultDriverBackToTruck = () => {
        handleCloseMenu();
    };

    const handleRemoveFromSchedule = () => {
        handleCloseMenu();
    };

    const handleAddTrailer = () => {
        handleCloseMenu();
    };

    const handleChangeTrailer = () => {
        handleCloseMenu();
    };

    const handleRemoveTrailer = () => {
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
        const { features } = pageConfig;
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
                    <MenuItem onClick={handleAddTrailer}>Add trailer</MenuItem>
                }

                { truck.canPullTrailer && truck.trailer &&
                    <React.Fragment>
                        {/* Change trailer */}
                        <MenuItem onClick={handleChangeTrailer}>Change trailer</MenuItem>
                        {/* Remove trailer */}
                        <MenuItem onClick={handleRemoveTrailer}>Remove trailer</MenuItem>
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

            { truck !== null && !isEmpty(pageConfig) && renderContextMenu()}
        </div>
    );
};

export default TruckBlockItem;