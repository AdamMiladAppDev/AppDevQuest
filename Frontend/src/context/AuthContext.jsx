import { createContext, useContext, useEffect, useMemo, useState } from 'react';

const AuthContext = createContext(null);

function getStoredAuth() {
  const token = localStorage.getItem('authToken');
  const expiresAt = localStorage.getItem('authTokenExpiry');
  const email = localStorage.getItem('authUserEmail');

  if (!token || !expiresAt || Date.now() > Number(expiresAt)) {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authTokenExpiry');
    localStorage.removeItem('authUserEmail');
    return { token: null, expiresAt: null, email: null };
  }

  return { token, expiresAt: Number(expiresAt), email };
}

export function AuthProvider({ children }) {
  const [{ token, expiresAt, email }, setAuthState] = useState(() => getStoredAuth());

  useEffect(() => {
    if (!token || !expiresAt) {
      return;
    }

    const timeout = setTimeout(() => {
      logout();
    }, Math.max(expiresAt - Date.now(), 0));

    return () => clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, expiresAt]);

  useEffect(() => {
    const handleUnauthorized = () => logout();
    window.addEventListener('app:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('app:unauthorized', handleUnauthorized);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const login = ({ token: newToken, expiresAt: expires, email: userEmail }) => {
    const expiryMs = new Date(expires).getTime();
    localStorage.setItem('authToken', newToken);
    localStorage.setItem('authTokenExpiry', expiryMs.toString());
    localStorage.setItem('authUserEmail', userEmail);
    setAuthState({ token: newToken, expiresAt: expiryMs, email: userEmail });
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authTokenExpiry');
    localStorage.removeItem('authUserEmail');
    setAuthState({ token: null, expiresAt: null, email: null });
  };

  const value = useMemo(
    () => ({
      token,
      email,
      isAuthenticated: Boolean(token && expiresAt && Date.now() < expiresAt),
      login,
      logout,
    }),
    [token, expiresAt, email]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
