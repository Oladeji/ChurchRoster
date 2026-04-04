import apiService from './api.service';
import type { User, Skill, UserSkill } from '../types';

class MemberService {
  async getMembers(): Promise<User[]> {
    return await apiService.get<User[]>('/members');
  }

  // Alias for convenience
  async getAll(): Promise<User[]> {
    return this.getMembers();
  }

  async getMemberById(id: number): Promise<User> {
    return await apiService.get<User>(`/members/${id}`);
  }

  // Alias for convenience
  async getById(id: number): Promise<User> {
    return this.getMemberById(id);
  }

  async createMember(member: Partial<User>): Promise<User> {
    return await apiService.post<User>('/members', member);
  }

  // Alias for convenience
  async create(member: Partial<User>): Promise<User> {
    return this.createMember(member);
  }

  async updateMember(id: number, member: Partial<User>): Promise<User> {
    return await apiService.put<User>(`/members/${id}`, member);
  }

  // Alias for convenience
  async update(id: number, member: Partial<User>): Promise<User> {
    return this.updateMember(id, member);
  }

  async deleteMember(id: number): Promise<void> {
    await apiService.delete(`/members/${id}`);
  }

  // Alias for convenience
  async delete(id: number): Promise<void> {
    return this.deleteMember(id);
  }

  async getMemberSkills(memberId: number): Promise<Skill[]> {
    return await apiService.get<Skill[]>(`/members/${memberId}/skills`);
  }

  async assignSkillToMember(memberId: number, skillId: number): Promise<UserSkill> {
    return await apiService.post<UserSkill>(`/members/${memberId}/skills`, { skillId });
  }

  async removeSkillFromMember(memberId: number, skillId: number): Promise<void> {
    await apiService.delete(`/members/${memberId}/skills/${skillId}`);
  }

  async getQualifiedMembers(taskId: number): Promise<User[]> {
    return await apiService.get<User[]>(`/members/qualified/${taskId}`);
  }
}

export default new MemberService();
