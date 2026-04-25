import React, { useState, useEffect, useRef } from 'react';
import assignmentService from '../services/assignment.service';
import type { Assignment } from '../types';

interface CalendarProps {
  onDateClick?: (date: Date) => void;
  onAssignmentClick?: (assignment: Assignment) => void;
  userId?: number; // If provided, only show this user's assignments
  currentMonth?: Date;
  onMonthChange?: (date: Date) => void;
}

const Calendar: React.FC<CalendarProps> = ({ onDateClick, onAssignmentClick, userId, currentMonth, onMonthChange }) => {
  const [currentDate, setCurrentDate] = useState(currentMonth ?? new Date());
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(true);
  const prevCurrentMonthRef = useRef<Date | undefined>(currentMonth);

  // Sync external currentMonth prop into internal state only when the
  // parent actually navigates to a different month (avoids infinite loop
  // from new Date object references on every parent render).
  useEffect(() => {
    const prev = prevCurrentMonthRef.current;
    prevCurrentMonthRef.current = currentMonth;
    if (
      currentMonth &&
      (currentMonth.getFullYear() !== (prev?.getFullYear() ?? -1) ||
        currentMonth.getMonth() !== (prev?.getMonth() ?? -1))
    ) {
      setCurrentDate(new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1));
    }
  }, [currentMonth]);

  useEffect(() => {
    loadAssignments();
  }, [currentDate, userId]);

  const loadAssignments = async () => {
    try {
      setLoading(true);
      const firstDay = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
      const lastDay = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);

      const filter = {
        startDate: firstDay.toISOString().split('T')[0],
        endDate: lastDay.toISOString().split('T')[0],
        ...(userId && { userId })
      };

      const data = await assignmentService.getAssignments(filter);
      setAssignments(data);
    } catch (error) {
      console.error('Failed to load assignments:', error);
    } finally {
      setLoading(false);
    }
  };

  const getDaysInMonth = () => {
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    const days: (Date | null)[] = [];

    // Add empty cells for days before the month starts
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(null);
    }

    // Add all days in the month
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(new Date(year, month, day));
    }

    return days;
  };

  const getAssignmentsForDate = (date: Date | null): Assignment[] => {
    if (!date) return [];
    const dateString = date.toISOString().split('T')[0];
    return assignments.filter(a => a.eventDate.split('T')[0] === dateString);
  };

  const previousMonth = () => {
    const newDate = new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);
    setCurrentDate(newDate);
    onMonthChange?.(newDate);
  };

  const nextMonth = () => {
    const newDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);
    setCurrentDate(newDate);
    onMonthChange?.(newDate);
  };

  const goToToday = () => {
    const newDate = new Date();
    setCurrentDate(newDate);
    onMonthChange?.(newDate);
  };

  const isToday = (date: Date | null): boolean => {
    if (!date) return false;
    const today = new Date();
    return date.toDateString() === today.toDateString();
  };

  const isPastDate = (date: Date | null): boolean => {
    if (!date) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const compareDate = new Date(date);
    compareDate.setHours(0, 0, 0, 0);
    return compareDate < today;
  };

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  const days = getDaysInMonth();

  return (
    <div className="calendar-container">
      <div className="calendar-header">
        <button onClick={previousMonth} className="btn-calendar-nav">‹</button>
        <h2>
          {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
        </h2>
        <button onClick={nextMonth} className="btn-calendar-nav">›</button>
        <button onClick={goToToday} className="btn-today">Today</button>
      </div>

      {loading ? (
        <div className="calendar-loading">Loading calendar...</div>
      ) : (
        <div className="calendar-grid">
          <div className="calendar-days-header">
            {dayNames.map(day => (
              <div key={day} className="calendar-day-name">
                {day}
              </div>
            ))}
          </div>

          <div className="calendar-days">
            {days.map((day, index) => {
              const dayAssignments = getAssignmentsForDate(day);
              const isCurrentDay = isToday(day);
              const isPast = isPastDate(day);

              return (
                <div
                  key={index}
                  className={`calendar-day ${!day ? 'empty' : ''} ${isCurrentDay ? 'today' : ''} ${isPast ? 'past' : ''}`}
                  onClick={() => day && onDateClick?.(day)}
                >
                  {day && (
                    <>
                      <div className="calendar-day-number">{day.getDate()}</div>
                      {dayAssignments.length > 0 && (
                        <div className="calendar-assignments">
                          {dayAssignments.map(assignment => (
                            <div
                              key={assignment.assignmentId}
                              className={`calendar-assignment status-${assignment.status.toLowerCase()}`}
                              onClick={(e) => {
                                e.stopPropagation();
                                onAssignmentClick?.(assignment);
                              }}
                              title={
                                assignment.status === 'Rejected' && assignment.rejectionReason
                                  ? `Rejected: ${assignment.rejectionReason}`
                                  : `${assignment.taskName || 'Task'} - ${assignment.status}`
                              }
                            >
                              <div className="assignment-task">
                                {assignment.status === 'Rejected' && '✗ '}
                                {assignment.taskName || assignment.task?.taskName || 'Task'}
                              </div>
                              {(assignment.userName || assignment.user) && (
                                <div className="assignment-user">{assignment.userName || assignment.user?.name}</div>
                              )}
                              {assignment.status === 'Rejected' && assignment.rejectionReason && (
                                <div className="assignment-rejection-icon" title={assignment.rejectionReason}>
                                  ⓘ
                                </div>
                              )}
                            </div>
                          ))}
                        </div>
                      )}
                    </>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};

export default Calendar;
