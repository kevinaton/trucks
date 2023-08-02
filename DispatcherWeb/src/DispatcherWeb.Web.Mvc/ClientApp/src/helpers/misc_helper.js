import moment from 'moment';

export const renderTime = (value, emptyValue) => {
    if (value) {
        return moment(value, ['YYYY-MM-DDTHH:mm:ss', 'hh:mm A']).format('LT');
    }
    return emptyValue !== undefined ? emptyValue : '-';
}

export const renderDate = (value, emptyValue) => {
    if (value) {
        return moment(value, ['YYYY-MM-DDTHH:mm:ss', 'MM/DD/YYYY']);
    }
    return emptyValue !== undefined ? emptyValue : null;
}

export const isToday = (value) => {
    var isToday = moment(value, 'MM/DD/YYYY').isSame(moment().startOf('day'));
    return isToday;
}

export const isPastDate = (value) => {
    var isPastDate = moment(value, 'MM/DD/YYYY') < moment().startOf('day');
    return isPastDate;
};

export const round = num => {
    num = Number(num);
    if (isNaN(num))
        return null;
    return Math.round((num + 0.00001) * 100) / 100;
}