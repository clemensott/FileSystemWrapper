import {createContext, useContext, useEffect, useState} from 'react';
import API from '../Helpers/API';

const AuthContext = createContext({
    isLoaded: false,
    user: null,
    reloadUser: async () => null,
});

export function useAuth() {
    return useContext(AuthContext);
}

export function AuthProvider({children}) {
    const [isLoaded, setLoaded] = useState(false);
    const [user, setUser] = useState(null);

    async function reloadUser() {
        try {
            setLoaded(false);
            await API.loadConfig();
            const res = await API.getMe();
            if (res.ok) {
                if (res.redirected) {
                    setUser(null);
                    return null;
                }

                const user = await res.json();
                setUser(user);
                return user;
            }
        } catch (error) {
            console.error('load user error', error);
            return null;
        } finally {
            setLoaded(true);
        }
    }

    useEffect(() => {
        reloadUser();
    }, []);

    return (
        <AuthContext.Provider value={{isLoaded, user, reloadUser}}>
            {children}
        </AuthContext.Provider>
    );
}
