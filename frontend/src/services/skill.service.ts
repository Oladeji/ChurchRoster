import apiService from './api.service';
import type { Skill } from '../types';

class SkillService {
  async getSkills(): Promise<Skill[]> {
    return await apiService.get<Skill[]>('/skills');
  }

  async getSkillById(id: number): Promise<Skill> {
    return await apiService.get<Skill>(`/skills/${id}`);
  }

  async createSkill(skill: Partial<Skill>): Promise<Skill> {
    return await apiService.post<Skill>('/skills', skill);
  }

  async updateSkill(id: number, skill: Partial<Skill>): Promise<Skill> {
    return await apiService.put<Skill>(`/skills/${id}`, skill);
  }

  async deleteSkill(id: number): Promise<void> {
    await apiService.delete(`/skills/${id}`);
  }
}

export default new SkillService();
