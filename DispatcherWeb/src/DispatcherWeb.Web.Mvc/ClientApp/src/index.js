import React from 'react'
import ReactDOM from 'react-dom/client'
import './index.css'
import App from './App'
import reportWebVitals from './reportWebVitals'
import { setOptions } from '@mobiscroll/react'
import { ThemeProvider } from '@mui/material'
import { theme } from './Theme'
import { BrowserRouter } from 'react-router-dom'
import { Provider } from 'react-redux'

import store from './store'

// Apply options globally to all Mobiscroll components
setOptions({
    theme: 'material',
    themeVariant: 'light'
})

const root = ReactDOM.createRoot(document.getElementById('root'))
root.render(
    <React.StrictMode>
        <Provider store={store}>
            <BrowserRouter>
                <ThemeProvider theme={theme}>
                    <React.Fragment>
                        <App />
                    </React.Fragment>
                </ThemeProvider>
            </BrowserRouter>
        </Provider>
    </React.StrictMode>
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals()