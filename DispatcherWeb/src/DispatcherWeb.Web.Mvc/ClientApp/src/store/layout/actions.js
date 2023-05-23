import {
    GET_MENU_ITEMS,
    GET_MENU_ITEMS_SUCCESS,
    GET_MENU_ITEMS_FAILURE,
    GET_SUPPORT_LINK_ADDRESS,
    GET_SUPPORT_LINK_ADDRESS_SUCCESS,
    GET_SUPPORT_LINK_ADDRESS_FAILURE,
} from './actionTypes'

export const getMenuItems = () => ({
    type: GET_MENU_ITEMS
})

export const getMenuItemsSuccess = menuItems => ({
    type: GET_MENU_ITEMS_SUCCESS,
    payload: menuItems
})

export const getMenuItemsFailure = error => ({
    type: GET_MENU_ITEMS_FAILURE,
    payload: error
})

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