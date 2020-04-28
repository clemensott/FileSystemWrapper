import React from 'react';
import { Redirect } from 'react-router';

export default function () {
    localStorage.removeItem('password');

    return (
        <Redirect to="/" />
    );
}