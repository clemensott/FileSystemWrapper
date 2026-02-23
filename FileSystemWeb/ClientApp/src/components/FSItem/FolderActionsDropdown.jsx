import React, {useState, useEffect} from 'react';
import {Dropdown, DropdownToggle, DropdownMenu, DropdownItem,} from 'reactstrap';
import DropdownLinkItem from './DropdownLinkItem';
import {useAuth} from '../../contexts/AuthContext';

export default function ({folder, title, onDelete}) {
    const {user} = useAuth();

    const shareFolderLink = folder.sharedId ?
        `/share/folder/edit/${encodeURIComponent(folder.path)}` :
        `/share/folder/add?path=${encodeURIComponent(folder.path)}`;

    const [dropdownOpen, setDropdownOpen] = useState(false);
    const toggleDropdown = () => setDropdownOpen(prevState => !prevState);

    return (
        <Dropdown isOpen={dropdownOpen} toggle={toggleDropdown}>
            <DropdownToggle caret>
                {title || ''}
            </DropdownToggle>
            <DropdownMenu end>
                {user ? (
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