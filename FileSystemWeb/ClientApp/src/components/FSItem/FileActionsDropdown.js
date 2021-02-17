import React, {useState, useEffect} from 'react';
import {Dropdown, DropdownToggle, DropdownMenu, DropdownItem,} from 'reactstrap';
import DropdownLinkItem from './DropdownLinkItem';
import {encodeBase64UnicodeCustom, getFileType} from '../../Helpers/Path';
import store from '../../Helpers/store';
import formatFileSize from "../../Helpers/formatFileSize";

export default function ({file, title, hideOpenFileLink, onDelete}) {
    const fileOpenFileLink = `/file/view?path=${encodeURIComponent(file.path)}`;
    const fileOpenContentLink = `/api/files/${encodeBase64UnicodeCustom(file.path)}`;
    const fileDownloadLink = `/api/files/${encodeBase64UnicodeCustom(file.path)}/download`;
    const fileShareFileLink = file.sharedId ?
        `/share/file/edit/${encodeURIComponent(file.path)}` :
        `/share/file/add?path=${encodeURIComponent(file.path)}`;
    const isSupportedFile = ['image', 'audio', 'video', 'text', 'pdf'].includes(getFileType(file.extension));

    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoggedInCallbackId, setIsLoggedInCallbackId] = useState(null);

    useEffect(() => {
        setIsLoggedInCallbackId(store.addCallback('isLoggedIn', value => setIsLoggedIn(value)));
        setIsLoggedIn(store.get('isLoggedIn'));
    }, []);
    useEffect(() => {
        return () => {
            isLoggedInCallbackId && store.removeCallback('isLoggedIn', isLoggedInCallbackId);
        }
    }, [isLoggedInCallbackId]);

    const [dropdownOpen, setDropdownOpen] = useState(false);
    const toggleDropdown = () => setDropdownOpen(prevState => !prevState);

    return (
        <Dropdown isOpen={dropdownOpen} toggle={toggleDropdown}>
            <DropdownToggle caret>
                {title || ''}
            </DropdownToggle>
            <DropdownMenu right>
                {!hideOpenFileLink ? (
                    <DropdownLinkItem disabled={!file.permission.info} to={fileOpenFileLink}
                                      target="_blank" rel="noopener noreferrer">
                        <i className="mr-2 fas fa-external-link-square-alt"/>
                        Open file in new tab
                    </DropdownLinkItem>
                ) : null}
                <DropdownLinkItem disabled={!file.permission.read || !isSupportedFile} to={fileOpenContentLink}
                                  target="_blank" rel="noopener noreferrer">
                    <i className="mr-2 fas fa-external-link-square-alt"/>
                    Open content in new tab
                </DropdownLinkItem>
                <DropdownLinkItem disabled={!file.permission.read} to={fileDownloadLink}
                                  download={file.name} target="_blank" rel="noopener noreferrer">
                    <i className="mr-2 fas fa-download"/>
                    Download {file.size ? `(${formatFileSize(file.size)})` : ''}
                </DropdownLinkItem>
                <DropdownItem divider/>
                {isLoggedIn ? (
                    <DropdownLinkItem disabled={!file.permission.info} to={fileShareFileLink}>
                        <i className="mr-2 fas fa-share"/>
                        Share
                    </DropdownLinkItem>
                ) : null}
                {onDelete ? (
                    <DropdownItem disabled={!file.permission.write}
                                  onClick={() => onDelete(file)}>
                        <i className="mr-2 fas fa-trash"/>
                        Delete
                    </DropdownItem>
                ) : null}
            </DropdownMenu>
        </Dropdown>
    );
}