import apiService from './api.service';
import type { Task } from '../types';

class TaskService {
  async getTasks(): Promise<Task[]> {
    return await apiService.get<Task[]>('/tasks');
  }

  async getTaskById(id: number): Promise<Task> {
    return await apiService.get<Task>(`/tasks/${id}`);
  }

  async createTask(task: Partial<Task>): Promise<Task> {
    return await apiService.post<Task>('/tasks', task);
  }

  async updateTask(id: number, task: Partial<Task>): Promise<Task> {
    return await apiService.put<Task>(`/tasks/${id}`, task);
  }

  async deleteTask(id: number): Promise<void> {
    await apiService.delete(`/tasks/${id}`);
  }
}

export default new TaskService();
