import Dashboard from '../pages/Dashboard';
import Customers from '../pages/Customers';
import ProductsOrServices from '../pages/ProductsOrServices';
import Drivers from '../pages/Drivers';
import Locations from '../pages/Locations';
import Schedule from '../pages/Schedule';
import TruckDispatchList from '../pages/TruckDispatchList';
import Quotes from '../pages/Quotes';

const authProtectedRoutes = [
    { path: '/dashboard', component: Dashboard },
    { path: '/customers', component: Customers },
    { path: '/products-services', component: ProductsOrServices },
    { path: '/drivers', component: Drivers },
    { path: '/locations', component: Locations },
    { path: '/dispatching/schedule', component: Schedule },
    { path: '/dispatching/dispatches/truck-list', component: TruckDispatchList },
    { path: '/quote', component: Quotes },
];

export { authProtectedRoutes };
