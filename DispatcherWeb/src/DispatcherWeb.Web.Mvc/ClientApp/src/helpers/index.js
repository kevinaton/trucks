import { baseUrl } from "./api_helper";

export const getUserProfilePicturePath = () => {
    return `${baseUrl}/Profile/GetProfilePicture?v=${new Date().valueOf()}`;
};