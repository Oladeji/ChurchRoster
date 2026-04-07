import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import authService from '../services/auth.service';
import type { Tenant } from '../types';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    tenantId: '',
    name: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: ''
  });
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { register } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const loadTenants = async () => {
      try {
        const data = await authService.getTenants();
        setTenants(data);

        const savedTenantId = authService.getSelectedTenantId();
        if (savedTenantId) {
          setFormData(prev => ({ ...prev, tenantId: savedTenantId.toString() }));
        }
      } catch (err: unknown) {
        console.error(err);
        setError('Failed to load churches');
      }
    };

    loadTenants();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!formData.tenantId) {
      setError('Please select a church');
      return;
    }

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (formData.password.length < 8) {
      setError('Password must be at least 8 characters long');
      return;
    }

    // Check password requirements
    const hasUpperCase = /[A-Z]/.test(formData.password);
    const hasLowerCase = /[a-z]/.test(formData.password);
    const hasNumber = /[0-9]/.test(formData.password);

    if (!hasUpperCase || !hasLowerCase || !hasNumber) {
      setError('Password must contain uppercase, lowercase, and a number');
      return;
    }

    setLoading(true);

    try {
      await register(Number(formData.tenantId), formData.name, formData.email, formData.phone, formData.password);
      navigate('/dashboard');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError((err.response?.data as { message?: string } | undefined)?.message || 'Registration failed. Please try again.');
      } else {
        setError('Registration failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card" style={{ maxWidth: '500px' }}>
        <h1>Church Ministry Roster</h1>
        <h2>Create Account</h2>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="tenantId">Church *</label>
            <select
              id="tenantId"
              name="tenantId"
              value={formData.tenantId}
              onChange={(e) => {
                setFormData({ ...formData, tenantId: e.target.value });
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
            <label htmlFor="name">Full Name *</label>
            <input
              id="name"
              name="name"
              type="text"
              value={formData.name}
              onChange={handleChange}
              required
              autoComplete="name"
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email *</label>
            <input
              id="email"
              name="email"
              type="email"
              value={formData.email}
              onChange={handleChange}
              required
              autoComplete="email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="phone">Phone Number *</label>
            <input
              id="phone"
              name="phone"
              type="tel"
              value={formData.phone}
              onChange={handleChange}
              required
              autoComplete="tel"
              placeholder="555-0123"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password *</label>
            <input
              id="password"
              name="password"
              type="password"
              value={formData.password}
              onChange={handleChange}
              required
              autoComplete="new-password"
            />
            <small style={{ color: '#6B7280', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              Min 8 characters, with uppercase, lowercase, and number
            </small>
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password *</label>
            <input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              autoComplete="new-password"
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Creating Account...' : 'Create Account'}
          </button>
        </form>

        <div className="login-footer">
          <p>
            Already have an account?{' '}
            <Link to="/login" style={{ color: '#4F46E5', textDecoration: 'none', fontWeight: '500' }}>
              Login here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;
