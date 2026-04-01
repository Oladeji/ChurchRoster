// User Types
export interface User {
  userId: number;
  name: string;
  email: string;
  phone: string;
  role: 'Admin' | 'Member';
  monthlyLimit?: number;
  deviceToken?: string;
  isActive: boolean;
  userSkills?: UserSkill[];
}

export interface UserSkill {
  userId: number;
  skillId: number;
  skill?: Skill;
  assignedDate: string;
}

// Skill Types
export interface Skill {
  skillId: number;
  skillName: string;
  description: string;
  isActive: boolean;
}

// Task Types
export interface Task {
  taskId: number;
  taskName: string;
  frequency: 'Weekly' | 'Monthly';
  dayRule: string;
  requiredSkillId?: number;
  isRestricted: boolean;
  requiredSkill?: Skill;
}

// Assignment Types
export interface Assignment {
  assignmentId: number;
  taskId: number;
  userId: number;
  eventDate: string;
  status: AssignmentStatus;
  rejectionReason?: string;
  isOverride: boolean;
  assignedBy: number;
  createdAt: string;
  updatedAt: string;
  task?: Task;
  user?: User;
}

export type AssignmentStatus = 
  | 'Pending' 
  | 'Accepted' 
  | 'Rejected' 
  | 'Confirmed' 
  | 'Completed' 
  | 'Expired';

// Auth Types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  name: string;
  email: string;
  role: 'Admin' | 'Member';
  token: string;
  expiresAt: string;
}

// API Response Types
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Calendar Types
export interface CalendarEvent {
  id: number;
  title: string;
  date: Date;
  assignment: Assignment;
}

// Filter Types
export interface AssignmentFilter {
  userId?: number;
  taskId?: number;
  status?: AssignmentStatus;
  startDate?: string;
  endDate?: string;
}
