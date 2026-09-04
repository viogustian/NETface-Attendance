import { describe, it, expect, beforeEach } from 'vitest';
import { setToken, getToken, clearToken, isAuthenticated } from './auth';

describe('auth utilities', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  it('sets and gets admin token in sessionStorage', () => {
    expect(getToken()).toBeNull();
    setToken('sample-jwt-token');
    expect(sessionStorage.getItem('adminToken')).toBe('sample-jwt-token');
    expect(getToken()).toBe('sample-jwt-token');
  });

  it('clears admin token from sessionStorage', () => {
    setToken('sample-jwt-token');
    expect(getToken()).toBe('sample-jwt-token');
    clearToken();
    expect(sessionStorage.getItem('adminToken')).toBeNull();
    expect(getToken()).toBeNull();
  });

  it('checks authentication status correctly', () => {
    expect(isAuthenticated()).toBe(false);
    setToken('sample-jwt-token');
    expect(isAuthenticated()).toBe(true);
    clearToken();
    expect(isAuthenticated()).toBe(false);
  });
});
