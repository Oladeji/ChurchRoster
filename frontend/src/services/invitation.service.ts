import apiService from './api.service';

export interface SendInvitationRequest {
  email: string;
  name: string;
  phone: string;
  role: 'Admin' | 'Member';
}

export interface InvitationDto {
  invitationId: number;
  email: string;
  name: string;
  phone: string;
  role: string;
  token: string;
  createdAt: string;
  expiresAt: string;
  isUsed: boolean;
  usedAt?: string;
  userId?: number;
  createdBy: number;
  createdByName?: string;
}

export interface VerifyInvitationResponse {
  isValid: boolean;
  email: string;
  name: string;
  phone: string;
  role: string;
  expiresAt: string;
  message?: string;
}

export interface AcceptInvitationRequest {
  token: string;
  password: string;
}

const invitationService = {
  sendInvitation: async (request: SendInvitationRequest): Promise<InvitationDto> => {
    return await apiService.post<InvitationDto>('/invitations/send', request);
  },

  verifyToken: async (token: string): Promise<VerifyInvitationResponse> => {
    return await apiService.get<VerifyInvitationResponse>(`/invitations/verify/${token}`);
  },

  acceptInvitation: async (request: AcceptInvitationRequest): Promise<void> => {
    await apiService.post('/invitations/accept', request);
  },

  getAllInvitations: async (): Promise<InvitationDto[]> => {
    return await apiService.get<InvitationDto[]>('/invitations');
  },

  getPendingInvitations: async (): Promise<InvitationDto[]> => {
    return await apiService.get<InvitationDto[]>('/invitations/pending');
  },

  cancelInvitation: async (invitationId: number): Promise<void> => {
    await apiService.delete(`/invitations/${invitationId}`);
  },
};

export default invitationService;
