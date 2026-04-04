import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import invitationService from '../services/invitation.service';

interface InvitationDetails {
  email: string;
  name: string;
  phone: string;
  role: string;
  expiresAt: string;
}

const AcceptInvitation = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [invitationDetails, setInvitationDetails] = useState<InvitationDetails | null>(null);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  useEffect(() => {
    if (!token) {
      setError('Invalid invitation link');
      setLoading(false);
      return;
    }
    verifyInvitation();
  }, [token]);

  const verifyInvitation = async () => {
    if (!token) return;
    try {
      const response = await invitationService.verifyToken(token);
      if (response.isValid) {
        setInvitationDetails({
          email: response.email,
          name: response.name,
          phone: response.phone,
          role: response.role,
          expiresAt: response.expiresAt
        });
      } else {
        setError(response.message || 'Invalid or expired invitation');
      }
    } catch {
      setError('Invalid or expired invitation');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }
    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }
    if (!token) return;
    try {
      setSubmitting(true);
      await invitationService.acceptInvitation({ token, password });
      setSuccess(true);
      setTimeout(() => navigate('/login'), 2500);
    } catch {
      setError('Failed to accept invitation');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f9fafb' }}>
        <div style={{ backgroundColor: 'white', borderRadius: '16px', boxShadow: '0 10px 40px rgba(0,0,0,0.1)', padding: '48px', maxWidth: '400px', width: '100%', textAlign: 'center' }}>
          <div style={{ width: '60px', height: '60px', margin: '0 auto 24px', border: '4px solid #e5e7eb', borderTop: '4px solid #8b5cf6', borderRadius: '50%', animation: 'spin 1s linear infinite' }}></div>
          <h3 style={{ fontSize: '24px', fontWeight: 'bold', color: '#111827', marginBottom: '8px' }}>Verifying Invitation</h3>
          <p style={{ color: '#6b7280' }}>Please wait...</p>
          <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
        </div>
      </div>
    );
  }

  if (error && !invitationDetails) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f9fafb' }}>
        <div style={{ backgroundColor: 'white', borderRadius: '16px', boxShadow: '0 10px 40px rgba(0,0,0,0.1)', padding: '48px', maxWidth: '400px', width: '100%', textAlign: 'center' }}>
          <h2 style={{ fontSize: '28px', fontWeight: 'bold', color: '#111827', marginBottom: '12px' }}>Invalid Invitation</h2>
          <p style={{ color: '#6b7280', marginBottom: '32px' }}>{error}</p>
          <button onClick={() => navigate('/login')} style={{ width: '100%', background: 'linear-gradient(to right, #8b5cf6, #6366f1)', color: 'white', fontWeight: '600', padding: '16px', borderRadius: '12px', border: 'none', cursor: 'pointer' }}>
            Return to Login
          </button>
        </div>
      </div>
    );
  }

  if (success) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f9fafb' }}>
        <div style={{ backgroundColor: 'white', borderRadius: '16px', boxShadow: '0 10px 40px rgba(0,0,0,0.1)', padding: '48px', maxWidth: '400px', width: '100%', textAlign: 'center' }}>
          <h2 style={{ fontSize: '28px', fontWeight: 'bold', color: '#111827', marginBottom: '12px' }}>Welcome! 🎉</h2>
          <p style={{ color: '#6b7280' }}>Account created successfully. Redirecting...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f9fafb', padding: '40px 20px' }}>
      <div style={{ maxWidth: '900px', margin: '0 auto' }}>
        <div style={{ textAlign: 'center', marginBottom: '32px' }}>
          <div style={{ width: '70px', height: '70px', margin: '0 auto 16px', background: 'linear-gradient(135deg, #8b5cf6, #6366f1)', borderRadius: '16px', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '36px' }}>⛪</div>
          <h1 style={{ fontSize: '32px', fontWeight: 'bold', color: '#111827', marginBottom: '8px' }}>Church Roster System</h1>
          <p style={{ color: '#6b7280' }}>Complete your registration</p>
        </div>

        <div style={{ backgroundColor: 'white', borderRadius: '16px', boxShadow: '0 10px 40px rgba(0,0,0,0.1)', overflow: 'hidden', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))' }}>
          <div style={{ background: 'linear-gradient(135deg, #8b5cf6, #6366f1)', padding: '40px', color: 'white' }}>
            <h2 style={{ fontSize: '24px', fontWeight: 'bold', marginBottom: '12px' }}>Welcome!</h2>
            <p style={{ marginBottom: '24px', opacity: '0.9' }}>Join our church roster system</p>
            {invitationDetails && (
              <div style={{ backgroundColor: 'rgba(255,255,255,0.15)', borderRadius: '12px', padding: '20px' }}>
                <div style={{ marginBottom: '12px' }}><strong>Name:</strong> {invitationDetails.name}</div>
                <div style={{ marginBottom: '12px' }}><strong>Email:</strong> {invitationDetails.email}</div>
                <div style={{ marginBottom: '12px' }}><strong>Phone:</strong> {invitationDetails.phone}</div>
                <div><strong>Role:</strong> <span style={{ backgroundColor: 'white', color: '#8b5cf6', padding: '4px 12px', borderRadius: '6px', fontWeight: 'bold' }}>{invitationDetails.role}</span></div>
              </div>
            )}
          </div>

          <div style={{ padding: '40px' }}>
            <h3 style={{ fontSize: '20px', fontWeight: 'bold', color: '#111827', marginBottom: '8px' }}>Create Password</h3>
            <p style={{ color: '#6b7280', marginBottom: '24px' }}>Set up your account password</p>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {error && <div style={{ backgroundColor: '#fef2f2', border: '1px solid #fecaca', borderRadius: '8px', padding: '12px', color: '#991b1b' }}>{error}</div>}

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontWeight: '600', fontSize: '14px' }}>Password</label>
                <div style={{ position: 'relative' }}>
                  <input type={showPassword ? 'text' : 'password'} value={password} onChange={(e) => setPassword(e.target.value)} style={{ width: '100%', padding: '12px', border: '2px solid #e5e7eb', borderRadius: '8px', fontSize: '16px' }} placeholder="Enter password" required minLength={6} />
                  <button type="button" onClick={() => setShowPassword(!showPassword)} style={{ position: 'absolute', right: '12px', top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', fontSize: '18px' }}>{showPassword ? '🙈' : '👁️'}</button>
                </div>
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '6px', fontWeight: '600', fontSize: '14px' }}>Confirm Password</label>
                <div style={{ position: 'relative' }}>
                  <input type={showConfirmPassword ? 'text' : 'password'} value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} style={{ width: '100%', padding: '12px', border: '2px solid #e5e7eb', borderRadius: '8px', fontSize: '16px' }} placeholder="Confirm password" required minLength={6} />
                  <button type="button" onClick={() => setShowConfirmPassword(!showConfirmPassword)} style={{ position: 'absolute', right: '12px', top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', fontSize: '18px' }}>{showConfirmPassword ? '🙈' : '👁️'}</button>
                </div>
                {confirmPassword && password === confirmPassword && <div style={{ marginTop: '8px', color: '#059669', fontSize: '14px' }}>✅ Passwords match</div>}
              </div>

              <button type="submit" disabled={submitting} style={{ background: 'linear-gradient(to right, #8b5cf6, #6366f1)', color: 'white', fontWeight: 'bold', padding: '14px', borderRadius: '8px', border: 'none', cursor: submitting ? 'not-allowed' : 'pointer', opacity: submitting ? 0.6 : 1 }}>
                {submitting ? 'Creating Account...' : 'Create Account'}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AcceptInvitation;
