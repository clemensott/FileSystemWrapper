import React, { useEffect, useRef, useState } from 'react';
import { Route, Routes } from 'react-router-dom';
import Login from './components/Login';
import NavMenu from './components/NavMenu';
import { Container } from 'reactstrap';
import Logout from './components/Logout';
import FilePage from './components/FilePage';
import SharesPage from './components/Share/SharesPage';
import AddShareFileSystemItemPage from './components/Share/AddShareFileSystemItemPage';
import EditShareFileSystemItemPage from './components/Share/EditShareFileSystemItemPage';
import Home from './components/Home';
import DeleteShareItemModal from './components/Modals/DeleteShareItemModal';
import DeleteFileSystemItemModal from './components/Modals/DeleteFileSystemItemModal';
import OverrideFileModal from './components/Modals/OverrideFileModal';
import LoadingModal from './components/Modals/LoadingModal';
import ErrorModal from './components/Modals/ErrorModal';
import store from './Helpers/store'
import { getAllRefs } from './Helpers/storeExtensions';
import API from './Helpers/API';
import Loading from './components/Loading/Loading';
import UserPage from './components/User/UserPage';
import UsersPage from './components/Users/UsersPage';
import './App.css';

export default function () {
    const [isLoaded, setLoaded] = useState(false);

    useEffect(() => {
        (async () => {
            let isLoggedIn = false;

            try {
                const { ok, redirected } = await API.isAuthorized();
                isLoggedIn = ok && !redirected;
            } catch (err) {
                console.error('isAuthorized Error:', err);
            }

            store.set('isLoggedIn', isLoggedIn);
            
            let me = null;
            try {
                const res = await API.getMe();
                if (res.ok) {
                    me = await res.json();
                }
            } catch (err) {
                console.error('getMe Error:', err);
            }
            
            store.set('me', me);

            await API.loadConfig();
            setLoaded(true);
        })();
    }, []);

    const allRefs = getAllRefs();
    allRefs.deleteShareItem = useRef();
    allRefs.deleteFSItemModal = useRef();
    allRefs.overrideFileModal = useRef();
    allRefs.loadingModal = useRef();
    allRefs.errorModal = useRef();

    return isLoaded ? (
        <div>
            <NavMenu />
            <Container>
                <Routes>
                    <Route path='/login' element={<Login />} />
                    <Route path='/logout' element={<Logout />} />
                    <Route path='/file/view' element={<FilePage />} />
                    <Route path='/share' element={<SharesPage />} />
                    <Route path='/share/file/add' element={<AddShareFileSystemItemPage />} />
                    <Route path='/share/folder/add' element={<AddShareFileSystemItemPage />} />
                    <Route path='/share/file/edit/:id' element={<EditShareFileSystemItemPage />} />
                    <Route path='/share/folder/edit/:id' element={<EditShareFileSystemItemPage />} />
                    <Route path='/user' element={<UserPage />} />
                    <Route path='/users' element={<UsersPage />} />
                    <Route path='/' element={<Home />} />
                    <Route path='/api/*' element={
                        <h3>Please reload page without cache (Ctrl + F5)</h3>
                    } />
                </Routes>
            </Container>
            <DeleteShareItemModal ref={allRefs.deleteShareItem} />
            <DeleteFileSystemItemModal ref={allRefs.deleteFSItemModal} />
            <OverrideFileModal ref={allRefs.overrideFileModal} />
            <LoadingModal ref={allRefs.loadingModal} />
            <ErrorModal ref={allRefs.errorModal} />
        </div>
    ) : (
            <div className="center-root">
                <Loading />
            </div>
        );
}
