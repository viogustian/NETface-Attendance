const ADMIN_TOKEN_KEY = 'adminToken';

export const setToken = (token) => {
  sessionStorage.setItem(ADMIN_TOKEN_KEY, token);
};

export const getToken = () => {
  return sessionStorage.getItem(ADMIN_TOKEN_KEY);
};

export const clearToken = () => {
  sessionStorage.removeItem(ADMIN_TOKEN_KEY);
};

export const isAuthenticated = () => {
  return Boolean(getToken());
};

export const requiresPasswordChange = () => {
  const token = getToken();
  if (!token) return false;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.requiresPasswordChange === 'true' || payload.requiresPasswordChange === true;
  } catch {
    return false;
  }
};
