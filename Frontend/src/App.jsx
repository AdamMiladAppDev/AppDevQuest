import { useEffect, useMemo, useState } from 'react';
import { BrowserRouter, Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { Alert, Container, CssBaseline, ThemeProvider, createTheme } from '@mui/material';
import { AuthProvider, useAuth } from './context/AuthContext';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import RespondPage from './pages/RespondPage';
import OfflineGame from './pages/OfflineGame';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#2563eb',
    },
    secondary: {
      main: '#9333ea',
    },
    background: {
      default: '#f5f6fb',
    },
  },
  typography: {
    fontFamily: 'Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
  },
});

function PrivateRoute({ children }) {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return children;
}

function AppRoutes() {
  const location = useLocation();
  const [isOnline, setIsOnline] = useState(navigator.onLine);

  useEffect(() => {
    const goOnline = () => setIsOnline(true);
    const goOffline = () => setIsOnline(false);
    window.addEventListener('online', goOnline);
    window.addEventListener('offline', goOffline);
    return () => {
      window.removeEventListener('online', goOnline);
      window.removeEventListener('offline', goOffline);
    };
  }, []);

  const tokenFromPath = useMemo(() => {
    if (location.pathname.startsWith('/respond')) {
      const parts = location.pathname.split('/').filter(Boolean);
      return parts[1] ?? new URLSearchParams(location.search).get('token');
    }
    return null;
  }, [location]);

  if (!isOnline && !location.pathname.startsWith('/respond')) {
    return <OfflineGame />;
  }

  return (
    <Routes>
      <Route
        path="/"
        element={<Navigate to={tokenFromPath ? `/respond/${tokenFromPath}` : '/dashboard'} replace />}
      />
      <Route
        path="/login"
        element={<LoginPage />}
      />
      <Route
        path="/dashboard"
        element={(
          <PrivateRoute>
            <DashboardPage />
          </PrivateRoute>
        )}
      />
      <Route
        path="/respond/:token"
        element={tokenFromPath ? <RespondPage token={tokenFromPath} /> : <MissingToken />}
      />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}

function MissingToken() {
  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Alert severity="error">Missing survey token.</Alert>
    </Container>
  );
}

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
