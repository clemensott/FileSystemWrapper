import React from 'react';
import {Route, Routes} from 'react-router-dom';
import Login from './components/Login';
import NavMenu from './components/NavMenu';
import {Container} from 'reactstrap';
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
import Loading from './components/Loading/Loading';
import UserPage from './components/User/UserPage';
import UsersPage from './components/Users/UsersPage';
import {AuthProvider, useAuth} from './contexts/AuthContext';
import {GlobalRefsProvider, useGlobalRefs} from './contexts/GlobalRefsContext';
import './App.css';

function Main() {
    const {isLoaded} = useAuth();
    const {deleteShareItem, deleteFSItemModal, overrideFileModal, loadingModal, errorModal} = useGlobalRefs();

    return isLoaded ? (
        <div>
            <NavMenu/>
            <Container>
                <Routes>
                    <Route path='/login' element={<Login/>}/>
                    <Route path='/logout' element={<Logout/>}/>
                    <Route path='/file/view' element={<FilePage/>}/>
                    <Route path='/share' element={<SharesPage/>}/>
                    <Route path='/share/file/add' element={<AddShareFileSystemItemPage/>}/>
                    <Route path='/share/folder/add' element={<AddShareFileSystemItemPage/>}/>
                    <Route path='/share/file/edit/:id' element={<EditShareFileSystemItemPage/>}/>
                    <Route path='/share/folder/edit/:id' element={<EditShareFileSystemItemPage/>}/>
                    <Route path='/user' element={<UserPage/>}/>
                    <Route path='/users' element={<UsersPage/>}/>
                    <Route path='/' element={<Home/>}/>
                    <Route path='/api/*' element={
                        <h3>Please reload page without cache (Ctrl + F5)</h3>
                    }/>
                </Routes>
            </Container>
            <DeleteShareItemModal ref={deleteShareItem}/>
            <DeleteFileSystemItemModal ref={deleteFSItemModal}/>
            <OverrideFileModal ref={overrideFileModal}/>
            <LoadingModal ref={loadingModal}/>
            <ErrorModal ref={errorModal}/>
        </div>
    ) : (
        <div className="center-root">
            <Loading/>
        </div>
    );
}

export default function () {
    return (
        <AuthProvider>
            <GlobalRefsProvider>
                <Main/>
            </GlobalRefsProvider>
        </AuthProvider>
    );
}
