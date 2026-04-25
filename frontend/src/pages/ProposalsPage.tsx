import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import proposalService from '../services/proposal.service';
import ProposalStatusBadge from '../components/ProposalStatusBadge';
import type { ProposalSummary } from '../types';

const ProposalsPage: React.FC = () => {
  const navigate = useNavigate();
  const [proposals, setProposals] = useState<ProposalSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchProposals();
  }, []);

  const fetchProposals = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await proposalService.getAll();
      setProposals(data);
    } catch {
      setError('Failed to load proposals.');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (d: string) =>
    new Date(d).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #EFF6FF 0%, #FFFFFF 50%, #E0E7FF 100%)' }}>
      <div style={{ maxWidth: '1100px', margin: '0 auto', padding: '32px 16px' }}>

        {/* Header */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '32px' }}>
          <div>
            <button
              onClick={() => navigate('/dashboard')}
              style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#6B7280', fontSize: '14px', marginBottom: '8px', padding: 0 }}
            >
              ← Back to Dashboard
            </button>
            <h1 style={{ fontSize: '28px', fontWeight: 'bold', color: '#111827', margin: 0 }}>
              🤖 Roster Proposals
            </h1>
            <p style={{ color: '#6B7280', marginTop: '4px', fontSize: '15px' }}>
              Generated draft rosters — review, edit and publish
            </p>
          </div>
          <button
            onClick={() => navigate('/proposals/generate')}
            style={{
              display: 'flex', alignItems: 'center', gap: '8px',
              padding: '12px 24px',
              background: 'linear-gradient(135deg, #3B82F6, #6366F1)',
              color: 'white', border: 'none', borderRadius: '10px',
              fontWeight: 600, fontSize: '15px', cursor: 'pointer',
              boxShadow: '0 4px 12px rgba(99,102,241,0.35)',
            }}
          >
            ✨ Generate New Proposal
          </button>
        </div>

        {error && (
          <div style={{ background: '#FEE2E2', border: '1px solid #FCA5A5', borderRadius: '8px', padding: '12px 16px', marginBottom: '20px', color: '#991B1B', fontSize: '14px' }}>
            {error}
          </div>
        )}

        {loading ? (
          <div style={{ textAlign: 'center', padding: '80px 0', color: '#6B7280', fontSize: '16px' }}>
            Loading proposals...
          </div>
        ) : proposals.length === 0 ? (
          <div style={{
            background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)',
            padding: '60px 40px', textAlign: 'center',
          }}>
            <div style={{ fontSize: '48px', marginBottom: '16px' }}>🤖</div>
            <h3 style={{ fontSize: '20px', fontWeight: 600, color: '#111827', marginBottom: '8px' }}>No proposals yet</h3>
            <p style={{ color: '#6B7280', marginBottom: '24px' }}>Generate your first AI-powered roster proposal to get started.</p>
            <button
              onClick={() => navigate('/proposals/generate')}
              style={{
                padding: '12px 28px', background: 'linear-gradient(135deg, #3B82F6, #6366F1)',
                color: 'white', border: 'none', borderRadius: '10px',
                fontWeight: 600, cursor: 'pointer',
              }}
            >
              ✨ Generate Proposal
            </button>
          </div>
        ) : (
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)', overflow: 'hidden' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#F9FAFB', borderBottom: '2px solid #E5E7EB' }}>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 700, color: '#6B7280', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Name</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 700, color: '#6B7280', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Date Range</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 700, color: '#6B7280', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Status</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 700, color: '#6B7280', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Items</th>
                  <th style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 700, color: '#6B7280', textTransform: 'uppercase', letterSpacing: '0.05em' }}>Generated</th>
                  <th style={{ padding: '12px 16px' }}></th>
                </tr>
              </thead>
              <tbody>
                {proposals.map((p) => (
                  <tr
                    key={p.proposalId}
                    style={{ borderBottom: '1px solid #F3F4F6', cursor: 'pointer', transition: 'background 0.15s' }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = '#F9FAFB')}
                    onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
                    onClick={() => navigate(`/proposals/${p.proposalId}`)}
                  >
                    <td style={{ padding: '14px 16px', fontSize: '14px', fontWeight: 600, color: '#111827' }}>
                      {p.name}
                    </td>
                    <td style={{ padding: '14px 16px', fontSize: '13px', color: '#374151' }}>
                      {formatDate(p.dateRangeStart)} – {formatDate(p.dateRangeEnd)}
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <ProposalStatusBadge status={p.status} />
                    </td>
                    <td style={{ padding: '14px 16px', fontSize: '14px', color: '#374151' }}>
                      {p.itemCount}
                    </td>
                    <td style={{ padding: '14px 16px', fontSize: '13px', color: '#6B7280' }}>
                      {formatDate(p.generatedAt)}
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <span style={{ color: '#3B82F6', fontSize: '14px', fontWeight: 500 }}>View →</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProposalsPage;
