import {useEffect, useState} from 'react';
import {fileSystemItemSortDirection, fileSystemItemSortType} from '../constants';

function getDefaultSortBy() {
    return {
        type: fileSystemItemSortType.NAME,
        direction: fileSystemItemSortDirection.ASC,
    };
}

function isValidSortBy(sortBy) {
    return sortBy &&
        Object.values(fileSystemItemSortType).includes(sortBy.type) &&
        Object.values(fileSystemItemSortDirection).includes(sortBy.direction);
}

export default function useSortByState(id) {
    const storageKey = `sort-by-state-${id}`;
    const [sortBy, setSortBy] = useState(getDefaultSortBy());

    useEffect(() => {
        const initValue = JSON.parse(localStorage.getItem(storageKey));
        setSortBy(isValidSortBy(initValue) ? initValue : getDefaultSortBy());
    }, [storageKey]);

    return [
        sortBy,
        value => {
            if (isValidSortBy(value)) {
                setSortBy(value);
                localStorage.setItem(storageKey, JSON.stringify(value));
            }
        },
    ];
}
