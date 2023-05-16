import {
    GET_SUPPORT_LINK_ADDRESS,
    GET_SUPPORT_LINK_ADDRESS_SUCCESS,
    GET_SUPPORT_LINK_ADDRESS_FAILURE,
} from './actionTypes'

export const getSupportLinkAddress = () => ({
    type: GET_SUPPORT_LINK_ADDRESS
})

export const getSupportLinkAddressSuccess = link => ({
    type: GET_SUPPORT_LINK_ADDRESS_SUCCESS,
    payload: link
})

export const getSupportLinkAddressFailure = error => ({
    type: GET_SUPPORT_LINK_ADDRESS_FAILURE,
    payload: error
})