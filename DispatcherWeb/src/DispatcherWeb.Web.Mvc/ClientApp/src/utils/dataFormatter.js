import moment from 'moment';

// Utility function to get default value if the provided value is null or undefined
export const getDefaultVal = (value, defaultValue = '') => {
    return value ?? defaultValue;
}
  
  // Utility function to format date as 'MM/DD/YYYY'
export const formatDate = (date) => {
    return date ? moment(date).format('MM/DD/YYYY') : '';
}