import { combineReducers } from 'redux'

import LayoutReducer from './layout/reducer'
import DashboardReducer from './dashboard/reducer'

const rootReducer = combineReducers({
    // public
    LayoutReducer,
    DashboardReducer
})

export default rootReducer