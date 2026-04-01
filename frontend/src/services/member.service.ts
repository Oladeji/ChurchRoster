import apiService from './api.service';
import type { User, Skill, UserSkill } from '../types';

class MemberService {
  async getMembers(): Promise<User[]> {
    return await apiService.get<User[]>('/members');
  }

  async getMemberById(id: number): Promise<User> {
    return await apiService.get<User>(`/members/${id}`);
  }

  async createMember(member: Partial<User>): Promise<User> {
    return await apiService.post<User>('/members', member);
  }

  async updateMember(id: number, member: Partial<User>): Promise<User> {
    return await apiService.put<User>(`/members/${id}`, member);
  }

  async deleteMember(id: number): Promise<void> {
    await apiService.delete(`/members/${id}`);
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
