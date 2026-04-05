import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import assignmentService from '../services/assignment.service';
import type { Assignment } from '../types';
import { ClipboardDocumentListIcon, CalendarIcon, ChevronLeftIcon, PlusIcon, TrashIcon, ArchiveBoxIcon, ClockIcon, CheckCircleIcon, XCircleIcon, QuestionMarkCircleIcon, CheckBadgeIcon, NoSymbolIcon } from '@heroicons/react/24/solid';

const AssignmentsPage: React.FC = () => {
  const navigate = useNavigate();
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoadingId, setActionLoadingId] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  useEffect(() => {
    loadAssignments();
  }, []);

  const loadAssignments = async () => {
    try {
      setLoading(true);
      const data = await assignmentService.getAssignments();
      setAssignments(data);
    } catch (err) {
      setError('Failed to load assignments');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteAssignment = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this assignment?')) return;
    try {
      await assignmentService.deleteAssignment(id);
      loadAssignments();
    } catch (err) {
      setError('Failed to delete assignment');
      console.error(err);
    }
  };

  const handleSendReminder = async (id: number) => {
    try {
      setError('');
      setActionLoadingId(id);
      const result = await assignmentService.sendReminder(id);
      alert(result.message || 'Reminder sent successfully');
    } catch (err) {
      setError('Failed to send reminder');
      console.error(err);
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleRevokeAssignment = async (id: number) => {
    const reason = window.prompt('Enter a reason for revoking this pending assignment:', 'Assignment withdrawn by admin');

    if (reason === null) return;

    try {
      setError('');
      setActionLoadingId(id);
      const result = await assignmentService.revokeAssignment(id, reason.trim() || 'Assignment withdrawn by admin');
      alert(result.message || 'Assignment revoked successfully');
      await loadAssignments();
    } catch (err) {
      setError('Failed to revoke assignment');
      console.error(err);
    } finally {
      setActionLoadingId(null);
    }
  };

  const filteredAssignments = useMemo(() => {
    if (filterStatus === 'all') return assignments;
    return assignments.filter(a => a.status === filterStatus);
  }, [assignments, filterStatus]);

  const summaryStats = useMemo(() => ({
    total: assignments.length,
    pending: assignments.filter(a => a.status === 'Pending').length,
    accepted: assignments.filter(a => a.status === 'Accepted').length,
    rejected: assignments.filter(a => a.status === 'Rejected').length,
    confirmed: assignments.filter(a => a.status === 'Confirmed').length,
    completed: assignments.filter(a => a.status === 'Completed').length,
  }), [assignments]);

  const formatDate = (dateString: string) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric', timeZone: 'UTC' });
  };

  const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
    const statusConfig: Record<string, { icon: React.ElementType, color: string, bg: string }> = {
      'Pending': { icon: ClockIcon, color: '#92400e', bg: '#fef3c7' },
      'Accepted': { icon: CheckCircleIcon, color: '#065f46', bg: '#d1fae5' },
      'Rejected': { icon: XCircleIcon, color: '#991b1b', bg: '#fee2e2' },
      'Confirmed': { icon: CheckBadgeIcon, color: '#1e40af', bg: '#dbeafe' },
      'Completed': { icon: ArchiveBoxIcon, color: '#5b21b6', bg: '#e9d5ff' },
      'Expired': { icon: NoSymbolIcon, color: '#374151', bg: '#e5e7eb' },
    };
    const config = statusConfig[status] || { icon: QuestionMarkCircleIcon, color: '#6b7280', bg: '#f3f4f6' };
    const Icon = config.icon;
    return (
      <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', padding: '4px 10px', borderRadius: '9999px', fontSize: '12px', fontWeight: '500', background: config.bg, color: config.color }}>
        <Icon style={{ width: '16px', height: '16px' }} />{status}
      </span>
    );
  };

  const SummaryCard: React.FC<{ title: string, value: number, icon: React.ElementType, color: string }> = ({ title, value, icon: Icon, color }) => (
    <div style={{ background: 'white', padding: '24px', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', display: 'flex', alignItems: 'center', gap: '20px' }}>
      <div style={{ width: '48px', height: '48px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', background: color }}>
        <Icon style={{ width: '24px', height: '24px', color: 'white' }} />
      </div>
      <div>
        <p style={{ fontSize: '14px', fontWeight: '500', color: '#6b7280', margin: 0 }}>{title}</p>
        <p style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: '4px 0 0 0' }}>{value}</p>
      </div>
    </div>
  );

  if (loading) {
    return (
      <div style={{ minHeight: '100vh', background: '#f9fafb', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{ width: '48px', height: '48px', border: '3px solid #e5e7eb', borderTop: '3px solid #7c3aed', borderRadius: '50%', animation: 'spin 1s linear infinite', margin: '0 auto' }}></div>
          <p style={{ marginTop: '16px', color: '#6b7280' }}>Loading assignments...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ minHeight: '100vh', background: '#f9fafb', padding: '32px' }}>
      <div style={{ maxWidth: '1280px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px', flexWrap: 'wrap', gap: '16px' }}>
          <div>
            <h1 style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', display: 'flex', alignItems: 'center', gap: '12px', margin: 0 }}>
              <ClipboardDocumentListIcon style={{ width: '32px', height: '32px', color: '#7c3aed' }} />
              Assignments Management
            </h1>
            <p style={{ marginTop: '4px', color: '#6b7280' }}>View and manage all ministry task assignments</p>
          </div>
          <div style={{ display: 'flex', gap: '12px' }}>
            <button onClick={() => navigate('/dashboard')} style={{ background: 'white', color: '#374151', fontWeight: '600', padding: '8px 16px', borderRadius: '8px', border: '1px solid #d1d5db', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', fontSize: '16px' }}>
              <ChevronLeftIcon style={{ width: '20px', height: '20px' }} />Back
            </button>
            <button onClick={() => navigate('/calendar')} style={{ background: '#7c3aed', color: 'white', fontWeight: '600', padding: '8px 16px', borderRadius: '8px', border: 'none', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', fontSize: '16px' }}>
              <CalendarIcon style={{ width: '20px', height: '20px' }} />Calendar
            </button>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '24px', marginBottom: '32px' }}>
          <SummaryCard title="Total" value={summaryStats.total} icon={ClipboardDocumentListIcon} color="#3b82f6" />
          <SummaryCard title="Pending" value={summaryStats.pending} icon={ClockIcon} color="#f59e0b" />
          <SummaryCard title="Accepted" value={summaryStats.accepted} icon={CheckCircleIcon} color="#10b981" />
          <SummaryCard title="Rejected" value={summaryStats.rejected} icon={XCircleIcon} color="#ef4444" />
          <SummaryCard title="Confirmed" value={summaryStats.confirmed} icon={CheckBadgeIcon} color="#6366f1" />
          <SummaryCard title="Completed" value={summaryStats.completed} icon={ArchiveBoxIcon} color="#7c3aed" />
        </div>

        <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
          <div style={{ padding: '16px', borderBottom: '1px solid #e5e7eb', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}>
            <h2 style={{ fontSize: '18px', fontWeight: '600', color: '#1f2937', margin: 0 }}>All Assignments</h2>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
              <label style={{ fontSize: '14px', fontWeight: '500', color: '#6b7280' }}>Filter:</label>
              <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)} style={{ background: '#f9fafb', border: '1px solid #d1d5db', borderRadius: '8px', padding: '8px 12px', fontSize: '14px', cursor: 'pointer' }}>
                <option value="all">All</option>
                <option value="Pending">Pending</option>
                <option value="Accepted">Accepted</option>
                <option value="Rejected">Rejected</option>
                <option value="Confirmed">Confirmed</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
          </div>

          {error && <div style={{ margin: '16px', background: '#fef2f2', color: '#991b1b', padding: '16px', borderRadius: '8px' }}>{error}</div>}

          <div style={{ overflowX: 'auto' }}>
            {filteredAssignments.length === 0 ? (
              <div style={{ textAlign: 'center', padding: '64px 24px' }}>
                <ClipboardDocumentListIcon style={{ width: '64px', height: '64px', margin: '0 auto', color: '#d1d5db' }} />
                <h3 style={{ marginTop: '16px', fontSize: '20px', fontWeight: '600', color: '#1f2937' }}>No Assignments</h3>
                <p style={{ marginTop: '4px', color: '#6b7280' }}>{filterStatus === 'all' ? 'No assignments yet' : `No ${filterStatus} assignments`}</p>
                {filterStatus === 'all' && (
                  <button onClick={() => navigate('/calendar')} style={{ marginTop: '24px', background: '#7c3aed', color: 'white', fontWeight: '600', padding: '8px 20px', borderRadius: '8px', border: 'none', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
                    <PlusIcon style={{ width: '20px', height: '20px' }} />Create Assignment
                  </button>
                )}
              </div>
            ) : (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead style={{ background: '#f9fafb' }}>
                  <tr>
                    <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Task</th>
                    <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Member</th>
                    <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Date</th>
                    <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Status</th>
                    <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Assigned By</th>
                    <th style={{ padding: '12px 24px', textAlign: 'right', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredAssignments.map((a) => (
                    <tr key={a.assignmentId} style={{ borderTop: '1px solid #e5e7eb' }}>
                      <td style={{ padding: '16px 24px' }}>
                        <div style={{ fontWeight: '600', color: '#1f2937' }}>{a.taskName || 'Unknown'}</div>
                        <div style={{ color: '#6b7280', fontSize: '12px' }}>{a.task?.frequency || ''}</div>
                      </td>
                      <td style={{ padding: '16px 24px' }}>
                        <div style={{ fontWeight: '600', color: '#1f2937' }}>{a.userName || 'Unknown'}</div>
                      </td>
                      <td style={{ padding: '16px 24px', color: '#374151' }}>{formatDate(a.eventDate)}</td>
                      <td style={{ padding: '16px 24px' }}>
                        <StatusBadge status={a.status} />
                        {a.isOverride && <span style={{ marginLeft: '8px', padding: '2px 8px', borderRadius: '9999px', fontSize: '12px', background: '#fed7aa', color: '#92400e' }}>Override</span>}
                      </td>
                      <td style={{ padding: '16px 24px', color: '#6b7280' }}>{a.assignedByName || 'Admin'}</td>
                      <td style={{ padding: '16px 24px', textAlign: 'right' }}>
                        <div style={{ display: 'inline-flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap', justifyContent: 'flex-end' }}>
                          <button
                            onClick={() => handleSendReminder(a.assignmentId)}
                            disabled={actionLoadingId === a.assignmentId}
                            style={{
                              background: '#dbeafe',
                              color: '#1d4ed8',
                              border: 'none',
                              borderRadius: '6px',
                              padding: '6px 10px',
                              cursor: actionLoadingId === a.assignmentId ? 'not-allowed' : 'pointer',
                              fontSize: '12px',
                              fontWeight: '600',
                              opacity: actionLoadingId === a.assignmentId ? 0.7 : 1
                            }}>
                            {actionLoadingId === a.assignmentId ? 'Working...' : 'Send Reminder'}
                          </button>
                          {a.status === 'Pending' && (
                            <button
                              onClick={() => handleRevokeAssignment(a.assignmentId)}
                              disabled={actionLoadingId === a.assignmentId}
                              style={{
                                background: '#fee2e2',
                                color: '#b91c1c',
                                border: 'none',
                                borderRadius: '6px',
                                padding: '6px 10px',
                                cursor: actionLoadingId === a.assignmentId ? 'not-allowed' : 'pointer',
                                fontSize: '12px',
                                fontWeight: '600',
                                opacity: actionLoadingId === a.assignmentId ? 0.7 : 1
                              }}>
                              Revoke
                            </button>
                          )}
                          <button onClick={() => handleDeleteAssignment(a.assignmentId)} style={{ color: '#dc2626', background: 'none', border: 'none', cursor: 'pointer', padding: '4px' }}>
                            <TrashIcon style={{ width: '20px', height: '20px' }} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AssignmentsPage;
