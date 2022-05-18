import React from 'react';
import API from './API';
import {closeLoadingModal, getAllRefs, showErrorModal, showLoadingModal} from './storeExtensions';

export default async function (item, callback = null) {
    const allRefs = getAllRefs();
    const deleteItem = await allRefs.deleteShareItem.current.show(item);
    if (!deleteItem) return;

    try {
        showLoadingModal();
        const response = await API.deleteShareItem(item.id);
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
}