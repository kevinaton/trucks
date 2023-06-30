import { useEffect } from 'react';

export const useDebounceEffect = (fn, waitTime, deps) => {
    useEffect(() => {
        const timeout = setTimeout(() => {
            fn();
        }, waitTime);

        return () => {
            clearTimeout(timeout);
        };
    }, deps);
};