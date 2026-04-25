import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Calendar from '../components/Calendar';
import AssignmentModal from '../components/AssignmentModal';
import assignmentService from '../services/assignment.service';
import type { Assignment } from '../types';

const CalendarPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  const [exporting, setExporting] = useState(false);
  const [displayedMonth, setDisplayedMonth] = useState(new Date());
  const [actionLoading, setActionLoading] = useState(false);

  const handleDateClick = (date: Date) => {
    setSelectedDate(date);
    setSelectedAssignment(undefined);
    setIsModalOpen(true);
  };

  const handleAssignmentClick = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setIsDetailsOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setSelectedDate(undefined);
    setSelectedAssignment(undefined);
  };

  const handleDetailsClose = () => {
    setIsDetailsOpen(false);
    setSelectedAssignment(undefined);
  };

  const handleAcceptOnBehalf = async () => {
    if (!selectedAssignment) return;
    setActionLoading(true);
    try {
      const updated = await assignmentService.acceptAssignment(selectedAssignment.assignmentId);
      setSelectedAssignment(updated);
      setRefreshKey(prev => prev + 1);
    } catch (error) {
      console.error('Failed to accept assignment:', error);
      alert('Failed to accept assignment. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancelOnBehalf = async () => {
    if (!selectedAssignment) return;
    setActionLoading(true);
    try {
      const updated = await assignmentService.rejectAssignment(
        selectedAssignment.assignmentId,
        'Cancelled by admin on behalf of member'
      );
      setSelectedAssignment(updated);
      setRefreshKey(prev => prev + 1);
    } catch (error) {
      console.error('Failed to cancel assignment:', error);
      alert('Failed to cancel assignment. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleModalSuccess = () => {
    setRefreshKey(prev => prev + 1); // Force calendar refresh
  };

  const getMonthRange = () => {
    const firstDay = new Date(displayedMonth.getFullYear(), displayedMonth.getMonth(), 1);
    const lastDay = new Date(displayedMonth.getFullYear(), displayedMonth.getMonth() + 1, 0);

    return {
      startDate: firstDay.toISOString().split('T')[0],
      endDate: lastDay.toISOString().split('T')[0],
      monthLabel: firstDay.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
    };
  };

  const getDisplayedMonthLabel = () => {
    return displayedMonth.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  };

  const handlePrintCalendar = () => {
    window.print();
  };

  const handleExportCalendar = async () => {
    try {
      setExporting(true);
      const { startDate, endDate, monthLabel } = getMonthRange();
      const assignments = await assignmentService.getAssignments({ startDate, endDate });

      const rows = [
        ['Task', 'Member', 'Date', 'Status', 'Assigned By'],
        ...assignments.map(a => [
          a.taskName || a.task?.taskName || '',
          a.userName || a.user?.name || '',
          formatDate(a.eventDate),
          a.status,
          a.assignedByName || 'Admin'
        ])
      ];

      const csv = rows
        .map(row => row.map(value => `"${String(value).replace(/"/g, '""')}"`).join(','))
        .join('\n');

      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `calendar_export_${monthLabel.replace(/\s+/g, '_')}.csv`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export calendar:', error);
      alert('Failed to export calendar');
    } finally {
      setExporting(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      timeZone: 'UTC'
    });
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

  const actionButtonBaseStyle: React.CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: '8px',
    padding: '12px 18px',
    borderRadius: '12px',
    border: '1px solid transparent',
    fontSize: '14px',
    fontWeight: 600,
    letterSpacing: '-0.01em',
    cursor: 'pointer',
    transition: 'all 0.2s ease',
    boxShadow: '0 8px 20px rgba(15, 23, 42, 0.08)'
  };

  const secondaryButtonStyle: React.CSSProperties = {
    ...actionButtonBaseStyle,
    background: 'white',
    color: '#1F2937',
    borderColor: '#D1D5DB'
  };

  const primaryButtonStyle: React.CSSProperties = {
    ...actionButtonBaseStyle,
    background: 'linear-gradient(135deg, #2563EB, #4F46E5)',
    color: 'white',
    boxShadow: '0 12px 24px rgba(79, 70, 229, 0.28)'
  };

  return (
    <div className="page-container calendar-print-page">
      <div className="page-header no-print">
        <div>
          <h1>📅 Ministry Calendar</h1>
          <p>View and manage ministry assignments</p>
        </div>
        <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center' }}>
          <button
            onClick={() => navigate('/dashboard')}
            className="btn-secondary"
            style={secondaryButtonStyle}
          >
            <span style={{ fontSize: '16px', lineHeight: 1 }}>←</span>
            <span>Back to Dashboard</span>
          </button>
          <button
            onClick={handlePrintCalendar}
            className="btn-secondary"
            style={secondaryButtonStyle}
          >
            <span style={{ fontSize: '16px', lineHeight: 1 }}>🖨️</span>
            <span>Print Calendar</span>
          </button>
          <button
            onClick={handleExportCalendar}
            className="btn-secondary"
            disabled={exporting}
            style={{
              ...secondaryButtonStyle,
              opacity: exporting ? 0.75 : 1,
              cursor: exporting ? 'not-allowed' : 'pointer'
            }}
          >
            <span style={{ fontSize: '16px', lineHeight: 1 }}>{exporting ? '⏳' : '📤'}</span>
            <span>{exporting ? 'Exporting...' : 'Export CSV'}</span>
          </button>
          <button
            onClick={() => {
              setSelectedDate(new Date());
              setSelectedAssignment(undefined);
              setIsModalOpen(true);
            }}
            className="btn-primary"
            style={primaryButtonStyle}
          >
            <span style={{ fontSize: '18px', lineHeight: 1 }}>+</span>
            <span>New Assignment</span>
          </button>
        </div>
      </div>

      <div className="calendar-page-content calendar-print-container">
        <div className="print-only calendar-print-title">
          <h1>Ministry Calendar - {getDisplayedMonthLabel()}</h1>
          <p>Printable ministry assignment calendar</p>
        </div>
        <Calendar
          key={refreshKey}
          onDateClick={handleDateClick}
          onAssignmentClick={handleAssignmentClick}
          currentMonth={displayedMonth}
          onMonthChange={setDisplayedMonth}
        />
      </div>

      <AssignmentModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        onSuccess={handleModalSuccess}
        selectedDate={selectedDate}
        editAssignment={selectedAssignment}
      />

      {/* Assignment Details Modal */}
      {isDetailsOpen && selectedAssignment && (
        <div className="modal-overlay" onClick={handleDetailsClose}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Assignment Details</h2>
              <button className="modal-close" onClick={handleDetailsClose}>×</button>
            </div>

            <div style={{ marginTop: '24px' }}>
              <div className="detail-item" style={{ marginBottom: '16px' }}>
                <span className="detail-label">Task:</span>
                <h3 style={{ margin: '4px 0', color: '#1F2937' }}>
                  {selectedAssignment.taskName || 'Unknown Task'}
                </h3>
              </div>

              <div className="detail-item" style={{ marginBottom: '16px' }}>
                <span className="detail-label">Assigned To:</span>
                <div style={{ fontWeight: '500', color: '#1F2937' }}>
                  {selectedAssignment.userName || 'Unknown User'}
                </div>
              </div>

              <div className="detail-item" style={{ marginBottom: '16px' }}>
                <span className="detail-label">Event Date:</span>
                <div style={{ fontWeight: '500', color: '#1F2937' }}>
                  {formatDate(selectedAssignment.eventDate)}
                </div>
              </div>

              <div className="detail-item" style={{ marginBottom: '16px' }}>
                <span className="detail-label">Status:</span>
                <div>
                  <span className={`badge ${getStatusBadgeClass(selectedAssignment.status)}`}>
                    {selectedAssignment.status}
                  </span>
                  {selectedAssignment.isOverride && (
                    <span className="badge badge-warning" style={{ marginLeft: '8px' }}>
                      Override
                    </span>
                  )}
                </div>
              </div>

              {selectedAssignment.status === 'Rejected' && selectedAssignment.rejectionReason && (
                <div className="rejection-reason" style={{ marginTop: '20px' }}>
                  <strong>Rejection Reason:</strong>
                  <div style={{ marginTop: '8px', fontSize: '15px' }}>
                    {selectedAssignment.rejectionReason}
                  </div>
                </div>
              )}

              <div className="detail-item" style={{ marginTop: '20px', paddingTop: '20px', borderTop: '1px solid #E5E7EB' }}>
                <span className="detail-label">Assigned By:</span>
                <div style={{ fontWeight: '500', color: '#1F2937' }}>
                  {selectedAssignment.assignedByName || 'Admin'}
                </div>
              </div>

              <div className="modal-actions" style={{ marginTop: '24px', display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
                {selectedAssignment.status === 'Pending' && (
                  <button
                    onClick={handleAcceptOnBehalf}
                    disabled={actionLoading}
                    className="btn-primary"
                    style={{ background: 'linear-gradient(135deg, #16A34A, #15803D)', boxShadow: '0 8px 20px rgba(22,163,74,0.25)', opacity: actionLoading ? 0.7 : 1 }}
                  >
                    {actionLoading ? '⏳ Processing...' : '✅ Accept on Behalf'}
                  </button>
                )}
                {(selectedAssignment.status === 'Pending' || selectedAssignment.status === 'Accepted') && (
                  <button
                    onClick={handleCancelOnBehalf}
                    disabled={actionLoading}
                    className="btn-secondary"
                    style={{ background: '#FEF2F2', color: '#991B1B', borderColor: '#FECACA', opacity: actionLoading ? 0.7 : 1 }}
                  >
                    {actionLoading ? '⏳ Processing...' : '❌ Cancel on Behalf'}
                  </button>
                )}
                <button onClick={handleDetailsClose} className="btn-secondary" style={{ marginLeft: 'auto' }}>
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CalendarPage;
