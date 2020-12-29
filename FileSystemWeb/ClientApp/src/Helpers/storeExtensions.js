import store from './store';

export function getAllRefs() {
    let allRefs = store.get('refs');
    if (!allRefs) store.set('refs', allRefs = {});

    return allRefs;
}

export function showLoadingModal() {
    getAllRefs().loadingModal.current.show();
}

export function closeLoadingModal() {
    const modal = getAllRefs().loadingModal.current;
    modal && modal.close();
}

export function showErrorModal(error) {
    return getAllRefs().errorModal.current.show(error);
}