import apiService from './api.service';
import type { AuthResponse, LoginRequest, RegisterRequest, Tenant, User } from '../types';

class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>('/auth/login', credentials);
    if (response.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('tenantId', response.tenantId.toString());
      localStorage.setItem('user', JSON.stringify({
        userId: response.userId,
        tenantId: response.tenantId,
        name: response.name,
        email: response.email,
        role: response.role,
        isActive: true
      }));
    }
    return response;
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await apiService.post<AuthResponse>('/auth/register', userData);
    if (response.token) {
      localStorage.setItem('authToken', response.token);
      localStorage.setItem('tenantId', response.tenantId.toString());
      localStorage.setItem('user', JSON.stringify({
        userId: response.userId,
        tenantId: response.tenantId,
        name: response.name,
        email: response.email,
        role: response.role,
        isActive: true
      }));
    }
    return response;
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('tenantId');
    localStorage.removeItem('user');
  }

  async getTenants(): Promise<Tenant[]> {
    return await apiService.get<Tenant[]>('/tenants');
  }

  getSelectedTenantId(): number | null {
    const tenantId = localStorage.getItem('tenantId');
    return tenantId ? Number(tenantId) : null;
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }
}

export default new AuthService();
