import apiService from './api.service';
import { Assignment, AssignmentFilter, ApiResponse } from '../types';

class AssignmentService {
  async getAssignments(filter?: AssignmentFilter): Promise<Assignment[]> {
    const params = new URLSearchParams();
    if (filter?.userId) params.append('userId', filter.userId.toString());
    if (filter?.taskId) params.append('taskId', filter.taskId.toString());
    if (filter?.status) params.append('status', filter.status);
    if (filter?.startDate) params.append('startDate', filter.startDate);
    if (filter?.endDate) params.append('endDate', filter.endDate);

    const queryString = params.toString();
    const url = queryString ? `/assignments?${queryString}` : '/assignments';

    return await apiService.get<Assignment[]>(url);
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
    return await apiService.patch<Assignment>(`/assignments/${id}/accept`);
  }

  async rejectAssignment(id: number, reason: string): Promise<Assignment> {
    return await apiService.patch<Assignment>(`/assignments/${id}/reject`, { reason });
  }

  async getMyAssignments(): Promise<Assignment[]> {
    return await apiService.get<Assignment[]>('/assignments/my');
  }
}

export default new AssignmentService();
