import {createContext, useContext, useRef} from 'react';

const GlobalRefsContext = createContext({
    deleteShareItem: null,
    deleteFSItemModal: null,
    overrideFileModal: null,
    loadingModal: null,
    errorModal: null,
    showLoadingModal: () => {
    },
    closeLoadingModal: () => {
    },
    showErrorModal: (error) => Promise.resolve(),
});

export function useGlobalRefs() {
    return useContext(GlobalRefsContext);
}

export const GlobalRefsProvider = ({children}) => {
    const deleteShareItem = useRef();
    const deleteFSItemModal = useRef();
    const overrideFileModal = useRef();
    const loadingModal = useRef();
    const errorModal = useRef();

    return (
        <GlobalRefsContext.Provider value={{
            deleteShareItem,
            deleteFSItemModal,
            overrideFileModal,
            loadingModal,
            errorModal,
            showLoadingModal: () => loadingModal.current.show(),
            closeLoadingModal: () => loadingModal.current.close(),
            showErrorModal: (error) => errorModal.current.show(error),
        }}>
            {children}
        </GlobalRefsContext.Provider>
    )
}
