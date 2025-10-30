import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { fetchSurveyByToken, submitSurveyResponse } from '../api';

export default function RespondPage({ token }) {
  const [survey, setSurvey] = useState(null);
  const [answers, setAnswers] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [isOnline, setIsOnline] = useState(navigator.onLine);

  useEffect(() => {
    const onlineListener = () => setIsOnline(true);
    const offlineListener = () => setIsOnline(false);

    window.addEventListener('online', onlineListener);
    window.addEventListener('offline', offlineListener);

    return () => {
      window.removeEventListener('online', onlineListener);
      window.removeEventListener('offline', offlineListener);
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError('');

      try {
        const data = await fetchSurveyByToken(token);
        if (!cancelled) {
          setSurvey(data);
          if (data?.questions) {
            const initial = Object.fromEntries(data.questions.map((q) => [q.id, '']));
            setAnswers(initial);
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message ?? 'Unable to load survey.');
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
  }, [token]);

  const unanswered = useMemo(() => {
    if (!survey?.questions) {
      return 0;
    }
    return survey.questions.filter((q) => !answers[q.id]?.trim()).length;
  }, [survey, answers]);

  const handleAnswerChange = (questionId, value) => {
    setAnswers((current) => ({ ...current, [questionId]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');

    if (!isOnline) {
      setError('You need to be online to submit your response.');
      return;
    }

    if (unanswered > 0) {
      setError('Please answer every question before submitting.');
      return;
    }

    setSubmitting(true);

    try {
      const payload = {
        token,
        answers: survey.questions.map((question) => ({
          questionId: question.id,
          answer: answers[question.id].trim(),
        })),
      };

      await submitSurveyResponse(payload);
      setSubmitted(true);
    } catch (err) {
      setError(err.message ?? 'Failed to submit your response.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Stack alignItems="center" spacing={2}>
          <CircularProgress />
          <Typography variant="body1">Loading survey…</Typography>
        </Stack>
      </Container>
    );
  }

  if (error && !survey) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error">{error}</Alert>
      </Container>
    );
  }

  if (!survey) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error">This survey link is invalid or has expired.</Alert>
      </Container>
    );
  }

  if (submitted) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Stack spacing={2} alignItems="center">
            <Typography variant="h4" fontWeight={700} textAlign="center">
              Thank you!
            </Typography>
            <Typography variant="body1" color="text.secondary" textAlign="center">
              Your anonymous response has been recorded.
            </Typography>
          </Stack>
        </Paper>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Paper elevation={4} sx={{ p: { xs: 3, sm: 4 } }} component="form" onSubmit={handleSubmit}>
        <Stack spacing={3}>
          <Stack spacing={1}>
            <Typography variant="h4" fontWeight={700}>
              {survey.title}
            </Typography>
            {survey.description && (
              <Typography variant="body1" color="text.secondary">
                {survey.description}
              </Typography>
            )}
            {survey.expiresAt && (
              <Typography variant="body2" color="text.secondary">
                This survey closes on{' '}
                <strong>{new Date(survey.expiresAt).toLocaleString()}</strong>.
              </Typography>
            )}
            {!isOnline && (
              <Alert severity="warning">
                You are currently offline. Draft your answers and submit once you reconnect to the internet.
              </Alert>
            )}
          </Stack>

          {error && <Alert severity="error">{error}</Alert>}

          <Stack spacing={2}>
            {survey.questions.map((question, index) => (
              <TextField
                key={question.id}
                label={`${index + 1}. ${question.prompt}`}
                value={answers[question.id] ?? ''}
                onChange={(event) => handleAnswerChange(question.id, event.target.value)}
                required
                multiline
                minRows={3}
                fullWidth
              />
            ))}
          </Stack>

          <Button type="submit" variant="contained" disabled={submitting}>
            {submitting ? 'Submitting…' : 'Submit response'}
          </Button>
        </Stack>
      </Paper>
    </Container>
  );
}
