import React, {useState, useEffect} from 'react';
import {Dropdown, DropdownToggle, DropdownMenu, DropdownItem,} from 'reactstrap';
import DropdownLinkItem from './DropdownLinkItem';
import store from '../../Helpers/store';

export default function ({folder, title, onDelete}) {
    const shareFolderLink = folder.sharedId ?
        `/share/folder/edit/${encodeURIComponent(folder.path)}` :
        `/share/folder/add?path=${encodeURIComponent(folder.path)}`;

    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoggedInCallbackId, setIsLoggedInCallbackId] = useState(null);

    useEffect(() => {
        setIsLoggedInCallbackId(store.addCallback('isLoggedIn', value => setIsLoggedIn(value)));
        setIsLoggedIn(store.get('isLoggedIn'));
    }, []);
    useEffect(() => {
        return () => isLoggedInCallbackId && store.removeCallback('isLoggedIn', isLoggedInCallbackId)
    }, [isLoggedInCallbackId]);

    const [dropdownOpen, setDropdownOpen] = useState(false);
    const toggleDropdown = () => setDropdownOpen(prevState => !prevState);

    return (
        <Dropdown isOpen={dropdownOpen} toggle={toggleDropdown}>
            <DropdownToggle caret>
                {title || ''}
            </DropdownToggle>
            <DropdownMenu end>
                {isLoggedIn ? (
                    <DropdownLinkItem disabled={!folder.permission.info} to={shareFolderLink}>
                        <i className="me-2 fas fa-share"/>
                        Share
                    </DropdownLinkItem>
                ) : null}
                {onDelete ? (
                    <DropdownItem disabled={!folder.permission.write || !folder.deletable}
                                  onClick={() => onDelete(folder)}>
                        <i className="me-2 fas fa-trash"/>
                        Delete
                    </DropdownItem>
                ) : null}
            </DropdownMenu>
        </Dropdown>
    );
}