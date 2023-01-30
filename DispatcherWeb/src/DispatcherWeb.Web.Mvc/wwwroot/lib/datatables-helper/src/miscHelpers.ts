(function ($) {
    abp = abp || {};
    abp.helper = abp.helper || {};
    abp.utils = abp.utils || {};

    var uniqueElementIdNumber = 0;
    abp.helper.getUniqueElementId = function () {
        var result = 'uniqueElementId' + uniqueElementIdNumber;
        uniqueElementIdNumber++;
        return result;
    };

    abp.helper.isTimeString = function (str) {
        var r = new RegExp('^((1[0-2]|0?[1-9]):([0-5][0-9]) ([AaPp][Mm]))$');
        return r.test(str);
    };

    abp.helper.parseDateToJsonWithoutTime = function (date) {
        return moment.utc(date, 'MM/DD/YYYY').format('YYYY-MM-DD') + 'T00:00:00';
    };

    abp.utils.toPascalCaseRecursive = function (str) {
        if (!str || !str.length) {
            return str;
        }
        return $.map(str.split('.'), function (val) { return abp.utils.toPascalCase(val); }).join('.');
    };

    abp.utils.replaceAll = function (str, source, target) {
        return str.split(source).join(target);
    };

    abp.utils.truncate = function (str, maxLength, addElipsis) {
        if (!str || str.length <= maxLength || maxLength <= 0) {
            return str;
        }
        if (addElipsis) {
            maxLength--;
        }
        str = str.substring(0, maxLength);
        if (addElipsis) {
            str += '…';
        }
        return str;
    }

    abp.utils.round = function (num) {
        num = Number(num);
        if (isNaN(num))
            return null;
        return Math.round((num + 0.00001) * 100) / 100;
    };

    abp.utils.roundTo = function (num, decimals) {
        num = Number(num);
        if (isNaN(num))
            return null;
        if (decimals < 0 || decimals > 4) {
            return null;
        }
        var pow = Math.pow(10, decimals);
        return Math.round((num + 0.00001) * pow) / pow;
    };

})(jQuery);