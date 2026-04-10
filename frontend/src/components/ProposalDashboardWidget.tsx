import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import proposalService from '../services/proposal.service';
import ProposalStatusBadge from './ProposalStatusBadge';
import type { ProposalSummary } from '../types';

const ProposalDashboardWidget: React.FC = () => {
  const navigate = useNavigate();
  const [proposals, setProposals] = useState<ProposalSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    proposalService
      .getAll()
      .then(setProposals)
      .catch(() => setError(true))
      .finally(() => setLoading(false));
  }, []);

  const processing = proposals.filter((p) => p.status === 'Processing');
  const drafts = proposals.filter((p) => p.status === 'Draft');
  const latest = proposals[0] ?? null;

  return (
    <div
      style={{
        background: 'white',
        borderRadius: '16px',
        boxShadow: '0 4px 15px rgba(0,0,0,0.07)',
        overflow: 'hidden',
        cursor: 'pointer',
        transition: 'box-shadow 0.2s',
      }}
      onClick={() => navigate('/proposals')}
      onMouseEnter={(e) => ((e.currentTarget as HTMLDivElement).style.boxShadow = '0 8px 24px rgba(99,102,241,0.18)')}
      onMouseLeave={(e) => ((e.currentTarget as HTMLDivElement).style.boxShadow = '0 4px 15px rgba(0,0,0,0.07)')}
    >
      {/* Gradient header */}
      <div
        style={{
          background: 'linear-gradient(135deg, #3B82F6, #6366F1)',
          padding: '16px 20px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <div
            style={{
              width: '36px',
              height: '36px',
              background: 'rgba(255,255,255,0.2)',
              borderRadius: '8px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '18px',
            }}
          >
            🤖
          </div>
          <div>
            <h3 style={{ margin: 0, fontSize: '15px', fontWeight: 700, color: 'white' }}>AI Proposals</h3>
            <p style={{ margin: 0, fontSize: '12px', color: 'rgba(255,255,255,0.8)' }}>Roster generation</p>
          </div>
        </div>
        <span style={{ color: 'rgba(255,255,255,0.9)', fontSize: '18px' }}>→</span>
      </div>

      {/* Body */}
      <div style={{ padding: '16px 20px' }}>
        {loading ? (
          <p style={{ margin: 0, fontSize: '13px', color: '#9CA3AF' }}>Loading…</p>
        ) : error ? (
          <p style={{ margin: 0, fontSize: '13px', color: '#EF4444' }}>Failed to load proposals.</p>
        ) : (
          <>
            {/* Stat pills */}
            <div style={{ display: 'flex', gap: '8px', marginBottom: '14px', flexWrap: 'wrap' }}>
              <span
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '5px',
                  padding: '4px 10px',
                  background: processing.length > 0 ? '#FEF9C3' : '#F3F4F6',
                  color: processing.length > 0 ? '#854D0E' : '#6B7280',
                  borderRadius: '9999px',
                  fontSize: '12px',
                  fontWeight: 600,
                }}
              >
                {processing.length > 0 && (
                  <span style={{ width: '7px', height: '7px', borderRadius: '50%', background: '#EAB308', flexShrink: 0, display: 'inline-block' }} />
                )}
                {processing.length} Processing
              </span>

              <span
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '5px',
                  padding: '4px 10px',
                  background: drafts.length > 0 ? '#DBEAFE' : '#F3F4F6',
                  color: drafts.length > 0 ? '#1E40AF' : '#6B7280',
                  borderRadius: '9999px',
                  fontSize: '12px',
                  fontWeight: 600,
                }}
              >
                {drafts.length > 0 && (
                  <span style={{ width: '7px', height: '7px', borderRadius: '50%', background: '#3B82F6', flexShrink: 0, display: 'inline-block' }} />
                )}
                {drafts.length} Draft{drafts.length !== 1 ? 's' : ''}
              </span>

              <span
                style={{
                  padding: '4px 10px',
                  background: '#F3F4F6',
                  color: '#6B7280',
                  borderRadius: '9999px',
                  fontSize: '12px',
                  fontWeight: 600,
                }}
              >
                {proposals.length} Total
              </span>
            </div>

            {/* Latest proposal */}
            {latest ? (
              <div
                style={{
                  border: '1px solid #E5E7EB',
                  borderRadius: '10px',
                  padding: '10px 14px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: '8px',
                }}
              >
                <div style={{ minWidth: 0 }}>
                  <p
                    style={{
                      margin: 0,
                      fontSize: '13px',
                      fontWeight: 600,
                      color: '#111827',
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }}
                  >
                    {latest.name}
                  </p>
                  <p style={{ margin: '2px 0 0', fontSize: '11px', color: '#9CA3AF' }}>
                    {new Date(latest.dateRangeStart).toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
                    {' – '}
                    {new Date(latest.dateRangeEnd).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })}
                  </p>
                </div>
                <ProposalStatusBadge status={latest.status} />
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '12px 0' }}>
                <p style={{ margin: '0 0 8px', fontSize: '13px', color: '#6B7280' }}>No proposals yet</p>
                <button
                  onClick={(e) => { e.stopPropagation(); navigate('/proposals/generate'); }}
                  style={{
                    padding: '6px 14px',
                    background: 'linear-gradient(135deg, #3B82F6, #6366F1)',
                    color: 'white',
                    border: 'none',
                    borderRadius: '7px',
                    fontSize: '12px',
                    fontWeight: 600,
                    cursor: 'pointer',
                  }}
                >
                  ✨ Generate First
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default ProposalDashboardWidget;
