import React from 'react';
import type { ProposalItem, User } from '../types';

interface Props {
  item: ProposalItem;
  members: User[];
  editable: boolean;
  onMemberChange: (itemId: number, userId: number) => void;
  onDelete: (itemId: number) => void;
}

const STATUS_STYLE: Record<string, { bg: string; color: string }> = {
  Proposed: { bg: '#FEF9C3', color: '#854D0E' },
  Skipped:  { bg: '#FEE2E2', color: '#991B1B' },
};

const ProposalItemRow: React.FC<Props> = ({ item, members, editable, onMemberChange, onDelete }) => {
  const style = STATUS_STYLE[item.status] ?? { bg: '#F3F4F6', color: '#374151' };
  const eventDate = new Date(item.eventDate+ 'T00:00:00');
  const dateLabel = eventDate.toLocaleDateString('en-GB', { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
//     {new Date(dateKey + 'T00:00:00').toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
            
  return (
    <tr style={{ borderBottom: '1px solid #E5E7EB' }}>
      <td style={{ padding: '10px 12px', fontSize: '14px', whiteSpace: 'nowrap' }}>{dateLabel}</td>
      <td style={{ padding: '10px 12px', fontSize: '14px', fontWeight: 500 }}>{item.taskName}</td>
      <td style={{ padding: '10px 12px', fontSize: '14px' }}>
        {editable ? (
          <select
            value={item.userId}
            onChange={(e) => onMemberChange(item.itemId, Number(e.target.value))}
            style={{
              padding: '5px 8px',
              fontSize: '13px',
              border: '1px solid #D1D5DB',
              borderRadius: '6px',
              background: 'white',
              minWidth: '160px',
            }}
          >
            {members.map((m) => (
              <option key={m.userId} value={m.userId}>{m.name}</option>
            ))}
          </select>
        ) : (
          item.memberName
        )}
      </td>
      <td style={{ padding: '10px 12px' }}>
        <span style={{
          display: 'inline-block',
          padding: '2px 8px',
          borderRadius: '9999px',
          fontSize: '12px',
          fontWeight: 600,
          background: style.bg,
          color: style.color,
        }}>
          {item.status}
        </span>
      </td>
      <td style={{ padding: '10px 12px', fontSize: '13px', color: '#6B7280' }}>
        {item.skipReason ?? '—'}
      </td>
      {editable && (
        <td style={{ padding: '10px 12px' }}>
          <button
            onClick={() => onDelete(item.itemId)}
            title="Remove item"
            style={{
              background: 'none',
              border: 'none',
              cursor: 'pointer',
              color: '#EF4444',
              fontSize: '18px',
              lineHeight: 1,
              padding: '2px 6px',
            }}
          >
            ✕
          </button>
        </td>
      )}
    </tr>
  );
};

export default ProposalItemRow;
