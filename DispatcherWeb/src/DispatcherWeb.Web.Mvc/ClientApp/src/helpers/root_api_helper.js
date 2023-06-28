import axios from 'axios';

// Get the current domain name
const currentDomain = window.location.origin;

// Set the base URL dynamically based on the current domain name
export const baseUrl = currentDomain;

const axiosApi = axios.create({
    baseURL: baseUrl,
    withCredentials: true
});

axiosApi.interceptors.request.use(
    (response) => response,
    (error) => Promise.reject(error)
);

export async function rootGet(url, config = {}) {
    return await axiosApi
        .get(url, { ...config })
        .then((response) => response.data);
};

export async function rootPost(url, data, config = {}) {
    return await axiosApi
        .post(url, data, { ...config })
        .then((response) => response.data);
};