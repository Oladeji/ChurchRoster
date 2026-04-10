import React from 'react';
import type { ProposalStatus } from '../types';

interface Props {
  status: ProposalStatus;
}

const CONFIG: Record<ProposalStatus, { label: string; bg: string; color: string; dot: string }> = {
  Processing: { label: 'Processing', bg: '#FEF9C3', color: '#854D0E', dot: '#EAB308' },
  Draft:      { label: 'Draft',      bg: '#DBEAFE', color: '#1E40AF', dot: '#3B82F6' },
  Published:  { label: 'Published',  bg: '#DCFCE7', color: '#166534', dot: '#22C55E' },
  Archived:   { label: 'Archived',   bg: '#F3F4F6', color: '#374151', dot: '#9CA3AF' },
};

const ProposalStatusBadge: React.FC<Props> = ({ status }) => {
  const cfg = CONFIG[status] ?? CONFIG.Archived;
  return (
    <span style={{
      display: 'inline-flex',
      alignItems: 'center',
      gap: '6px',
      padding: '3px 10px',
      borderRadius: '9999px',
      fontSize: '12px',
      fontWeight: 600,
      background: cfg.bg,
      color: cfg.color,
    }}>
      <span style={{
        width: '7px',
        height: '7px',
        borderRadius: '50%',
        background: cfg.dot,
        flexShrink: 0,
      }} />
      {cfg.label}
    </span>
  );
};

export default ProposalStatusBadge;
