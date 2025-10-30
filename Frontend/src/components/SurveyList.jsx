import {
  Box,
  Button,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';

export default function SurveyList({ surveys, onSelectSurvey }) {
  return (
    <Card elevation={3} component="section">
      <CardContent>
        <Typography component="h2" variant="h5" fontWeight={600} gutterBottom>
          Surveys
        </Typography>
        <Box sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Title</TableCell>
                <TableCell>Created</TableCell>
                <TableCell align="right">Questions</TableCell>
                <TableCell align="right">Invitations</TableCell>
                <TableCell align="right">Responses</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {surveys.map((survey) => (
                <TableRow hover key={survey.id}>
                  <TableCell>
                    <Typography fontWeight={600}>{survey.title}</Typography>
                    {survey.description && (
                      <Typography variant="body2" color="text.secondary">
                        {survey.description}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>{new Date(survey.createdAt).toLocaleString()}</TableCell>
                  <TableCell align="right">{survey.questionCount}</TableCell>
                  <TableCell align="right">{survey.invitationCount}</TableCell>
                  <TableCell align="right">{survey.responseCount}</TableCell>
                  <TableCell align="right">
                    <Button size="small" onClick={() => onSelectSurvey(survey.id)}>
                      View
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Box>
      </CardContent>
    </Card>
  );
}
