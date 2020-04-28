import React from 'react';
import { useLocation, useParams } from 'react-router';
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export default function () {
    const { path } = useParams();
    const normal = path.split(/[\/\\|]/).filter(Boolean).join('\\');
    console.log('location:', useParams());

    return (
        <div>
            <h1>Test</h1>
            <h6>{path}</h6>
            <h6>{normal}</h6>
            <NavLink tag={Link} to="/test">Recursion</NavLink>
        </div>
    );
}