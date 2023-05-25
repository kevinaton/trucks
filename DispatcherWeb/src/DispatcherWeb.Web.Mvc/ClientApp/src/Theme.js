import { createTheme } from '@mui/material'

export const theme = createTheme({
    palette: {
        primary: {
            main: '#67A2D7',
            contrastText: '#ffffff'
        },
        success: {
            main: '#00A65A'
        },
        error: {
            main: '#DD4B39'
        }
    },

    typography: {
        fontFamily: ['Poppins', 'Roboto', 'Helvetica Neue'].join(','),
        fontSize: 12,
        h5: {
            fontWeight: 700
        },
        color: '#546674'
    },

    components: {
        MuiIconButton: {
            defaultProps: {
                size: 'small',
            },
            styleOverrides: {
                root: ({ theme }) => theme.unstable_sx({})
            }
        },

        MuiDrawer: {
            styleOverrides: {
              root: ({ theme }) =>
                  theme.unstable_sx({
                      '& .MuiDrawer-paper': { borderWidth: 0 }
                  })
            }
        },

        MuiListItemButton: {
            styleOverrides: {
                root: {
                    '&:hover': {
                        color: '#698092',
                        backgroundColor: '#f8f9fa' // background color of ListItemButton when hovered
                    },
                    '& .MuiTouchRipple-root': {
                        color: '#58a3dc' // Set the desired ripple color
                    },
                    '&.Mui-selected': {
                        backgroundColor: '#ecf6fd', //selected button color
                        color: '#6AA0CA', //color of text of the selected button
                        icon: '#6AA0CA' //color of icon of the selected button
                    }
                }
            }
        },

        MuiOutlinedInput: {
            styleOverrides: {
                root: {
                    '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#ebedf2'
                    }
                }
            }
        },

        MuiSelect: {
            //IN PROGRESS
            styleOverrides: {
              root: {
                  borderColor: '#000000'
              }
            }
        },

        MuiToggleButton: {
            styleOverrides: {
                root: {
                    backgroundColor: '#ffffff',
                    borderColor: '#E9EDF2',
                    '&.Mui-selected, &.Mui-selected:hover': {
                        backgroundColor: '#F4F5F8',
                        color: 'rgba(0, 0, 0, 0.54)'
                    }
                }
            }
        },

        MuiPaper: {
            styleOverrides: {
                rounded: {
                    borderColor: '#ebedf2'
                }
            }
        }
    },

    shadows: Array(25)
      .fill('none')
      .map((_, i) => `0px ${i + 1}px ${i + 1}px rgba(69, 65, 78, 0.08)`),
    // shadows: { 6: '0 1px 15px 1px rgba(69,65,78,0.1)' },
})
