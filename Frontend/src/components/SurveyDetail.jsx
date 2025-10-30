import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import { fetchSurveyDetails } from '../api';
import InvitationForm from './InvitationForm';

export default function SurveyDetail({ surveyId, onDataChanged }) {
  const [survey, setSurvey] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError('');

      try {
        const data = await fetchSurveyDetails(surveyId);
        if (!cancelled) {
          setSurvey(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message ?? 'Failed to load survey.');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [surveyId]);

  const handleInvitationsSent = async () => {
    onDataChanged?.();
    try {
      const data = await fetchSurveyDetails(surveyId);
      setSurvey(data);
    } catch {
      // keep current view if refresh fails
    }
  };

  if (loading) {
    return (
      <Stack alignItems="center" justifyContent="center" minHeight={240}>
        <CircularProgress />
      </Stack>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (!survey) {
    return <Alert severity="error">Survey not found.</Alert>;
  }

  const responseUrlHint = `${window.location.origin}/respond/your-unique-token`;

  return (
    <Grid container spacing={2}>
      <Grid item xs={12} md={8}>
        <Card elevation={3} component="article">
          <CardContent>
            <Stack spacing={3}>
              <Box>
                <Typography component="h2" variant="h5" fontWeight={600} gutterBottom>
                  {survey.title}
                </Typography>
                {survey.description && (
                  <Typography variant="body1" color="text.secondary">
                    {survey.description}
                  </Typography>
                )}
              </Box>

              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <StatCard label="Questions" value={survey.questions.length} />
                <StatCard label="Invitations sent" value={survey.invitationCount} />
                <StatCard label="Responses" value={survey.responseCount} />
              </Stack>

              <Box>
                <Typography component="h3" variant="subtitle1" fontWeight={600} gutterBottom>
                  Questions
                </Typography>
                <Stack component="ol" spacing={1} sx={{ pl: 2, m: 0 }}>
                  {survey.questions.map((question) => (
                    <Typography component="li" key={question.id} variant="body1">
                      {question.prompt}
                    </Typography>
                  ))}
                </Stack>
              </Box>

              <Box>
                <Typography component="h3" variant="subtitle1" fontWeight={600} gutterBottom>
                  Sharing instructions
                </Typography>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Each invitation email includes a unique, single-use link. Links follow this format:
                </Typography>
                <Box
                  component="code"
                  sx={{
                    display: 'inline-block',
                    px: 1.5,
                    py: 1,
                    borderRadius: 1,
                    bgcolor: 'grey.100',
                    fontFamily: 'monospace',
                  }}
                >
                  {responseUrlHint}
                </Box>
                <Typography variant="body2" color="text.secondary" mt={1}>
                  You can manually share links if needed—responses remain anonymous and each token can only
                  be used once.
                </Typography>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      </Grid>

      <Grid item xs={12} md={4}>
        <InvitationForm surveyId={surveyId} onSent={handleInvitationsSent} />
      </Grid>
    </Grid>
  );
}

function StatCard({ label, value }) {
  return (
    <Card variant="outlined" sx={{ flex: 1 }}>
      <CardContent>
        <Typography variant="h4" fontWeight={700}>
          {value}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      </CardContent>
    </Card>
  );
}
