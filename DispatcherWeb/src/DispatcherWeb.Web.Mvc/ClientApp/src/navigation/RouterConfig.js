import React, { useEffect } from 'react';
import { Routes, Route } from 'react-router';
import { useLocation, useNavigate } from 'react-router-dom';
import { authProtectedRoutes } from './index';
  
export const RouterConfig = ({
    isAuthenticated,
    userAppConfiguration,
    handleCurrentPageName,
    openModal,
    closeModal,
    openDialog,
    closeDialog
}) => {
    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);
    const targetRoute = queryParams.get('route');
    const navigate = useNavigate();

    useEffect(() => {
        if (targetRoute) {
            navigate(targetRoute);
        }
    }, [targetRoute, navigate]);

    return (
        <React.Fragment>
            { isAuthenticated && 
                <Routes sx={{ height: '100%', overflow: 'auto' }}>
                    { authProtectedRoutes.map((route, index) => {
                        return (
                            <Route
                                key={index}
                                path={route.path}
                                element={
                                    <route.component 
                                        userAppConfiguration={userAppConfiguration}
                                        handleCurrentPageName={handleCurrentPageName} 
                                        openModal={openModal} 
                                        closeModal={closeModal} 
                                        openDialog={openDialog} 
                                        closeDialog={closeDialog}
                                    />
                                }
                            />
                        );
                    })}
        
                    {targetRoute && <Route path={targetRoute} component={() => null} />}
                </Routes>
            }
        </React.Fragment>
    );
};
