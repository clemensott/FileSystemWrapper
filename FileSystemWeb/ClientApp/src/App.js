import React, {useEffect, useRef} from 'react';
import {Route, Switch} from 'react-router-dom';
import {Login} from './components/Login';
import {NavMenu} from './components/NavMenu';
import {Container} from 'reactstrap';
import Logout from './components/Logout';
import FilePage from "./components/FilePage";
import AddShareFileSystemItemPage from './components/Share/AddShareFileSystemItemPage';
import EditShareFileSystemItemPage from './components/Share/EditShareFileSystemItemPage';
import Home from './components/Home';
import DeleteShareItemModal from "./components/Modals/DeleteShareItemModal";
import DeleteFileSystemItemModal from './components/Modals/DeleteFileSystemItemModal';
import LoadingModal from './components/Modals/LoadingModal';
import ErrorModal from './components/Modals/ErrorModal';
import store from './Helpers/store'
import {getCookieValue} from './Helpers/cookies';
import {encodeBase64UnicodeCustom} from './Helpers/Path';
import './App.css';

export default function () {
    const isLoggedIn = !!getCookieValue('fs_login');
    useEffect(
        () => store.set('isLoggedIn', isLoggedIn),
        [isLoggedIn]
    );

    let allRefs = store.get('refs');
    if (!store.get('refs')) store.set('refs', allRefs = {});

    allRefs.deleteShareItem = useRef();
    allRefs.deleteFSItemModal = useRef();
    allRefs.loadingModal = useRef();
    allRefs.errorModal = useRef();

    return (
        <div>
            <NavMenu/>
            <Container>
                <Switch>
                    <Route path='/login' component={Login}/>
                    <Route path='/logout' component={Logout}/>
                    <Route exact path='/file/view' component={FilePage}/>
                    <Route path='/share/file/add' component={AddShareFileSystemItemPage}/>
                    <Route path='/share/folder/add' component={AddShareFileSystemItemPage}/>
                    <Route path='/share/file/edit/:id' component={EditShareFileSystemItemPage}/>
                    <Route path='/share/folder/edit/:id' component={EditShareFileSystemItemPage}/>
                    <Route exact path='/' exec component={Home}/>
                </Switch>
            </Container>
            <DeleteShareItemModal ref={allRefs.deleteShareItem}/>
            <DeleteFileSystemItemModal ref={allRefs.deleteFSItemModal}/>
            <LoadingModal ref={allRefs.loadingModal}/>
            <ErrorModal ref={allRefs.errorModal}/>
        </div>
    );
}
