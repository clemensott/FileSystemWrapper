import React, { useEffect, useRef } from 'react';
import { Route, Switch } from 'react-router-dom';
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
import LoadingModal from './components/Modals/LoadingModal';
import ErrorModal from './components/Modals/ErrorModal';
import store from './Helpers/store'
import { getAllRefs } from './Helpers/storeExtensions';
import API from './Helpers/API';
import './App.css';

export default function () {
    useEffect(() => {
        (async () => {
            let isLoggedIn = false;

            try {
                const { ok } = await API.isAuthorized()
                isLoggedIn = ok;
            } catch (err) {
                console.error('isAuthorized Error:', err);
            }

            store.set('isLoggedIn', isLoggedIn)
        })();
    }, []);

    const allRefs = getAllRefs();
    allRefs.deleteShareItem = useRef();
    allRefs.deleteFSItemModal = useRef();
    allRefs.loadingModal = useRef();
    allRefs.errorModal = useRef();

    return (
        <div>
            <NavMenu />
            <Container>
                <Switch>
                    <Route path='/login' component={Login} />
                    <Route path='/logout' component={Logout} />
                    <Route exact path='/file/view' component={FilePage} />
                    <Route exact path='/share' component={SharesPage} />
                    <Route exact path='/share/file/add' component={AddShareFileSystemItemPage} />
                    <Route exact path='/share/folder/add' component={AddShareFileSystemItemPage} />
                    <Route path='/share/file/edit/:id' component={EditShareFileSystemItemPage} />
                    <Route path='/share/folder/edit/:id' component={EditShareFileSystemItemPage} />
                    <Route exact path='/' component={Home} />
                    <Route path='/api/' render={() => (
                        <h3>Please reload page without cache (Ctrl + F5)</h3>
                    )} />
                </Switch>
            </Container>
            <DeleteShareItemModal ref={allRefs.deleteShareItem} />
            <DeleteFileSystemItemModal ref={allRefs.deleteFSItemModal} />
            <LoadingModal ref={allRefs.loadingModal} />
            <ErrorModal ref={allRefs.errorModal} />
        </div>
    );
}
