import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  IconButton,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { createSurvey } from '../api';

export default function CreateSurveyForm({ onCreated, onCancel }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [questions, setQuestions] = useState(['']);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleQuestionChange = (index, value) => {
    setQuestions((current) => current.map((q, i) => (i === index ? value : q)));
  };

  const handleAddQuestion = () => {
    setQuestions((current) => [...current, '']);
  };

  const handleRemoveQuestion = (index) => {
    setQuestions((current) => current.filter((_, i) => i !== index));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');

    const trimmedQuestions = questions.map((q) => q.trim()).filter(Boolean);

    if (!title.trim()) {
      setError('A survey title is required.');
      return;
    }

    if (trimmedQuestions.length === 0) {
      setError('Add at least one question.');
      return;
    }

    setSubmitting(true);

    try {
      const payload = {
        title: title.trim(),
        description: description.trim() || null,
        questions: trimmedQuestions.map((prompt) => ({ prompt })),
      };

      const survey = await createSurvey(payload);
      onCreated?.(survey);
      setTitle('');
      setDescription('');
      setQuestions(['']);
    } catch (err) {
      setError(err.message ?? 'Failed to create survey.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Card elevation={3} component="section">
      <CardContent>
        <Box component="form" onSubmit={handleSubmit} noValidate>
          <Stack spacing={2}>
            <Typography component="h2" variant="h5" fontWeight={600}>
              Create Survey
            </Typography>

            {error && <Alert severity="error">{error}</Alert>}

            <TextField
              label="Title"
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              required
              fullWidth
            />

            <TextField
              label="Description"
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              fullWidth
              multiline
              minRows={3}
            />

            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="subtitle1" fontWeight={600}>
                Questions
              </Typography>
              <Button startIcon={<AddIcon />} onClick={handleAddQuestion} variant="outlined">
                Add question
              </Button>
            </Stack>

            <Stack spacing={1}>
              {questions.map((question, index) => (
                <Stack direction="row" alignItems="center" spacing={1} key={index}>
                  <TextField
                    label={`Question ${index + 1}`}
                    value={question}
                    onChange={(event) => handleQuestionChange(index, event.target.value)}
                    required
                    fullWidth
                  />
                  {questions.length > 1 && (
                    <IconButton
                      aria-label={`Remove question ${index + 1}`}
                      onClick={() => handleRemoveQuestion(index)}
                    >
                      <DeleteIcon />
                    </IconButton>
                  )}
                </Stack>
              ))}
            </Stack>

            <Stack direction="row" spacing={2} justifyContent="flex-start">
              <Button type="submit" variant="contained" disabled={submitting}>
                {submitting ? 'Creating…' : 'Create survey'}
              </Button>
              {onCancel && (
                <Button type="button" variant="outlined" onClick={onCancel} disabled={submitting}>
                  Cancel
                </Button>
              )}
            </Stack>
          </Stack>
        </Box>
      </CardContent>
    </Card>
  );
}
