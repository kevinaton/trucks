import { createTheme } from '@mui/material';

export const theme = createTheme({
    // These are the colors used throughout the app. Please use the palette as much as possible when using colors
    palette: {
        primary: {
            main: '#67A2D7',
            light: '#ecf6fd',
            contrastText: '#ffffff',
        },
        secondary: {
            main: '#F4F5F8',
            contrastText: '#303030',
        },
        success: {
            main: '#00A65A',
        },
        error: {
            main: '#DD4B39',
        },
        info: {
            main: '#c9dae1',
        },
        text: {
            primary: '#303030',
            disabled: '#616161',
        },
        grey: {
            A100: '#f8f9fa',
            A200: '#f1f5f8',
        },
        action: {
            hover: '#F8F9FA',
            selected: '#ecf6fd',
            disabled: '#ebedf3',
        },
        gradient: {
            main: 'linear-gradient(-60deg, #4991f8 0%, #4bc1ff 100%)',
            mainChannel: '0 0 0',
        },
    },

    typography: {
        fontFamily: ['Poppins', 'Roboto', 'Helvetica Neue'].join(','),
        fontSize: 12,
        h5: {
            fontWeight: 700,
        },
        color: '#546674',
    },

    components: {
        MuiIconButton: {
            defaultProps: {
                size: 'small',
            },
            styleOverrides: {
                root: ({ theme }) => theme.unstable_sx({}),
            },
        },

        MuiDrawer: {
            styleOverrides: {
                root: ({ theme }) =>
                    theme.unstable_sx({
                        '& .MuiDrawer-paper': { borderWidth: 0 },
                    }),
            },
        },

        MuiListItemButton: {
            styleOverrides: {
                root: {
                    '&:hover': {
                        color: '#698092',
                        backgroundColor: (theme) => theme.palette.action.hover, // background color of ListItemButton when hovered
                    },
                    '& .MuiTouchRipple-root': {
                        color: '#58a3dc', // Set the desired ripple color
                    },
                    '&.Mui-selected': {
                        backgroundColor: '#ecf6fd', //selected button color
                        color: '#6AA0CA', //color of text of the selected button
                        icon: '#6AA0CA', //color of icon of the selected button
                    },
                },
            },
        },

        MuiOutlinedInput: {
            styleOverrides: {
                root: {
                    '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#ebedf2',
                    },
                },
            },
        },

        MuiSelect: {
            //IN PROGRESS
            styleOverrides: {
                root: {
                    borderColor: '#000000',
                },
            },
        },

        MuiToggleButton: {
            styleOverrides: {
                root: {
                    backgroundColor: '#ffffff',
                    borderColor: '#E9EDF2',
                    '&.Mui-selected, &.Mui-selected:hover': {
                        backgroundColor: '#F4F5F8',
                        color: 'rgba(0, 0, 0, 0.54)',
                    },
                },
            },
        },

        MuiPaper: {
            styleOverrides: {
                rounded: {
                    borderColor: '#ebedf2',
                },
            },
        },

        MuiFormLabel: {
            styleOverrides: {
                asterisk: { color: '#DD4B39' },
            },
        },
    },

    shadows: Array(25)
        .fill('none')
        .map((_, i) => `0px ${i + 1}px ${i + 1}px rgba(69, 65, 78, 0.08)`),
    // shadows: { 6: "0 1px 15px 1px rgba(69,65,78,0.1)" },
});

export default theme;
