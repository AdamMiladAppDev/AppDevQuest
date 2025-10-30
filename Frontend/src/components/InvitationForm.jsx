import { useState } from 'react';
import {
  Alert,
  Button,
  Card,
  CardContent,
  Link,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { sendInvitations } from '../api';

export default function InvitationForm({ surveyId, onSent }) {
  const [emails, setEmails] = useState('');
  const [expiresAt, setExpiresAt] = useState('');
  const [sending, setSending] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [previews, setPreviews] = useState([]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setMessage('');
    setError('');
    setPreviews([]);

    const emailList = emails
      .split(/[\n,;]/)
      .map((email) => email.trim())
      .filter(Boolean);

    if (emailList.length === 0) {
      setError('Enter at least one recipient email.');
      return;
    }

    setSending(true);

    try {
      const result = await sendInvitations(surveyId, {
        emails: emailList,
        expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      });

      setEmails('');
      setMessage('Invitations sent successfully.');
      setPreviews(result?.previews ?? []);
      onSent?.();
    } catch (err) {
      setError(err.message ?? 'Failed to send invitations.');
      setPreviews([]);
    } finally {
      setSending(false);
    }
  };

  return (
    <Card elevation={3} component="section">
      <CardContent>
        <Typography component="h3" variant="h6" fontWeight={600} gutterBottom>
          Send survey invitations
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          Provide one email per line (or separate with commas). Each recipient receives a unique, single-use link.
        </Typography>

        <Stack component="form" onSubmit={handleSubmit} spacing={2} mt={2}>
          {message && <Alert severity="success">{message}</Alert>}
          {error && <Alert severity="error">{error}</Alert>}
          {previews.length > 0 && (
            <Alert severity="info">
              <Typography fontWeight={600}>Test invitation links</Typography>
              <Typography variant="body2" color="text.secondary">
                Available in development so you can open the single-use links without sending real emails.
              </Typography>
              <Stack spacing={0.5} mt={1}>
                {previews.map((preview) => (
                  <Typography key={preview.link} variant="body2">
                    {preview.email}:{' '}
                    <Link
                      href={preview.link}
                      underline="hover"
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {preview.link}
                    </Link>
                  </Typography>
                ))}
              </Stack>
            </Alert>
          )}

          <TextField
            label="Recipient emails"
            value={emails}
            onChange={(event) => setEmails(event.target.value)}
            minRows={4}
            multiline
            placeholder={"alice@example.com\nbob@example.com"}
            fullWidth
          />

          <TextField
            label="Link expiry (optional)"
            type="datetime-local"
            value={expiresAt}
            onChange={(event) => setExpiresAt(event.target.value)}
            InputLabelProps={{ shrink: true }}
          />

          <Button type="submit" variant="contained" disabled={sending}>
            {sending ? 'Sending…' : 'Send invitations'}
          </Button>
        </Stack>
      </CardContent>
    </Card>
  );
}
