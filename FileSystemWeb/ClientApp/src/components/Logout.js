﻿import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Loading from './Loading/Loading';
import store from '../Helpers/store';
import { showErrorModal } from '../Helpers/storeExtensions';
import API from '../Helpers/API';

async function logout() {
    try {
        const response = await API.logout();
        if (response.ok) store.set('isLoggedIn', false);
        else {
            await showErrorModal(await response.text());
        }
    } catch (e) {
        await showErrorModal(e.message);
    }
}

export default function () {
    const navigate = useNavigate();
    useEffect(() => {
        logout().then(() => navigate('/login'));
    }, []);

    return (
        <div className="center">
            <Loading />
        </div>
    );
}