const API_BASE = '/api';

async function request(path, options = {}, includeAuth = true) {
  const headers = { 'Content-Type': 'application/json', ...(options.headers ?? {}) };

  if (includeAuth) {
    const token = localStorage.getItem('authToken');
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('authTokenExpiry');
      localStorage.removeItem('authUserEmail');
      window.dispatchEvent(new Event('app:unauthorized'));
    }

    let message = `Request failed with status ${response.status}`;

    try {
      const data = await response.json();
      if (data?.message) {
        message = data.message;
      }
    } catch {
      // ignore parse failures
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

export function fetchSurveys() {
  return request('/surveys');
}

export function createSurvey(payload) {
  return request('/surveys', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function fetchSurveyDetails(surveyId) {
  return request(`/surveys/${surveyId}`);
}

export function sendInvitations(surveyId, payload) {
  return request(`/surveys/${surveyId}/invitations`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function fetchSurveyByToken(token) {
  return request(`/responses/${token}`);
}

export function submitSurveyResponse(payload) {
  return request('/responses', {
    method: 'POST',
    body: JSON.stringify(payload),
  }, false);
}

export function login(payload) {
  return request('/auth/login', {
    method: 'POST',
    body: JSON.stringify(payload),
  }, false);
}
