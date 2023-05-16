import axios from 'axios'

const baseUrl = process.env.REACT_APP_API_BASE_URL
const ABP_API_URL = `${baseUrl}/api/services/app`
const API_URL = `${baseUrl}/api`

const axiosAbpApi = axios.create({
    baseURL: ABP_API_URL,
    withCredentials: true
})

axiosAbpApi.interceptors.request.use(
    (response) => response,
    (error) => Promise.reject(error)
)

const axiosApi = axios.create({
    baseURL: API_URL,
    withCredentials: true
})

axiosApi.interceptors.request.use(
    (response) => response,
    (error) => Promise.reject(error)
)

export async function get(useAbp, url, config = {}) {
    if (useAbp) {
        return await axiosAbpApi
            .get(url, { ...config })
            .then((response) => response.data)
    }

    return await axiosApi
        .get(url, { ...config })
        .then((response) => response.data)
}