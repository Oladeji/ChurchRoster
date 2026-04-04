import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import assignmentService from '../services/assignment.service';
import type { Assignment } from '../types';

const MyAssignmentsPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('upcoming');

  useEffect(() => {
    loadMyAssignments();
  }, [filterStatus]);

  const loadMyAssignments = async () => {
    if (!user) return;

    try {
      setLoading(true);
      const filter = { userId: user.userId };
      const data = await assignmentService.getAssignments(filter);

      // Filter based on selected filter
      let filteredData = data;
      const today = new Date();

      // Get today's date in UTC for accurate comparison
      const todayUTC = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());

      if (filterStatus === 'upcoming') {
        filteredData = data.filter(a => {
          const eventDate = new Date(a.eventDate);
          const eventDateUTC = Date.UTC(eventDate.getUTCFullYear(), eventDate.getUTCMonth(), eventDate.getUTCDate());
          return eventDateUTC >= todayUTC && a.status !== 'Completed' && a.status !== 'Expired';
        });
      } else if (filterStatus === 'pending') {
        filteredData = data.filter(a => a.status === 'Pending');
      } else if (filterStatus === 'past') {
        filteredData = data.filter(a => {
          const eventDate = new Date(a.eventDate);
          const eventDateUTC = Date.UTC(eventDate.getUTCFullYear(), eventDate.getUTCMonth(), eventDate.getUTCDate());
          return eventDateUTC < todayUTC || a.status === 'Completed' || a.status === 'Expired';
        });
      }

      // Sort by event date (nearest first)
      filteredData.sort((a, b) => 
        new Date(a.eventDate).getTime() - new Date(b.eventDate).getTime()
      );

      setAssignments(filteredData);
    } catch (err) {
      setError('Failed to load your assignments');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleAcceptAssignment = async (id: number) => {
    try {
      await assignmentService.acceptAssignment(id);
      loadMyAssignments();
    } catch (err) {
      alert('Failed to accept assignment');
      console.error(err);
    }
  };

  const handleRejectAssignment = async (id: number) => {
    const reason = window.prompt('Please provide a reason for rejecting this assignment:');
    if (!reason) return;

    try {
      await assignmentService.rejectAssignment(id, reason);
      loadMyAssignments();
    } catch (err) {
      alert('Failed to reject assignment');
      console.error(err);
    }
  };

  const getStatusBadgeClass = (status: string) => {
    const statusMap: Record<string, string> = {
      'Pending': 'status-pending',
      'Accepted': 'status-accepted',
      'Rejected': 'status-rejected',
      'Confirmed': 'status-confirmed',
      'Completed': 'status-completed',
      'Expired': 'status-expired'
    };
    return statusMap[status] || 'status-pending';
  };

  const formatDate = (dateString: string) => {
    // Parse as UTC to avoid timezone shift
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      timeZone: 'UTC' // Force UTC timezone to show the actual date
    });
  };

  const getDaysUntil = (dateString: string) => {
    // Parse dates in UTC to avoid timezone issues
    const eventDate = new Date(dateString);
    const today = new Date();

    // Get UTC dates for comparison
    const eventDateUTC = Date.UTC(eventDate.getUTCFullYear(), eventDate.getUTCMonth(), eventDate.getUTCDate());
    const todayUTC = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());

    const diffTime = eventDateUTC - todayUTC;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>📝 My Assignments</h1>
          <p>View and respond to your ministry assignments</p>
        </div>
        <button
          onClick={() => navigate('/dashboard')}
          className="btn-secondary"
        >
          ← Back to Dashboard
        </button>
      </div>

      <div className="filter-bar">
        <div className="form-group" style={{ marginBottom: 0 }}>
          <label htmlFor="filterSelect">Show:</label>
          <select
            id="filterSelect"
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            style={{ width: '200px' }}
          >
            <option value="upcoming">Upcoming</option>
            <option value="pending">Pending Response</option>
            <option value="past">Past Assignments</option>
            <option value="all">All Assignments</option>
          </select>
        </div>
      </div>

      {error && (
        <div className="error-message">{error}</div>
      )}

      {loading ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
          Loading your assignments...
        </div>
      ) : assignments.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', color: '#666' }}>
          <p>No assignments found for this filter.</p>
        </div>
      ) : (
        <div className="assignments-list">
          {assignments.map((assignment) => {
            const daysUntil = getDaysUntil(assignment.eventDate);
            const isPending = assignment.status === 'Pending';

            return (
              <div key={assignment.assignmentId} className="assignment-card">
                <div className="assignment-card-header">
                  <div>
                    <h3>{assignment.taskName || assignment.task?.taskName || 'Unknown Task'}</h3>
                    <p className="assignment-date">{formatDate(assignment.eventDate)}</p>
                    {daysUntil >= 0 && (
                      <p className="assignment-countdown">
                        {daysUntil === 0 ? 'Today' : daysUntil === 1 ? 'Tomorrow' : `In ${daysUntil} days`}
                      </p>
                    )}
                  </div>
                  <div>
                    <span className={`badge ${getStatusBadgeClass(assignment.status)}`}>
                      {assignment.status}
                    </span>
                  </div>
                </div>

                <div className="assignment-card-body">
                  <div className="assignment-details">
                    {assignment.task?.frequency && (
                      <div className="detail-item">
                        <span className="detail-label">Frequency:</span>
                        <span className="detail-value">{assignment.task.frequency}</span>
                      </div>
                    )}
                    {assignment.task?.dayRule && (
                      <div className="detail-item">
                        <span className="detail-label">Day Rule:</span>
                        <span className="detail-value">{assignment.task.dayRule}</span>
                      </div>
                    )}
                    {assignment.task?.requiredSkill && (
                      <div className="detail-item">
                        <span className="detail-label">Required Skill:</span>
                        <span className="detail-value">{assignment.task.requiredSkill.skillName}</span>
                      </div>
                    )}
                  </div>

                  {assignment.rejectionReason && (
                    <div className="rejection-reason">
                      <strong>Rejection Reason:</strong> {assignment.rejectionReason}
                    </div>
                  )}

                  {isPending && (
                    <div className="assignment-actions">
                      <button
                        onClick={() => handleAcceptAssignment(assignment.assignmentId)}
                        className="btn-primary"
                      >
                        ✓ Accept
                      </button>
                      <button
                        onClick={() => handleRejectAssignment(assignment.assignmentId)}
                        className="btn-danger"
                      >
                        ✗ Reject
                      </button>
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {assignments.length > 0 && (
        <div style={{ marginTop: '24px', padding: '16px', background: '#F3F4F6', borderRadius: '8px' }}>
          <h3 style={{ marginTop: 0 }}>📊 Summary</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '16px' }}>
            <div>
              <div style={{ fontSize: '0.875rem', color: '#666' }}>Total Showing</div>
              <div style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>{assignments.length}</div>
            </div>
            <div>
              <div style={{ fontSize: '0.875rem', color: '#666' }}>Pending Response</div>
              <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#F59E0B' }}>
                {assignments.filter(a => a.status === 'Pending').length}
              </div>
            </div>
            <div>
              <div style={{ fontSize: '0.875rem', color: '#666' }}>Accepted</div>
              <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#10B981' }}>
                {assignments.filter(a => a.status === 'Accepted').length}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyAssignmentsPage;
