import React from 'react';
import {Link} from 'react-router-dom';
import {DropdownItem} from 'reactstrap';
import './DropdownLinkItem.css';

function clickLink(e) {
    const linkElement = e.target.querySelector('a');
    if (linkElement) linkElement.click();
}

export default function ({to, disabled, children, ...props}) {
    return (
        <DropdownItem disabled={disabled} onClick={clickLink}>
            <Link to={to} className={disabled ? 'text-secondary' : 'text-dark dropdown-link'} {...props}>
                {children}
            </Link>
        </DropdownItem>
    );
}