import axios from 'axios'

const API_URL = 'https://localhost:44332/api/services/app'

const apiService = axios.create({
    baseURL: API_URL,
    withCredentials: true
})

apiService.interceptors.request.use(
    (response) => response,
    (error) => Promise.reject(error)
)

// export async function get(url, config = {}) {
//     return await axiosApi
//         .get(url, { ...config })
//         .then((response) => response.data)
// }

export default apiService