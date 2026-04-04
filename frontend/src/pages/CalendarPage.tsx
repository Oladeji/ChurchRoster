import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Calendar from '../components/Calendar';
import AssignmentModal from '../components/AssignmentModal';
import type { Assignment } from '../types';

const CalendarPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

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

  const handleModalSuccess = () => {
    setRefreshKey(prev => prev + 1); // Force calendar refresh
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

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>📅 Ministry Calendar</h1>
          <p>View and manage ministry assignments</p>
        </div>
        <div style={{ display: 'flex', gap: '12px' }}>
          <button
            onClick={() => navigate('/dashboard')}
            className="btn-secondary"
          >
            ← Back to Dashboard
          </button>
          <button
            onClick={() => {
              setSelectedDate(new Date());
              setSelectedAssignment(undefined);
              setIsModalOpen(true);
            }}
            className="btn-primary"
          >
            + New Assignment
          </button>
        </div>
      </div>

      <div className="calendar-page-content">
        <Calendar
          key={refreshKey}
          onDateClick={handleDateClick}
          onAssignmentClick={handleAssignmentClick}
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

              <div className="modal-actions" style={{ marginTop: '24px' }}>
                <button onClick={handleDetailsClose} className="btn-primary">
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
