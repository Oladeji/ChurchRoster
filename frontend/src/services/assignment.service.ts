import apiService from './api.service';
import type { Assignment, AssignmentFilter } from '../types';

class AssignmentService {
  async getAssignments(filter?: AssignmentFilter): Promise<Assignment[]> {
    // Use specific endpoints when filtering by userId or status
    if (filter?.userId && !filter.status && !filter.taskId) {
      return await apiService.get<Assignment[]>(`/assignments/user/${filter.userId}`);
    }

    if (filter?.status && !filter.userId && !filter.taskId) {
      return await apiService.get<Assignment[]>(`/assignments/status/${filter.status}`);
    }

    // For complex filters or no filters, use the main endpoint
    // Note: The backend GetAllAssignments doesn't support query params yet
    // For now, we'll fetch all and filter client-side if needed
    const allAssignments = await apiService.get<Assignment[]>('/assignments');

    if (!filter) return allAssignments;

    // Client-side filtering
    return allAssignments.filter(assignment => {
      if (filter.userId && assignment.userId !== filter.userId) return false;
      if (filter.taskId && assignment.taskId !== filter.taskId) return false;
      if (filter.status && assignment.status !== filter.status) return false;
      if (filter.startDate) {
        const eventDate = new Date(assignment.eventDate);
        const startDate = new Date(filter.startDate);
        if (eventDate < startDate) return false;
      }
      if (filter.endDate) {
        const eventDate = new Date(assignment.eventDate);
        const endDate = new Date(filter.endDate);
        if (eventDate > endDate) return false;
      }
      return true;
    });
  }

  async getAssignmentById(id: number): Promise<Assignment> {
    return await apiService.get<Assignment>(`/assignments/${id}`);
  }

  async createAssignment(assignment: Partial<Assignment>): Promise<Assignment> {
    return await apiService.post<Assignment>('/assignments', assignment);
  }

  async updateAssignment(id: number, assignment: Partial<Assignment>): Promise<Assignment> {
    return await apiService.put<Assignment>(`/assignments/${id}`, assignment);
  }

  async deleteAssignment(id: number): Promise<void> {
    await apiService.delete(`/assignments/${id}`);
  }

  async acceptAssignment(id: number): Promise<Assignment> {
    return await apiService.put<Assignment>(`/assignments/${id}/status`, {
      status: 'Accepted',
      rejectionReason: null
    });
  }

  async rejectAssignment(id: number, reason: string): Promise<Assignment> {
    return await apiService.put<Assignment>(`/assignments/${id}/status`, {
      status: 'Rejected',
      rejectionReason: reason
    });
  }

  async getMyAssignments(): Promise<Assignment[]> {
    return await apiService.get<Assignment[]>('/assignments/my');
  }
}

export default new AssignmentService();
