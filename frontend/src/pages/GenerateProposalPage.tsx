import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import proposalService from '../services/proposal.service';

const GenerateProposalPage: React.FC = () => {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !startDate || !endDate) {
      setError('All fields are required.');
      return;
    }
    if (endDate < startDate) {
      setError('End date must be on or after start date.');
      return;
    }

    try {
      setSubmitting(true);
      setError('');
      const result = await proposalService.generate({
        name: name.trim(),
        dateRangeStart: startDate,
        dateRangeEnd: endDate,
      });
      navigate(`/proposals/${result.proposalId}`);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { status?: number } };
      if (axiosErr?.response?.status === 409) {
        setError('A proposal is already being generated for your church. Please wait for it to finish.');
      } else {
        setError('Failed to start proposal generation. Please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #EFF6FF 0%, #FFFFFF 50%, #E0E7FF 100%)', display: 'flex', alignItems: 'flex-start', justifyContent: 'center', paddingTop: '64px' }}>
      <div style={{ width: '100%', maxWidth: '520px', margin: '0 16px' }}>

        <button
          onClick={() => navigate('/proposals')}
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#6B7280', fontSize: '14px', marginBottom: '20px', padding: 0 }}
        >
          ← Back to Proposals
        </button>

        <div style={{ background: 'white', borderRadius: '20px', boxShadow: '0 8px 30px rgba(0,0,0,0.10)', overflow: 'hidden' }}>
          {/* Card header */}
          <div style={{ background: 'linear-gradient(135deg, #3B82F6, #6366F1)', padding: '28px 32px' }}>
            <div style={{ fontSize: '36px', marginBottom: '8px' }}>✨</div>
            <h1 style={{ fontSize: '22px', fontWeight: 'bold', color: 'white', margin: 0 }}>Generate Roster Proposal</h1>
            <p style={{ color: 'rgba(255,255,255,0.85)', marginTop: '6px', fontSize: '14px' }}>
              The AI agent will build a draft roster for the selected date range, honouring member availability and fairness rules.
            </p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} style={{ padding: '32px' }}>
            {error && (
              <div style={{ background: '#FEE2E2', border: '1px solid #FCA5A5', borderRadius: '8px', padding: '12px 16px', marginBottom: '20px', color: '#991B1B', fontSize: '14px' }}>
                {error}
              </div>
            )}

            <div style={{ marginBottom: '20px' }}>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, color: '#374151', marginBottom: '6px' }}>
                Proposal Name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. May 2026 Sunday Roster"
                style={{ width: '100%', padding: '10px 12px', fontSize: '14px', border: '1.5px solid #D1D5DB', borderRadius: '8px', boxSizing: 'border-box', outline: 'none' }}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '28px' }}>
              <div>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, color: '#374151', marginBottom: '6px' }}>
                  Start Date
                </label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  style={{ width: '100%', padding: '10px 12px', fontSize: '14px', border: '1.5px solid #D1D5DB', borderRadius: '8px', boxSizing: 'border-box' }}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, color: '#374151', marginBottom: '6px' }}>
                  End Date
                </label>
                <input
                  type="date"
                  value={endDate}
                  min={startDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  style={{ width: '100%', padding: '10px 12px', fontSize: '14px', border: '1.5px solid #D1D5DB', borderRadius: '8px', boxSizing: 'border-box' }}
                />
              </div>
            </div>

            <div style={{ display: 'flex', gap: '12px' }}>
              <button
                type="button"
                onClick={() => navigate('/proposals')}
                style={{ flex: 1, padding: '12px', background: '#F3F4F6', color: '#374151', border: 'none', borderRadius: '10px', fontWeight: 600, fontSize: '14px', cursor: 'pointer' }}
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={submitting}
                style={{
                  flex: 2, padding: '12px',
                  background: submitting ? '#9CA3AF' : 'linear-gradient(135deg, #3B82F6, #6366F1)',
                  color: 'white', border: 'none', borderRadius: '10px',
                  fontWeight: 600, fontSize: '14px',
                  cursor: submitting ? 'not-allowed' : 'pointer',
                  boxShadow: submitting ? 'none' : '0 4px 12px rgba(99,102,241,0.35)',
                }}
              >
                {submitting ? 'Starting…' : '✨ Generate Proposal'}
              </button>
            </div>
          </form>
        </div>

        <p style={{ textAlign: 'center', color: '#9CA3AF', fontSize: '13px', marginTop: '16px' }}>
          Generation runs in the background — you'll be taken to the detail page to track progress.
        </p>
      </div>
    </div>
  );
};

export default GenerateProposalPage;
