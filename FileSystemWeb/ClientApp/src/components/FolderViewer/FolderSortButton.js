import React, { useState } from 'react';
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem, } from 'reactstrap';
import { fileSystemItemSortDirection, fileSystemItemSortType } from '../../constants';
import './FolderSortButton.css';

export default function FolderSortButton({ sortBy, onSortByChange }) {
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const toggleDropdown = () => setDropdownOpen(prevState => !prevState);

    const isSelected = value => value.type === sortBy.type && value.direction === sortBy.direction;
    const renderDropdownItem = ({ text, value }) => (
        <DropdownItem
            toggle
            active={isSelected(value)}
            onClick={() => typeof onSortByChange === 'function' && onSortByChange(value)}
        >
            {text}
        </DropdownItem>
    );

    return (
        <Dropdown className="folder-sort-button-container" isOpen={dropdownOpen} toggle={toggleDropdown}>
            <DropdownToggle>
                <i className="fas fa-sort" />
            </DropdownToggle>
            <DropdownMenu end>
                {
                    renderDropdownItem({
                        text: 'Name ASC',
                        value: { type: fileSystemItemSortType.NAME, direction: fileSystemItemSortDirection.ASC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Name DESC',
                        value: { type: fileSystemItemSortType.NAME, direction: fileSystemItemSortDirection.DESC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Size ASC',
                        value: { type: fileSystemItemSortType.SIZE, direction: fileSystemItemSortDirection.ASC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Size DESC',
                        value: { type: fileSystemItemSortType.SIZE, direction: fileSystemItemSortDirection.DESC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Creation time ASC',
                        value: { type: fileSystemItemSortType.CREATION_TIME, direction: fileSystemItemSortDirection.ASC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Creation time DESC',
                        value: { type: fileSystemItemSortType.CREATION_TIME, direction: fileSystemItemSortDirection.DESC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Last write time ASC',
                        value: { type: fileSystemItemSortType.LAST_WRITE_TIME, direction: fileSystemItemSortDirection.ASC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Last write time DESC',
                        value: { type: fileSystemItemSortType.LAST_WRITE_TIME, direction: fileSystemItemSortDirection.DESC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Last access time ASC',
                        value: { type: fileSystemItemSortType.LAST_ACCESS_TIME, direction: fileSystemItemSortDirection.ASC }
                    })
                }
                {
                    renderDropdownItem({
                        text: 'Last access time DESC',
                        value: { type: fileSystemItemSortType.LAST_ACCESS_TIME, direction: fileSystemItemSortDirection.DESC }
                    })
                }
            </DropdownMenu>
        </Dropdown>
    );
}
