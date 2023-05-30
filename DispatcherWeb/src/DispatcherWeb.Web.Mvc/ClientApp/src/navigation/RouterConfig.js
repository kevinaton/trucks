import React, { useEffect } from 'react';
import { Routes, Route } from 'react-router';
import { useLocation, useNavigate } from 'react-router-dom';
import { authProtectedRoutes } from './index';
import Dashboard from '../pages/Dashboard';
import Customers from '../pages/Customers';
import ProductsOrServices from '../pages/ProductsOrServices';
import Drivers from '../pages/Drivers';
import Locations from '../pages/Locations';
import Schedule from '../pages/Schedule';
import TruckDispatchList from '../pages/TruckDispatchList';
  
export const RouterConfig = ({
    isAuthenticated,
    handleCurrentPageName
}) => {
    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);
    const targetRoute = queryParams.get('route');
    const navigate = useNavigate();

    const routes = [
        { path: '/dashboard', component: Dashboard },
        { path: '/customers', component: Customers },
        { path: '/products-services', component: ProductsOrServices },
        { path: '/drivers', component: Drivers },
        { path: '/locations', component: Locations },
        { path: '/dispatching/schedule', component: Schedule },
        { path: '/dispatching/dispatches/truck-list', component: TruckDispatchList },
    ];

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
                        const Component = route.component;
        
                        return (
                            <Route
                                key={index}
                                path={route.path}
                                element={
                                    <Component handleCurrentPageName={handleCurrentPageName} />
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
