(function ($) {
    abp = abp || {};

    abp.appPath = abp.appPath || '/';

    /* LOGGING ***************************************************/
    //Implements Logging API that provides secure & controlled usage of console.log

    abp.log = abp.log || {};

    abp.log.levels = {
        DEBUG: 1,
        INFO: 2,
        WARN: 3,
        ERROR: 4,
        FATAL: 5
    };

    abp.log.level = abp.log.levels.DEBUG;

    abp.log.log = function (logObject, logLevel) {
        if (!window.console || !window.console.log) {
            return;
        }

        if (logLevel != undefined && logLevel < abp.log.level) {
            return;
        }

        console.log(logObject);
    };

    abp.log.debug = function (logObject) {
        abp.log.log("DEBUG: ", abp.log.levels.DEBUG);
        abp.log.log(logObject, abp.log.levels.DEBUG);
    };

    abp.log.info = function (logObject) {
        abp.log.log("INFO: ", abp.log.levels.INFO);
        abp.log.log(logObject, abp.log.levels.INFO);
    };

    abp.log.warn = function (logObject) {
        abp.log.log("WARN: ", abp.log.levels.WARN);
        abp.log.log(logObject, abp.log.levels.WARN);
    };

    abp.log.error = function (logObject) {
        abp.log.log("ERROR: ", abp.log.levels.ERROR);
        abp.log.log(logObject, abp.log.levels.ERROR);
    };

    abp.log.fatal = function (logObject) {
        abp.log.log("FATAL: ", abp.log.levels.FATAL);
        abp.log.log(logObject, abp.log.levels.FATAL);
    };

    /* NOTIFICATION *********************************************/
    //Defines Notification API, not implements it

    abp.notify = abp.notify || {};

    abp.notify.success = function (message, title, options) {
        abp.log.warn('abp.notify.success is not implemented!');
    };

    abp.notify.info = function (message, title, options) {
        abp.log.warn('abp.notify.info is not implemented!');
    };

    abp.notify.warn = function (message, title, options) {
        abp.log.warn('abp.notify.warn is not implemented!');
    };

    abp.notify.error = function (message, title, options) {
        abp.log.warn('abp.notify.error is not implemented!');
    };

    /* MESSAGE **************************************************/
    //Defines Message API, not implements it

    abp.message = abp.message || {};

    var showMessage = function (message, title) {
        alert((title || '') + ' ' + message);

        if (!$) {
            abp.log.warn('abp.message can not return promise since jQuery is not defined!');
            return null;
        }

        return $.Deferred(function ($dfd) {
            $dfd.resolve();
        });
    };

    abp.message.info = function (message, title) {
        abp.log.warn('abp.message.info is not implemented!');
        return showMessage(message, title);
    };

    abp.message.success = function (message, title) {
        abp.log.warn('abp.message.success is not implemented!');
        return showMessage(message, title);
    };

    abp.message.warn = function (message, title) {
        abp.log.warn('abp.message.warn is not implemented!');
        return showMessage(message, title);
    };

    abp.message.error = function (message, title) {
        abp.log.warn('abp.message.error is not implemented!');
        return showMessage(message, title);
    };

    abp.message.confirm = function (message, titleOrCallback, callback) {
        abp.log.warn('abp.message.confirm is not implemented!');

        if (titleOrCallback && !(typeof titleOrCallback == 'string')) {
            callback = titleOrCallback;
        }

        var result = confirm(message);
        callback && callback(result);

        if (!$) {
            abp.log.warn('abp.message can not return promise since jQuery is not defined!');
            return null;
        }

        return $.Deferred(($dfd) => {
            $dfd.resolve(result);
        });
    };

    /* UI *******************************************************/

    abp.ui = abp.ui || {};

    /* UI BLOCK */
    //Defines UI Block API, not implements it

    abp.ui.block = function (elm) {
        abp.log.warn('abp.ui.block is not implemented!');
    };

    abp.ui.unblock = function (elm) {
        abp.log.warn('abp.ui.unblock is not implemented!');
    };

    /* UI BUSY */
    //Defines UI Busy API, not implements it

    abp.ui.setBusy = function (elm, optionsOrPromise) {
        abp.log.warn('abp.ui.setBusy is not implemented!');
    };

    abp.ui.clearBusy = function (elm) {
        abp.log.warn('abp.ui.clearBusy is not implemented!');
    };

    /* SIMPLE EVENT BUS *****************************************/

    abp.event = (function () {

        var _callbacks = {};

        var on = function (eventName, callback) {
            if (!_callbacks[eventName]) {
                _callbacks[eventName] = [];
            }

            _callbacks[eventName].push(callback);
        };

        var off = function (eventName, callback) {
            var callbacks = _callbacks[eventName];
            if (!callbacks) {
                return;
            }

            var index = -1;
            for (var i = 0; i < callbacks.length; i++) {
                if (callbacks[i] === callback) {
                    index = i;
                    break;
                }
            }

            if (index < 0) {
                return;
            }

            _callbacks[eventName].splice(index, 1);
        };

        var trigger = function (eventName) {
            var callbacks = _callbacks[eventName];
            if (!callbacks || !callbacks.length) {
                return;
            }

            var args = Array.prototype.slice.call(arguments, 1);
            for (var i = 0; i < callbacks.length; i++) {
                callbacks[i].apply(this, args);
            }
        };

        // Public interface ///////////////////////////////////////////////////

        return {
            on: on,
            off: off,
            trigger: trigger
        };
    })();


    /* UTILS ***************************************************/

    abp.utils = abp.utils || {};

    /* Find and replaces a string (search) to another string (replacement) in
    *  given string (str).
    *  Example:
    *  abp.utils.replaceAll('This is a test string', 'is', 'X') = 'ThX X a test string'
    ************************************************************/
    abp.utils.replaceAll = function (str, search, replacement) {
        var fix = search.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
        return str.replace(new RegExp(fix, 'g'), replacement);
    };

    /* Formats a string just like string.format in C#.
    *  Example:
    *  abp.utils.formatString('Hello {0}','Tuana') = 'Hello Tuana'
    ************************************************************/
    abp.utils.formatString = function () {
        if (arguments.length < 1) {
            return null;
        }

        var str = arguments[0];

        for (var i = 1; i < arguments.length; i++) {
            var placeHolder = '{' + (i - 1) + '}';
            str = abp.utils.replaceAll(str, placeHolder, arguments[i]);
        }

        return str;
    };

    abp.utils.toPascalCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toUpperCase();
        }

        return str.charAt(0).toUpperCase() + str.substr(1);
    }

    abp.utils.toCamelCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toLowerCase();
        }

        return str.charAt(0).toLowerCase() + str.substr(1);
    }

    abp.utils.truncateString = function (str, maxLength) {
        if (!str || !str.length || str.length <= maxLength) {
            return str;
        }

        return str.substr(0, maxLength);
    };

    abp.utils.truncateStringWithPostfix = function (str, maxLength, postfix) {
        postfix = postfix || '...';

        if (!str || !str.length || str.length <= maxLength) {
            return str;
        }

        if (maxLength <= postfix.length) {
            return postfix.substr(0, maxLength);
        }

        return str.substr(0, maxLength - postfix.length) + postfix;
    };

    abp.utils.isFunction = function (obj) {
        if ($) {
            //Prefer to use jQuery if possible
            return $.isFunction(obj);
        }

        //alternative for $.isFunction
        return !!(obj && obj.constructor && obj.call && obj.apply);
    };

})(jQuery);


//todo
//abp.ui.setBusy
//abp.ui.clearBusy
//abp.message.confirm
//abp.message.error