import React from 'react';
import API from './API';
import sleep from './sleep';
import {useGlobalRefs} from '../contexts/GlobalRefsContext';

export default function () {
    const {deleteFSItemModal, showLoadingModal, closeLoadingModal, showErrorModal} = useGlobalRefs();
    
    return async (item, callback = null) => {
        const deleteItem = await deleteFSItemModal.current.show(item);
        if (!deleteItem) return;

        try {
            const promise = API.deleteFileSystemItem(item.path, item.isFile);
            await sleep(200);
            showLoadingModal();
            const response = await promise;
            closeLoadingModal();

            if (response.ok) callback && await callback();
            else {
                const text = await response.text();
                await showErrorModal(
                    <div>
                        Status: {response.status}
                        <br/>
                        {text}
                    </div>
                );
            }
        } catch (e) {
            closeLoadingModal();
            await showErrorModal(e.message);
        }
    }
}