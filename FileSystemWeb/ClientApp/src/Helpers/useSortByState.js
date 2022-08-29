import { useState } from 'react';
import { fileSystemItemSortDirection, fileSystemItemSortType } from '../constants';

function isValidSortBy(sortBy) {
    return sortBy &&
        Object.values(fileSystemItemSortType).includes(sortBy.type) &&
        Object.values(fileSystemItemSortDirection).includes(sortBy.direction);
}

export default function useSortByState(id) {
    const storageKey = `sort-by-state-${id}`;
    const [sortBy, setSortBy] = useState(() => {
        const initValue = JSON.parse(localStorage.getItem(storageKey));
        return isValidSortBy(initValue) ? initValue : ({
            type: fileSystemItemSortType.NAME,
            direction: fileSystemItemSortDirection.ASC,
        });
    });

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
