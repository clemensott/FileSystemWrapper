import React, {useEffect} from 'react';
import {useNavigate} from 'react-router-dom';
import Loading from './Loading/Loading';
import API from '../Helpers/API';
import {useAuth} from '../contexts/AuthContext';
import {useGlobalRefs} from '../contexts/GlobalRefsContext';

export default function () {
    const navigate = useNavigate();
    const {showErrorModal} = useGlobalRefs();
    const {reloadUser} = useAuth();
    useEffect(() => {
        (async () => {
            try {
                const response = await API.logout();
                if (!response.ok) {
                    await showErrorModal(await response.text());
                }
            } catch (e) {
                await showErrorModal(e.message);
            }

            navigate('/login');
            await reloadUser();
        })();
    }, []);

    return (
        <div className="center">
            <Loading/>
        </div>
    );
}