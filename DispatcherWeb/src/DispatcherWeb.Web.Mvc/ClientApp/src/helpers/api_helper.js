import axios from 'axios'

// Get the current domain name
const currentDomain = window.location.origin

// Set the base URL dynamically based on the current domain name
const baseUrl = currentDomain
const API_URL = `${baseUrl}/api/services/app`

const axiosApi = axios.create({
    baseURL: API_URL,
    withCredentials: true
})

axiosApi.interceptors.request.use(
    (response) => response,
    (error) => Promise.reject(error)
)

export async function get(url, config = {}) {
    return await axiosApi
        .get(url, { ...config })
        .then((response) => response.data)
}