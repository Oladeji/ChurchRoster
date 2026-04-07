import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import authService from '../services/auth.service';
import type { Tenant } from '../types';

const Login: React.FC = () => {
  const [tenantId, setTenantId] = useState('');
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const loadTenants = async () => {
      try {
        const data = await authService.getTenants();
        setTenants(data);

        const savedTenantId = authService.getSelectedTenantId();
        if (savedTenantId) {
          setTenantId(savedTenantId.toString());
        }
      } catch (err: unknown) {
        console.error(err);
        setError('Failed to load churches');
      }
    };

    loadTenants();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!tenantId) {
      setError('Please select a church');
      return;
    }

    setLoading(true);

    try {
      await login(Number(tenantId), email, password);
      navigate('/dashboard');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { message?: string } | undefined)?.message || 'Login failed. Please try again.');
      } else {
        setError('Login failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Church Ministry Roster</h1>
        <h2>Login</h2>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="tenant">Church</label>
            <select
              id="tenant"
              value={tenantId}
              onChange={(e) => {
                setTenantId(e.target.value);
                if (e.target.value) {
                  localStorage.setItem('tenantId', e.target.value);
                } else {
                  localStorage.removeItem('tenantId');
                }
              }}
              required
            >
              <option value="">Select church</option>
              {tenants.map((tenant) => (
                <option key={tenant.tenantId} value={tenant.tenantId}>
                  {tenant.name}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <div className="login-footer">
          <p>
            Don't have an account?{' '}
            <Link to="/register" style={{ color: '#4F46E5', textDecoration: 'none', fontWeight: '500' }}>
              Register here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
