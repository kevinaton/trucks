import {
    GET_SUPPORT_LINK_ADDRESS_SUCCESS,
    GET_SUPPORT_LINK_ADDRESS_FAILURE
} from './actionTypes'

const INIT_STATE = {
    supportLinkAddress: null
}

const LayoutReducer = (state = INIT_STATE, action) => {
    switch (action.type) {
        case GET_SUPPORT_LINK_ADDRESS_SUCCESS:
            return {
                ...state,
                supportLinkAddress: action.payload
            }
        case GET_SUPPORT_LINK_ADDRESS_FAILURE:
            return {
                ...state,
                error: action.payload
            }
        default:
            return state
    }   
}

export default LayoutReducer