import React, { useState, useEffect, useRef } from 'react';
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
import { assetType } from '../../common/enums/assetType';
import { isPastDate } from '../../helpers/misc_helper';
import { isEmpty } from 'lodash';

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
    openModal
}) => {
    const [showMenu, setShowMenu] = useState(false);
    const [menuAnchorPoint, setMenuAnchorPoint] = useState(null);

    const getCombinedTruckCode = (truck) => {
        if (pageConfig.settings.validateUtilization) {
            if (truck.canPullTrailer && truck.trailer) {
                return `${truck.truckCode} :: ${truck.trailer.truckCode}`;
            }

            if (truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor) {
                return `${truck.tractor.truckCode} :: ${truck.truckCode}`;
            }
        }

        return truck.truckCode;
    };

    // const truckHasNoDriver = truck => !truck.isExternal && 
    //     (truck.hasNoDriver || (!truck.hasDefaultDriver && !truck.hasDriverAssignment));

    // const truckCategoryNeedsDriver = truck => 
    //     truck.vehicleCategory.isPowered && 
    //     (leaseHaulers || (!truck.alwaysShowOnSchedule && !truck.isExternal));

    const truckHasOrderLineTrucks = truck => {
        // todo: implement this
        return false;
    };

    const handleShowMenu = (e) => {
        e.preventDefault();
        setMenuAnchorPoint({ mouseX: e.clientX, mouseY: e.clientY });
        setShowMenu(true);
    };

    const handleCloseMenu = () => {
        setShowMenu(false);
    };

    const handlePlaceBackInService = (e, data) => {
        e.preventDefault();
        // put back in service
        handleCloseMenu();
    };

    const handlePlaceOutOfService = (e, data) => {
        e.preventDefault();

        openModal(
            <AddOutOfServiceReason data={data} />,
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

    const renderTruckTitle = truck => {
        return (
            <React.Fragment>
                <Typography color="inherit" display='block'>{getCombinedTruckCode(truck)}</Typography>

                { truck.vehicleCategory.assetType === assetType.TRAILER && 
                    <React.Fragment>
                        <Typography color="inherit" variant='caption' display='block'>{`${truck.vehicleCategory.name} ${truck.bedConstructionFormatted}`}</Typography>
                        <Typography color="inherit" variant='caption' display='block'>{`${truck.year} ${truck.make} ${truck.model}`}</Typography>
                    </React.Fragment>
                }
                
                { truckCategoryNeedsDriver && 
                    <Typography color="inherit" variant='caption' display='block'>Driver: {truck.driverName}</Typography>
                }
            </React.Fragment>
        );
    };

    const renderMenu = () => {
        const { features } = pageConfig;
        return (
            <Menu
                open={showMenu}
                onClose={handleCloseMenu}
                anchorReference="anchorPosition"
                anchorPosition={
                    menuAnchorPoint !== null
                        ? { top: menuAnchorPoint.mouseY, left: menuAnchorPoint.mouseX }
                        : undefined
                }
            >
                {/* Plase back in service */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && truck.isOutOfService && 
                    <MenuItem onClick={(e) => handlePlaceBackInService(e, truck.id)}>Place back in service</MenuItem>
                }
                
                {/* Place out of service */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && !truck.isOutOfService && 
                    <MenuItem onClick={(e) => handlePlaceOutOfService(e, truck.id)}>Place out of service</MenuItem>
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
                { truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor !== null && !truck.tractor && 
                    <MenuItem onClick={handleAddTractor}>Add tractor</MenuItem>
                }

                { truck.vehicleCategory.assetType === assetType.TRAILER && truck.tractor !== null && truck.tractor && 
                    <React.Fragment>
                        {/* Change tractor */}
                        <MenuItem onClick={handleAddTractor}>Change tractor</MenuItem>
                        {/* Remove tractor */}
                        <MenuItem onClick={handleRemoveTractor}>Remove tractor</MenuItem>
                    </React.Fragment>
                    
                }

                {/* TODO: add truck.sharedWithOfficeId !== session.officeId */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule 
                    && ((features.allowMultiOffice && truck.shareWithOfficeId === null && !truck.isOutOfService) || (truck.sharedWithOfficeId !== null)) && 
                    <Divider />
                }
                
                {/* Share */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && features.allowMultiOffice && truck.sharedWithOfficeId === null && !truck.isOutOfService && 
                    <MenuItem onClick={handleShare}>Share</MenuItem>
                }
                
                {/* Revoke share */}
                {/* TODO: add truck.sharedWithOfficeId !== session.officeId */}
                { !truck.isExternal && !truck.alwaysShowOnSchedule && truck.sharedWithOfficeId !== null && 
                    <MenuItem onClick={handleRevokeShare}>Revoke Share</MenuItem>
                }
            </Menu>
        );
    };
    //console.log('rendering...')
    return (
        <div onContextMenu={handleShowMenu}>
            <HtmlTooltip
                title={renderTruckTitle(truck)}
            >
                <Chip
                    label={truck.truckCode}
                    sx={{
                        minWidth: '81px',
                        borderRadius: 0,
                        fontSize: 18,
                        fontWeight: 600,
                        py: 3,
                        backgroundColor: `${truckColors.backgroundColor}`,
                        color: `${truckColors.color}`,
                        border: `${truckColors.border}`,
                        '&:hover': {
                            backgroundColor: `${truckColors.backgroundColor}`
                        }
                    }}
                />
            </HtmlTooltip>

            { truck !== null && !isEmpty(pageConfig) && renderMenu()}
        </div>
    );
};

export default TruckBlockItem;