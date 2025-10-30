import { useEffect, useState } from 'react';
import {
  AppBar,
  Box,
  Button,
  CircularProgress,
  Container,
  IconButton,
  Stack,
  Toolbar,
  Typography,
} from '@mui/material';
import AddCircleIcon from '@mui/icons-material/AddCircle';
import LogoutIcon from '@mui/icons-material/Logout';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchSurveys } from '../api';
import { useAuth } from '../context/AuthContext';
import CreateSurveyForm from '../components/CreateSurveyForm';
import SurveyDetail from '../components/SurveyDetail';
import SurveyList from '../components/SurveyList';

const VIEW = {
  LIST: 'list',
  CREATE: 'create',
  DETAIL: 'detail',
};

export default function DashboardPage() {
  const { email, logout } = useAuth();
  const [view, setView] = useState(VIEW.LIST);
  const [surveys, setSurveys] = useState([]);
  const [selectedSurveyId, setSelectedSurveyId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [refreshToken, setRefreshToken] = useState(0);

  useEffect(() => {
    if (view === VIEW.LIST) {
      loadSurveys();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [view, refreshToken]);

  const loadSurveys = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await fetchSurveys();
      setSurveys(data ?? []);
    } catch (err) {
      setError(err.message ?? 'Failed to load surveys.');
    } finally {
      setLoading(false);
    }
  };

  const handleSurveyCreated = (survey) => {
    setSelectedSurveyId(survey.id);
    setView(VIEW.DETAIL);
    setRefreshToken((value) => value + 1);
  };

  const handleSelectSurvey = (id) => {
    setSelectedSurveyId(id);
    setView(VIEW.DETAIL);
  };

  const handleBackToList = () => {
    setSelectedSurveyId(null);
    setView(VIEW.LIST);
    setRefreshToken((value) => value + 1);
  };

  const handleDataChanged = () => {
    setRefreshToken((value) => value + 1);
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" color="primary" enableColorOnDark>
        <Toolbar sx={{ justifyContent: 'space-between' }}>
          <Stack direction="row" spacing={2} alignItems="center">
            {(view === VIEW.CREATE || view === VIEW.DETAIL) && (
              <IconButton color="inherit" onClick={handleBackToList}>
                <ArrowBackIcon />
              </IconButton>
            )}
            <Typography variant="h6" component="div" fontWeight={600}>
              Survey Management
            </Typography>
          </Stack>
          <Stack direction="row" spacing={2} alignItems="center">
            <Typography variant="body2" sx={{ display: { xs: 'none', sm: 'inline' } }}>
              {email}
            </Typography>
            <Button color="inherit" startIcon={<LogoutIcon />} onClick={logout}>
              Log out
            </Button>
          </Stack>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Stack spacing={3}>
          {view === VIEW.LIST && (
            <Stack direction="row" justifyContent="flex-end">
              <Button
                variant="contained"
                startIcon={<AddCircleIcon />}
                onClick={() => setView(VIEW.CREATE)}
              >
                New survey
              </Button>
            </Stack>
          )}

          {error && view === VIEW.LIST && (
            <Box>
              <Typography color="error">{error}</Typography>
            </Box>
          )}

          {view === VIEW.LIST && (
            loading ? (
              <Stack alignItems="center" justifyContent="center" minHeight={240}>
                <CircularProgress />
              </Stack>
            ) : surveys.length === 0 ? (
              <Box textAlign="center" py={8}>
                <Typography variant="h5" fontWeight={600} gutterBottom>
                  You have not created any surveys yet.
                </Typography>
                <Button variant="contained" onClick={() => setView(VIEW.CREATE)}>
                  Create your first survey
                </Button>
              </Box>
            ) : (
              <SurveyList surveys={surveys} onSelectSurvey={handleSelectSurvey} />
            )
          )}

          {view === VIEW.CREATE && (
            <CreateSurveyForm onCreated={handleSurveyCreated} onCancel={handleBackToList} />
          )}

          {view === VIEW.DETAIL && selectedSurveyId && (
            <SurveyDetail surveyId={selectedSurveyId} onDataChanged={handleDataChanged} />
          )}
        </Stack>
      </Container>
    </Box>
  );
}
