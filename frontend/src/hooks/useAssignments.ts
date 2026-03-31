import { useState, useEffect } from 'react';
import assignmentService from '../services/assignment.service';
import { Assignment, AssignmentFilter } from '../types';

export const useAssignments = (filter?: AssignmentFilter) => {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAssignments = async () => {
    try {
      setLoading(true);
      const data = await assignmentService.getAssignments(filter);
      setAssignments(data);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Failed to fetch assignments');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAssignments();
  }, [JSON.stringify(filter)]);

  return { assignments, loading, error, refetch: fetchAssignments };
};
