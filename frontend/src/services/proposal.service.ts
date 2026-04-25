import apiService from './api.service';
import type {
  ProposalSummary,
  ProposalDetail,
  GenerateProposalRequest,
  UpdateProposalItemRequest,
  AddProposalItemRequest,
  PublishResult,
} from '../types';

const BASE = '/v1/proposals';

const proposalService = {
  getAll(): Promise<ProposalSummary[]> {
    return apiService.get<ProposalSummary[]>(BASE);
  },

  getById(id: number): Promise<ProposalDetail> {
    return apiService.get<ProposalDetail>(`${BASE}/${id}`);
  },

  generate(req: GenerateProposalRequest): Promise<{ proposalId: number }> {
    return apiService.post<{ proposalId: number }>(BASE, req);
  },

  updateItem(proposalId: number, itemId: number, req: UpdateProposalItemRequest): Promise<void> {
    return apiService.patch<void>(`${BASE}/${proposalId}/items/${itemId}`, req);
  },

  addItem(proposalId: number, req: AddProposalItemRequest): Promise<ProposalDetail> {
    return apiService.post<ProposalDetail>(`${BASE}/${proposalId}/items`, req);
  },

  deleteItem(proposalId: number, itemId: number): Promise<void> {
    return apiService.delete<void>(`${BASE}/${proposalId}/items/${itemId}`);
  },

  publish(proposalId: number): Promise<PublishResult> {
    return apiService.post<PublishResult>(`${BASE}/${proposalId}/publish`);
  },

  archive(proposalId: number): Promise<void> {
    return apiService.post<void>(`${BASE}/${proposalId}/archive`);
  },

  async downloadDraftPdf(proposalId: number, proposalName: string): Promise<void> {
    const token = localStorage.getItem('authToken');
    const tenantId = localStorage.getItem('tenantId');
    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

    const response = await fetch(`${apiUrl}/v1/proposals/${proposalId}/pdf`, {
      headers: {
        Authorization: `Bearer ${token}`,
        ...(tenantId ? { 'X-Tenant-Id': tenantId } : {}),
      },
    });

    if (!response.ok) throw new Error('Failed to download PDF');

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `Proposal_Draft_${proposalName.replace(/\s+/g, '_')}.pdf`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },
};

export default proposalService;
