import moment from 'moment';

export const renderTime = (value, emptyValue) => {
    if (value) {
        return moment(value, ['YYYY-MM-DDTHH:mm:ss', 'hh:mm A']).format('LT');
    }
    return emptyValue !== undefined ? emptyValue : '-';
}

export const isToday = (value) => {
    var isToday = moment(value, 'MM/DD/YYYY').isSame(moment().startOf('day'));
    return isToday;
}

export const round = num => {
    num = Number(num);
    if (isNaN(num))
        return null;
    return Math.round((num + 0.00001) * 100) / 100;
}