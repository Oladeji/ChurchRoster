import React, { useState, useEffect } from 'react';
import memberService from '../services/member.service';
import taskService from '../services/task.service';
import assignmentService from '../services/assignment.service';
import type { User, Task, Assignment } from '../types';

interface AssignmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  selectedDate?: Date;
  editAssignment?: Assignment;
}

const AssignmentModal: React.FC<AssignmentModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  selectedDate,
  editAssignment
}) => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [members, setMembers] = useState<User[]>([]);
  const [filteredMembers, setFilteredMembers] = useState<User[]>([]);
  const [selectedTaskId, setSelectedTaskId] = useState<number | ''>('');
  const [selectedUserId, setSelectedUserId] = useState<number | ''>('');
  const [eventDate, setEventDate] = useState('');
  const [isOverride, setIsOverride] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [validationWarnings, setValidationWarnings] = useState<string[]>([]);

  useEffect(() => {
    if (isOpen) {
      loadData();
      if (selectedDate) {
        setEventDate(selectedDate.toISOString().split('T')[0]);
      }
      if (editAssignment) {
        setSelectedTaskId(editAssignment.taskId);
        setSelectedUserId(editAssignment.userId);
        setEventDate(editAssignment.eventDate.split('T')[0]);
        setIsOverride(editAssignment.isOverride);
      }
    }
  }, [isOpen, selectedDate, editAssignment]);

  useEffect(() => {
    filterMembersByQualification();
  }, [selectedTaskId, members]);

  const loadData = async () => {
    try {
      const [tasksData, membersData] = await Promise.all([
        taskService.getTasks(),
        memberService.getAll()
      ]);
      setTasks(tasksData.filter(t => t.taskId > 0)); // Filter out any invalid tasks
      setMembers(membersData.filter(m => m.isActive));
    } catch (err) {
      setError('Failed to load tasks and members');
      console.error(err);
    }
  };

  const filterMembersByQualification = async () => {
    if (!selectedTaskId) {
      setFilteredMembers(members);
      setValidationWarnings([]);
      return;
    }

    const selectedTask = tasks.find(t => t.taskId === selectedTaskId);
    if (!selectedTask) {
      setFilteredMembers(members);
      setValidationWarnings([]);
      return;
    }

    // If task doesn't require a skill, show all active members
    if (!selectedTask.requiredSkillId) {
      setFilteredMembers(members);
      setValidationWarnings([]);
      return;
    }

    // Use backend endpoint to get qualified members for the task
    try {
      const qualifiedMembers = await memberService.getQualifiedMembers(selectedTask.taskId);
      setFilteredMembers(qualifiedMembers);

      // Show warning if no qualified members
      if (qualifiedMembers.length === 0) {
        setValidationWarnings([
          `No members are qualified for this task (requires: ${selectedTask.requiredSkill?.skillName || 'specific skill'})`
        ]);
      } else {
        setValidationWarnings([]);
      }
    } catch (err) {
      console.error('Failed to load qualified members:', err);
      // Fallback to all members if the endpoint fails
      setFilteredMembers(members);
      setValidationWarnings([]);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setValidationWarnings([]);

    if (!selectedTaskId || !selectedUserId || !eventDate) {
      setError('Please fill in all required fields');
      return;
    }

    try {
      setLoading(true);

      const assignmentData = {
        taskId: Number(selectedTaskId),
        userId: Number(selectedUserId),
        eventDate: new Date(eventDate).toISOString(),
        isOverride
      };

      if (editAssignment) {
        await assignmentService.updateAssignment(editAssignment.assignmentId, assignmentData);
      } else {
        await assignmentService.createAssignment(assignmentData);
      }

      onSuccess();
      handleClose();
    } catch (err: any) {
      console.error('Failed to save assignment:', err);
      if (err.response?.data?.errors) {
        setError(err.response.data.errors.join(', '));
      } else if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Failed to save assignment. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setSelectedTaskId('');
    setSelectedUserId('');
    setEventDate('');
    setIsOverride(false);
    setError('');
    setValidationWarnings([]);
    onClose();
  };

  if (!isOpen) return null;

  const selectedTask = tasks.find(t => t.taskId === selectedTaskId);
  const showQualificationWarning = selectedTask?.isRestricted && selectedTask?.requiredSkillId;

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content assignment-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{editAssignment ? 'Edit Assignment' : 'Create Assignment'}</h2>
          <button className="modal-close" onClick={handleClose}>×</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="task">Task *</label>
            <select
              id="task"
              value={selectedTaskId}
              onChange={(e) => setSelectedTaskId(e.target.value ? Number(e.target.value) : '')}
              required
              disabled={loading}
            >
              <option value="">Select a task...</option>
              {tasks.map(task => (
                <option key={task.taskId} value={task.taskId}>
                  {task.taskName} ({task.frequency})
                  {task.isRestricted && task.requiredSkill && ` - Requires: ${task.requiredSkill.skillName}`}
                </option>
              ))}
            </select>
          </div>

          {showQualificationWarning && filteredMembers.length > 0 && (
            <div className="info-message">
              ℹ️ Only showing members qualified for this task ({filteredMembers.length} available)
            </div>
          )}

          {validationWarnings.length > 0 && (
            <div className="warning-message">
              {validationWarnings.map((warning, index) => (
                <div key={index}>⚠️ {warning}</div>
              ))}
            </div>
          )}

          <div className="form-group">
            <label htmlFor="member">Member *</label>
            <select
              id="member"
              value={selectedUserId}
              onChange={(e) => setSelectedUserId(e.target.value ? Number(e.target.value) : '')}
              required
              disabled={loading || filteredMembers.length === 0}
            >
              <option value="">Select a member...</option>
              {filteredMembers.map(member => (
                <option key={member.userId} value={member.userId}>
                  {member.name} ({member.email})
                </option>
              ))}
            </select>
            {filteredMembers.length === 0 && selectedTaskId && (
              <small style={{ color: '#EF4444' }}>
                No qualified members available. Enable override to assign anyway.
              </small>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="eventDate">Event Date *</label>
            <input
              type="date"
              id="eventDate"
              value={eventDate}
              onChange={(e) => setEventDate(e.target.value)}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group-checkbox">
            <label>
              <input
                type="checkbox"
                checked={isOverride}
                onChange={(e) => setIsOverride(e.target.checked)}
                disabled={loading}
              />
              <span>Override qualification check</span>
            </label>
            <small>Enable this to assign unqualified members or bypass validation rules</small>
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <div className="modal-actions">
            <button
              type="button"
              onClick={handleClose}
              className="btn-secondary"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={loading || (!isOverride && filteredMembers.length === 0)}
            >
              {loading ? 'Saving...' : editAssignment ? 'Update Assignment' : 'Create Assignment'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AssignmentModal;
