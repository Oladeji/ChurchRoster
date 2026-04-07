// User Types
export interface User {
  userId: number;
  tenantId?: number;
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
  taskName: string;
  userId: number;
  userName: string;
  eventDate: string;
  status: AssignmentStatus;
  rejectionReason?: string;
  isOverride: boolean;
  assignedBy: number;
  assignedByName: string;
  createdAt: string;
  // Legacy optional nested objects (deprecated, use flat properties above)
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
  tenantId: number;
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantId: number;
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  tenantId: number;
  name: string;
  email: string;
  role: 'Admin' | 'Member';
  token: string;
  expiresAt: string;
}

export interface Tenant {
  tenantId: number;
  name: string;
  slug: string;
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
