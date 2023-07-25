import '../../fontawesome/css/all.css';

export const sideMenuItems = [
    {
        icon: 'fa-regular fa-display-chart-up',
        name: 'Dashboard',
        path: '/app/dashboard',
        select: false,
    },
    {
        icon: 'fa-regular fa-user-tie',
        name: 'Customers',
        path: '/customers',
        select: false,
    },
    {
        icon: 'fa-regular fa-boxes-stacked',
        name: 'Products/Services',
        path: '/products-services',
        select: false,
    },
    {
        icon: 'fa-regular fa-steering-wheel',
        name: 'Drivers',
        path: '/drivers',
        select: false,
    },
    {
        icon: 'fa-regular fa-location-dot',
        name: 'Locations',
        path: '/locations',
        select: false,
    },
    {
        icon: 'fa-regular fa-file-contract',
        name: 'Quotes',
        path: '/quotes',
        select: false,
    },
    {
        icon: 'fa-regular fa-clipboard-list',
        name: 'Orders',
        path: '/order',
        select: false,
    },
    {
        icon: 'fa-regular fa-truck-ramp-box',
        name: 'Dispatching',
        submenu: [
            {
                icon: 'fa-regular fa-calendar-range',
                name: 'Schedule',
                path: 'dispatching/schedule',
                select: false,
            },
            {
                icon: 'fa-regular fa-message',
                name: 'Messages',
                path: 'dispatching/messages',
                select: false,
            },
            {
                icon: 'fa-regular fa-calendar-lines',
                name: 'Truck Dispatch List',
                path: '/',
                select: true,
            },
            {
                icon: 'fa-regular fa-steering-wheel',
                name: 'Driver Assignments',
                path: 'dispatching/driver-assignments',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-ticket',
        name: 'Tickets & Loads',
        submenu: [
            {
                icon: 'fa-regular fa-trailer',
                name: 'Loads',
                path: 'tickets-&-loads/loads',
                select: false,
            },
            {
                icon: 'fa-regular fa-ticket-simple',
                name: 'Tickets',
                path: 'tickets-&-loads/tickets',
                select: false,
            },
            {
                icon: 'fa-regular fa-ticket-simple',
                name: 'Tickets by Driver',
                path: 'tickets-&-loads/tickets-by-driver',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-file-invoice',
        name: 'Invoices',
        select: false,
    },
    {
        icon: 'fa-regular fa-truck-moving',
        name: 'Lease Hauler',
        submenu: [
            {
                icon: 'fa-regular fa-truck',
                name: 'Lease Haulers',
                select: false,
            },
            {
                icon: 'fa-regular fa-truck-moving',
                name: 'Lease Hauler Requests',
                select: false,
            },
            {
                icon: 'fa-regular fa-truck-moving',
                name: 'Lease Hauler Pay',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-clock',
        name: 'Time',
        select: false,
        submenu: [
            {
                icon: 'fa-regular fa-stopwatch',
                name: 'Time Entry',
                select: false,
            },
            {
                icon: 'fa-regular fa-user-clock',
                name: 'Time Off',
                select: false,
            },
            {
                icon: 'fa-regular fa-list-timeline',
                name: 'Time Classification',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-envelope-open-dollar',
        name: 'Driver Pay',
        select: false,
    },
    {
        icon: 'fa-regular fa-truck-container',
        name: 'Trucks',
        submenu: [
            {
                icon: 'fa-regular fa-truck-container',
                name: 'Trucks',
                select: false,
            },
            {
                icon: 'fa-regular fa-toolbox',
                name: 'Services',
                select: false,
            },
            {
                icon: 'fa-regular fa-calendar-day',
                name: 'PM Schedule',
                select: false,
            },
            {
                icon: 'fa-regular fa-car-wrench',
                name: 'Work Orders',
                select: false,
            },
            {
                icon: 'fa-regular fa-gas-pump',
                name: 'Fuel',
                select: false,
            },
            {
                icon: 'fa-regular fa-road-circle-xmark',
                name: 'Out of Service Trucks',
                select: false,
            },
            {
                icon: 'fa-regular fa-tire',
                name: 'Vehicle Usage',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-print',
        name: 'Reports',
        submenu: [
            {
                icon: 'fa-regular fa-file-chart-pie',
                name: 'Delivery Report',
                select: false,
            },
            {
                icon: 'fa-regular fa-file-invoice-dollar',
                name: 'Revenue Breakdown by Order',
                select: false,
            },
            {
                icon: 'fa-regular fa-file-invoice-dollar',
                name: 'Revenue Breakdown by Truck',
                select: false,
            },
            {
                icon: 'fa-regular fa-file-invoice-dollar',
                name: 'Revenue Analysis',
                select: false,
            },
            {
                icon: 'fa-regular fa-calendar-lines-pen',
                name: 'Scheduled Reports',
                select: false,
            },
            {
                icon: 'fa-regular fa-receipt',
                name: 'Receipts',
                select: false,
            },
            {
                icon: 'fa-regular fa-timeline-arrow',
                name: 'Driver Activity Report',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-file-import',
        name: 'Imports',
        submenu: [
            {
                icon: 'fa-regular fa-gas-pump',
                name: 'Fuel',
                select: false,
            },
        ],
    },
    {
        icon: 'fa-regular fa-wrench',
        name: 'Administration',
        submenu: [
            {
                icon: 'fa-regular fa-list-tree',
                name: 'Canned Texts',
                select: false,
            },
            {
                icon: 'fa-regular fa-network-wired',
                name: 'Organization Units',
                select: false,
            },
            {
                icon: 'fa-regular fa-id-badge',
                name: 'Roles',
                select: false,
            },
            {
                icon: 'fa-regular fa-user',
                name: 'Users',
                select: false,
            },
            {
                icon: 'fa-regular fa-language',
                name: 'Languages',
                select: false,
            },
            {
                icon: 'fa-regular fa-table-list',
                name: 'Audit Logs',
                select: false,
            },
            {
                icon: 'fa-regular fa-repeat',
                name: 'Subscription',
                select: false,
            },
            {
                icon: 'fa-regular fa-gear',
                name: 'Settings',
                select: false,
            },
        ],
    },
];

export const ProfileList = [
    {
        id: 1,
        name: 'Back to my account',
        icon: 'fa-angle-left',
        path: '/',
    },
    {
        id: 2,
        name: 'Manage linked accounts',
        icon: 'fa-users-gear',
        path: '/',
    },
    {
        id: 3,
        name: 'Login attempts',
        icon: 'fa-list-check',
        path: '/',
    },
    {
        id: 4,
        name: 'Change profile picture',
        icon: 'fa-square-user',
        path: '/',
    },
    {
        id: 5,
        name: 'Upload signature picture',
        icon: 'fa-signature',
        path: '/',
    },
    {
        id: 6,
        name: 'My settings',
        icon: 'fa-gear',
        path: '/',
    },
];
