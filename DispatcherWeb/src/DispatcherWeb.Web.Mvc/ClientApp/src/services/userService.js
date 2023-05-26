import apiService from '../helpers/api_helper';
import * as url from '../helpers/url_helper';

export const getLoggedInUser = () => {
    apiService.get(url.GET_CURRENT_LOGIN_INFO)
    .then(response => {
        console.log('response: ', response);
    })
    .catch(error => {
        console.log('error: ', error);
    });
};