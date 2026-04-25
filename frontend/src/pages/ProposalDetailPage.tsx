import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import proposalService from '../services/proposal.service';
import memberService from '../services/member.service';
import ProposalStatusBadge from '../components/ProposalStatusBadge';
import ProposalItemRow from '../components/ProposalItemRow';
import type { ProposalDetail, User, PublishResult } from '../types';

const POLL_INTERVAL_MS = 3000;

const ProposalDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const proposalId = Number(id);

  const [proposal, setProposal] = useState<ProposalDetail | null>(null);
  const [members, setMembers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [publishResult, setPublishResult] = useState<PublishResult | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [addTaskId, setAddTaskId] = useState('');
  const [addUserId, setAddUserId] = useState('');
  const [addDate, setAddDate] = useState('');
  const [addError, setAddError] = useState('');

  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const stopPolling = useCallback(() => {
    if (pollRef.current) {
      clearInterval(pollRef.current);
      pollRef.current = null;
    }
  }, []);

  const fetchProposal = useCallback(async (silent = false) => {
    try {
      if (!silent) setLoading(true);
      const data = await proposalService.getById(proposalId);
      setProposal(data);
      if (data.status !== 'Processing') stopPolling();
    } catch {
      setError('Failed to load proposal.');
      stopPolling();
    } finally {
      if (!silent) setLoading(false);
    }
  }, [proposalId, stopPolling]);

  useEffect(() => {
    const init = async () => {
      await Promise.all([fetchProposal(), fetchMembers()]);
    };
    init();
    return () => stopPolling();
  }, [fetchProposal, stopPolling]);

  useEffect(() => {
    if (proposal?.status === 'Processing' && !pollRef.current) {
      pollRef.current = setInterval(() => fetchProposal(true), POLL_INTERVAL_MS);
    }
    if (proposal?.status !== 'Processing') stopPolling();
  }, [proposal?.status, fetchProposal, stopPolling]);

  const fetchMembers = async () => {
    try {
      const data = await memberService.getAll();
      setMembers(data);
    } catch {
      // non-fatal
    }
  };

  const handleMemberChange = async (itemId: number, userId: number) => {
    if (!proposal) return;
    try {
      await proposalService.updateItem(proposal.proposalId, itemId, { userId });
      await fetchProposal(true);
    } catch {
      alert('Failed to update member. Please try again.');
    }
  };

  const handleDeleteItem = async (itemId: number) => {
    if (!proposal) return;
    if (!window.confirm('Remove this item from the proposal?')) return;
    try {
      await proposalService.deleteItem(proposal.proposalId, itemId);
      await fetchProposal(true);
    } catch {
      alert('Failed to remove item. Please try again.');
    }
  };

  const handlePublish = async () => {
    if (!proposal) return;
    if (!window.confirm('Publish this proposal? All items will be converted to Assignments and notifications will be sent to assigned members.')) return;
    try {
      setActionLoading('publish');
      const result = await proposalService.publish(proposal.proposalId);
      setPublishResult(result);
      await fetchProposal(true);
    } catch {
      alert('Failed to publish proposal. Please try again.');
    } finally {
      setActionLoading(null);
    }
  };

  const handleArchive = async () => {
    if (!proposal) return;
    if (!window.confirm('Archive this proposal? This action cannot be undone.')) return;
    try {
      setActionLoading('archive');
      await proposalService.archive(proposal.proposalId);
      await fetchProposal(true);
    } catch {
      alert('Failed to archive proposal. Please try again.');
    } finally {
      setActionLoading(null);
    }
  };

  const handlePrint = async () => {
    if (!proposal) return;
    try {
      setActionLoading('print');
      await proposalService.downloadDraftPdf(proposal.proposalId, proposal.name);
    } catch {
      alert('Failed to download PDF. Please try again.');
    } finally {
      setActionLoading(null);
    }
  };

  const handleAddItem = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!proposal || !addTaskId || !addUserId || !addDate) {
      setAddError('All fields are required.');
      return;
    }
    try {
      setAddError('');
      await proposalService.addItem(proposal.proposalId, {
        taskId: Number(addTaskId),
        userId: Number(addUserId),
        eventDate: addDate,
      });
      setShowAddForm(false);
      setAddTaskId('');
      setAddUserId('');
      setAddDate('');
      await fetchProposal(true);
    } catch {
      setAddError('Failed to add item. Please try again.');
    }
  };

  const formatDate = (d: string) =>
    new Date(d).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });

  const isEditable = proposal?.status === 'Draft';

  if (loading) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#F9FAFB' }}>
        <div style={{ textAlign: 'center', color: '#6B7280' }}>
          <div style={{ fontSize: '32px', marginBottom: '12px' }}>⏳</div>
          <p>Loading proposal...</p>
        </div>
      </div>
    );
  }

  if (error || !proposal) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#F9FAFB' }}>
        <div style={{ textAlign: 'center', color: '#991B1B' }}>
          <p>{error || 'Proposal not found.'}</p>
          <button onClick={() => navigate('/proposals')} style={{ marginTop: '12px', padding: '8px 16px', background: '#3B82F6', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer' }}>
            Back to Proposals
          </button>
        </div>
      </div>
    );
  }

  // Group items by date for display
  const itemsByDate = proposal.items.reduce<Record<string, typeof proposal.items>>((acc, item) => {
    const key = item.eventDate.slice(0, 10);
    (acc[key] = acc[key] ?? []).push(item);
    return acc;
  }, {});
  const sortedDates = Object.keys(itemsByDate).sort();

  // Unique task IDs in current items (for add-item task selector fallback)
  const uniqueTasks = Array.from(new Map(proposal.items.map((i) => [i.taskId, { taskId: i.taskId, taskName: i.taskName }])).values());

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #EFF6FF 0%, #FFFFFF 60%, #E0E7FF 100%)' }}>
      <div style={{ maxWidth: '1100px', margin: '0 auto', padding: '32px 16px' }}>

        {/* Back */}
        <button
          onClick={() => navigate('/proposals')}
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#6B7280', fontSize: '14px', marginBottom: '20px', padding: 0 }}
        >
          ← Back to Proposals
        </button>

        {/* Title bar */}
        <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)', padding: '24px 28px', marginBottom: '20px' }}>
          <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap', gap: '16px' }}>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '6px' }}>
                <h1 style={{ fontSize: '22px', fontWeight: 'bold', color: '#111827', margin: 0 }}>{proposal.name}</h1>
                <ProposalStatusBadge status={proposal.status} />
              </div>
              <p style={{ color: '#6B7280', fontSize: '14px', margin: 0 }}>
                {formatDate(proposal.dateRangeStart)} – {formatDate(proposal.dateRangeEnd)}
                {proposal.publishedAt && ` · Published ${formatDate(proposal.publishedAt)}`}
              </p>
            </div>

            {/* Action buttons */}
            <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
              {(proposal.status === 'Draft' || proposal.status === 'Published') && (
                <button
                  onClick={handlePrint}
                  disabled={actionLoading === 'print'}
                  style={{ padding: '9px 18px', background: '#F3F4F6', color: '#374151', border: '1.5px solid #D1D5DB', borderRadius: '8px', fontWeight: 600, fontSize: '13px', cursor: actionLoading === 'print' ? 'not-allowed' : 'pointer' }}
                >
                  {actionLoading === 'print' ? 'Downloading…' : '📄 Print PDF'}
                </button>
              )}
              {proposal.status === 'Draft' && (
                <>
                  <button
                    onClick={handleArchive}
                    disabled={!!actionLoading}
                    style={{ padding: '9px 18px', background: '#FEF2F2', color: '#991B1B', border: '1.5px solid #FCA5A5', borderRadius: '8px', fontWeight: 600, fontSize: '13px', cursor: actionLoading ? 'not-allowed' : 'pointer' }}
                  >
                    {actionLoading === 'archive' ? 'Archiving…' : '🗄 Archive'}
                  </button>
                  <button
                    onClick={handlePublish}
                    disabled={!!actionLoading}
                    style={{ padding: '9px 18px', background: 'linear-gradient(135deg, #22C55E, #16A34A)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: 600, fontSize: '13px', cursor: actionLoading ? 'not-allowed' : 'pointer', boxShadow: '0 3px 10px rgba(34,197,94,0.3)' }}
                  >
                    {actionLoading === 'publish' ? 'Publishing…' : '🚀 Publish'}
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Processing banner */}
          {proposal.status === 'Processing' && (
            <div style={{ marginTop: '16px', background: '#FEF9C3', border: '1px solid #FDE047', borderRadius: '8px', padding: '12px 16px', display: 'flex', alignItems: 'center', gap: '12px' }}>
              <span style={{ fontSize: '20px' }}>⏳</span>
              <div>
                <p style={{ margin: 0, fontWeight: 600, color: '#854D0E', fontSize: '14px' }}>AI is generating the roster…</p>
                <p style={{ margin: 0, fontSize: '13px', color: '#92400E' }}>This page will update automatically when the draft is ready.</p>
              </div>
            </div>
          )}

          {/* Publish result banner */}
          {publishResult && (
            <div style={{ marginTop: '16px', background: publishResult.slotsSkipped > 0 ? '#FFFBEB' : '#F0FDF4', border: `1px solid ${publishResult.slotsSkipped > 0 ? '#FCD34D' : '#86EFAC'}`, borderRadius: '10px', padding: '14px 18px' }}>
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: publishResult.slotsSkipped > 0 ? '10px' : 0 }}>
                <p style={{ margin: 0, fontWeight: 700, fontSize: '14px', color: publishResult.slotsSkipped > 0 ? '#92400E' : '#166534' }}>
                  ✅ Published — {publishResult.assignmentsCreated} assignment{publishResult.assignmentsCreated !== 1 ? 's' : ''} created
                  {publishResult.slotsSkipped > 0 && `, ${publishResult.slotsSkipped} slot${publishResult.slotsSkipped !== 1 ? 's' : ''} skipped`}
                </p>
                <button onClick={() => setPublishResult(null)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '16px', color: '#6B7280', lineHeight: 1 }}>✕</button>
              </div>
              {publishResult.skipped.length > 0 && (
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid #FCD34D' }}>
                      <th style={{ textAlign: 'left', padding: '4px 8px', color: '#92400E', fontWeight: 700, fontSize: '11px', textTransform: 'uppercase' }}>Task</th>
                      <th style={{ textAlign: 'left', padding: '4px 8px', color: '#92400E', fontWeight: 700, fontSize: '11px', textTransform: 'uppercase' }}>Member</th>
                      <th style={{ textAlign: 'left', padding: '4px 8px', color: '#92400E', fontWeight: 700, fontSize: '11px', textTransform: 'uppercase' }}>Date</th>
                      <th style={{ textAlign: 'left', padding: '4px 8px', color: '#92400E', fontWeight: 700, fontSize: '11px', textTransform: 'uppercase' }}>Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    {publishResult.skipped.map((s, i) => (
                      <tr key={i} style={{ borderBottom: '1px solid #FEF3C7' }}>
                        <td style={{ padding: '5px 8px', color: '#374151' }}>{s.taskName}</td>
                        <td style={{ padding: '5px 8px', color: '#374151' }}>{s.memberName}</td>
                        <td style={{ padding: '5px 8px', color: '#374151' }}>{new Date(s.eventDate + 'T00:00:00').toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })}</td>
                        <td style={{ padding: '5px 8px', color: '#B45309', fontStyle: 'italic' }}>{s.reason}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>

        {/* Summary stats */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '14px', marginBottom: '20px' }}>
          {[
            { label: 'Total Items', value: proposal.items.length, color: '#3B82F6' },
            { label: 'Proposed', value: proposal.items.filter((i) => i.status === 'Proposed').length, color: '#EAB308' },
            { label: 'Skipped', value: proposal.items.filter((i) => i.status === 'Skipped').length, color: '#EF4444' },
            { label: 'Conflicts Logged', value: proposal.skipLogs.length, color: '#8B5CF6' },
          ].map((stat) => (
            <div key={stat.label} style={{ background: 'white', borderRadius: '12px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', padding: '16px 20px' }}>
              <p style={{ margin: 0, fontSize: '12px', color: '#6B7280', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.05em' }}>{stat.label}</p>
              <p style={{ margin: '6px 0 0', fontSize: '26px', fontWeight: 'bold', color: stat.color }}>{stat.value}</p>
            </div>
          ))}
        </div>

        {/* Items table */}
        {proposal.items.length > 0 ? (
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)', overflow: 'hidden', marginBottom: '20px' }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '16px 20px', borderBottom: '1px solid #E5E7EB' }}>
              <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#111827', margin: 0 }}>Roster Items</h2>
              {isEditable && (
                <button
                  onClick={() => setShowAddForm((v) => !v)}
                  style={{ padding: '7px 14px', background: 'linear-gradient(135deg, #3B82F6, #6366F1)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: 600, fontSize: '13px', cursor: 'pointer' }}
                >
                  {showAddForm ? '✕ Cancel' : '+ Add Item'}
                </button>
              )}
            </div>

            {/* Add item form */}
            {isEditable && showAddForm && (
              <form onSubmit={handleAddItem} style={{ padding: '16px 20px', background: '#F9FAFB', borderBottom: '1px solid #E5E7EB', display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'flex-end' }}>
                {addError && <p style={{ color: '#991B1B', fontSize: '13px', margin: 0, width: '100%' }}>{addError}</p>}
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: 600, marginBottom: '4px', color: '#374151' }}>Task</label>
                  <select value={addTaskId} onChange={(e) => setAddTaskId(e.target.value)} style={{ padding: '8px 10px', fontSize: '13px', border: '1px solid #D1D5DB', borderRadius: '7px', minWidth: '160px' }}>
                    <option value="">Select task…</option>
                    {uniqueTasks.map((t) => <option key={t.taskId} value={t.taskId}>{t.taskName}</option>)}
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: 600, marginBottom: '4px', color: '#374151' }}>Member</label>
                  <select value={addUserId} onChange={(e) => setAddUserId(e.target.value)} style={{ padding: '8px 10px', fontSize: '13px', border: '1px solid #D1D5DB', borderRadius: '7px', minWidth: '160px' }}>
                    <option value="">Select member…</option>
                    {members.map((m) => <option key={m.userId} value={m.userId}>{m.name}</option>)}
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: 600, marginBottom: '4px', color: '#374151' }}>Date</label>
                  <input type="date" value={addDate} onChange={(e) => setAddDate(e.target.value)} min={proposal.dateRangeStart} max={proposal.dateRangeEnd}
                    style={{ padding: '8px 10px', fontSize: '13px', border: '1px solid #D1D5DB', borderRadius: '7px' }} />
                </div>
                <button type="submit" style={{ padding: '9px 18px', background: '#22C55E', color: 'white', border: 'none', borderRadius: '8px', fontWeight: 600, fontSize: '13px', cursor: 'pointer' }}>
                  Add
                </button>
              </form>
            )}

            {/* Grouped by date */}
            {sortedDates.map((dateKey) => (
              <div key={dateKey}>
                <div style={{ padding: '9px 20px', background: '#EFF6FF', borderBottom: '1px solid #DBEAFE' }}>
                  <span style={{ fontSize: '13px', fontWeight: 700, color: '#1E40AF' }}>
                    {new Date(dateKey + 'T00:00:00').toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                  </span>
                </div>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                  <thead>
                    <tr style={{ background: '#F9FAFB' }}>
                      <th style={{ padding: '8px 12px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase', width: '130px' }}>Date</th>
                      <th style={{ padding: '8px 12px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Task</th>
                      <th style={{ padding: '8px 12px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Assigned To</th>
                      <th style={{ padding: '8px 12px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase', width: '90px' }}>Status</th>
                      <th style={{ padding: '8px 12px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Skip Reason</th>
                      {isEditable && <th style={{ width: '40px' }}></th>}
                    </tr>
                  </thead>
                  <tbody>
                    {itemsByDate[dateKey].map((item) => (
                      <ProposalItemRow
                        key={item.itemId}
                        item={item}
                        members={members}
                        editable={isEditable}
                        onMemberChange={handleMemberChange}
                        onDelete={handleDeleteItem}
                      />
                    ))}
                  </tbody>
                </table>
              </div>
            ))}
          </div>
        ) : proposal.status !== 'Processing' ? (
          <div style={{ background: 'white', borderRadius: '16px', padding: '40px', textAlign: 'center', color: '#6B7280', marginBottom: '20px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)' }}>
            No items in this proposal yet.
          </div>
        ) : null}

        {/* Skip logs */}
        {proposal.skipLogs.length > 0 && (
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.07)', overflow: 'hidden' }}>
            <div style={{ padding: '16px 20px', borderBottom: '1px solid #E5E7EB', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <span style={{ fontSize: '16px' }}>⚠️</span>
              <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#111827', margin: 0 }}>Conflict Log</h2>
              <span style={{ background: '#FEE2E2', color: '#991B1B', fontSize: '12px', fontWeight: 700, padding: '2px 8px', borderRadius: '9999px' }}>{proposal.skipLogs.length}</span>
            </div>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ background: '#FEF2F2' }}>
                  <th style={{ padding: '8px 16px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Date</th>
                  <th style={{ padding: '8px 16px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Task</th>
                  <th style={{ padding: '8px 16px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Reason</th>
                  <th style={{ padding: '8px 16px', textAlign: 'left', fontSize: '11px', fontWeight: 700, color: '#9CA3AF', textTransform: 'uppercase' }}>Logged At</th>
                </tr>
              </thead>
              <tbody>
                {proposal.skipLogs.map((log) => (
                  <tr key={log.logId} style={{ borderBottom: '1px solid #F3F4F6' }}>
                    <td style={{ padding: '10px 16px', fontSize: '13px', color: '#374151' }}>
                     
                      {new Date(log.eventDate + 'T00:00:00').toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                    </td>
                    <td style={{ padding: '10px 16px', fontSize: '13px', color: '#374151', fontWeight: 500 }}>{log.taskName}</td>
                    <td style={{ padding: '10px 16px', fontSize: '13px', color: '#991B1B' }}>{log.reason}</td>
                    <td style={{ padding: '10px 16px', fontSize: '12px', color: '#9CA3AF' }}>
                      {new Date(log.loggedAt).toLocaleString('en-GB', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })}
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

export default ProposalDetailPage;
