import { Route, Routes } from 'react-router'
import Dashboard from '../pages/Dashboard'
import Customers from '../pages/Customers'
import ProductsOrServices from '../pages/ProductsOrServices'
import Drivers from '../pages/Drivers'
import Locations from '../pages/Locations'
import Schedule from '../pages/Schedule'
import TruckDispatchList from '../pages/TruckDispatchList'
  
export const RouterConfig = ({
    handleCurrentPageName
}) => {
    const routes = [
        { path: "/app/dashboard", component: Dashboard },
        { path: "/customers", component: Customers },
        { path: "/products-services", component: ProductsOrServices },
        { path: "/drivers", component: Drivers },
        { path: "/locations", component: Locations },
        { path: "/dispatching/schedule", component: Schedule },
        { path: "/", component: TruckDispatchList },
        { path: "*", component: TruckDispatchList } // default page
    ];

    return (
        <Routes sx={{ height: "100%", overflow: "auto" }}>
            {routes.map((route, index) => (
            <Route
                key={index}
                path={route.path}
                element={<route.component handleCurrentPageName={handleCurrentPageName} />}
            />
            ))}
        </Routes>
    )
}