import { useMemo, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Stack,
  Typography,
} from '@mui/material';

const QUESTIONS = [
  {
    prompt: 'What is the most important rule of anonymous surveys?',
    options: [
      'Store every respondent’s name for auditing',
      'Ensure answers cannot be linked back to a person',
      'Share results with participants immediately',
      'Limit surveys to five questions',
    ],
    correctIndex: 1,
  },
  {
    prompt: 'Which response guarantees everyone answers once?',
    options: [
      'Single-use invitation tokens',
      'Leaving the survey open forever',
      'Using the honor system only',
      'Collecting IP addresses',
    ],
    correctIndex: 0,
  },
  {
    prompt: 'Why are open questions useful?',
    options: [
      'They capture detailed feedback in a respondent’s own words',
      'They reduce the time needed to respond',
      'They guarantee faster analysis',
      'They prevent people from skipping a survey',
    ],
    correctIndex: 0,
  },
];

export default function OfflineGame() {
  const randomisedQuestions = useMemo(
    () => QUESTIONS.map((question, index) => ({ ...question, id: `offline-q-${index}` })),
    []
  );

  const [index, setIndex] = useState(0);
  const [score, setScore] = useState(0);
  const [completed, setCompleted] = useState(false);

  const currentQuestion = randomisedQuestions[index];

  const handleAnswer = (selectedIndex) => {
    if (completed) return;

    if (selectedIndex === currentQuestion.correctIndex) {
      setScore((value) => value + 1);
    }

    const nextIndex = index + 1;
    if (nextIndex >= randomisedQuestions.length) {
      setCompleted(true);
    } else {
      setIndex(nextIndex);
    }
  };

  const resetGame = () => {
    setIndex(0);
    setScore(0);
    setCompleted(false);
  };

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Card elevation={4}>
        <CardContent>
          <Stack spacing={3} alignItems="stretch">
            <Box textAlign="center">
              <Typography variant="h4" fontWeight={700} gutterBottom>
                Offline Survey Quest
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Test your survey superpowers while we wait for the internet to return.
              </Typography>
            </Box>

            <Typography variant="subtitle1" color="text.secondary" textAlign="center">
              Score: {score} / {randomisedQuestions.length}
            </Typography>

            {completed ? (
              <Stack spacing={2} alignItems="center">
                <Typography variant="h5" fontWeight={600} textAlign="center">
                  {score === randomisedQuestions.length
                    ? 'Perfect! You are a survey hero.'
                    : 'Nicely done! Ready for another round?'}
                </Typography>
                <Button variant="contained" onClick={resetGame}>
                  Play again
                </Button>
              </Stack>
            ) : (
              <Stack spacing={2}>
                <Typography variant="h6" fontWeight={600}>
                  {currentQuestion.prompt}
                </Typography>
                <Stack spacing={1}>
                  {currentQuestion.options.map((option, optionIndex) => (
                    <Button
                      key={option}
                      variant="outlined"
                      onClick={() => handleAnswer(optionIndex)}
                    >
                      {option}
                    </Button>
                  ))}
                </Stack>
              </Stack>
            )}

            <Typography variant="body2" color="text.secondary" textAlign="center">
              Tip: Once you reconnect, head back to the dashboard to keep managing surveys.
            </Typography>
          </Stack>
        </CardContent>
      </Card>
    </Container>
  );
}
