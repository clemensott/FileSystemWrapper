import React from 'react';
import API from './API';
import {useGlobalRefs} from '../contexts/GlobalRefsContext';

export default function () {
    const {deleteShareItem, showLoadingModal, closeLoadingModal, showErrorModal} = useGlobalRefs();

    return async (item, callback = null) => {
        const deleteItem = await deleteShareItem.current.show(item);
        if (!deleteItem) return;

        try {
            showLoadingModal();
            const response = await API.deleteShareItem(item.id, item.isFile);
            closeLoadingModal();

            if (response.ok) await callback && callback();
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

    };
}