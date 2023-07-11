import localforage from 'localforage';

const setItem = (key, value) => {
    if (!localforage) {
        return;
    }

    localforage.setItem(key, value);
};

const getItem = (key, callback) => {
    if (!localforage || !callback) {
        return;
    }

    localforage.getItem(key)
        .then(function (value) {
            callback(value);
        });
};

const removeItem = (key) => {
    if (!localforage) {
        return;
    }

    localforage.removeItem(key);
};

const clear = (callback) => {
    if (!localforage) {
        return;
    }

    localforage.clear()
        .then(function () {
            if (callback) {
                callback();
            }
        });
};

export const appLocalStorage = {
    setItem,
    getItem,
    removeItem,
    clear
};