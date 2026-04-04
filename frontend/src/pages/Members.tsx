import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import memberService from '../services/member.service';
import invitationService from '../services/invitation.service';
import type { User, Skill } from '../types';

const Members: React.FC = () => {
  const [members, setMembers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingMember, setEditingMember] = useState<User | null>(null);
  const [managingSkillsFor, setManagingSkillsFor] = useState<User | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    fetchMembers();
  }, []);

  const fetchMembers = async () => {
    try {
      setLoading(true);
      const data = await memberService.getAll();
      setMembers(data);
    } catch (err: unknown) {
      setError('Failed to load members');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (member: User) => {
    if (!window.confirm(`Are you sure you want to delete ${member.name}?\n\nThis action cannot be undone.`)) {
      return;
    }

    try {
      await memberService.delete(member.userId);
      alert(`${member.name} has been deleted successfully.`);
      fetchMembers();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      alert(`Failed to delete member: ${error.response?.data?.message || 'Unknown error'}`);
    }
  };

  const handleBack = () => {
    navigate('/dashboard');
  };

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">Loading members...</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <button onClick={handleBack} className="back-button">
          ← Back to Dashboard
        </button>
        <h1>Church Members</h1>
        <div style={{ display: 'flex', gap: '12px' }}>
          <button 
            onClick={() => navigate('/member-report')} 
            className="secondary-button"
            style={{ background: '#10B981', color: 'white' }}
          >
            📊 My Reports
          </button>
          <button onClick={() => setShowAddModal(true)} className="primary-button">
            + Add Member
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="members-grid">
        {members.length === 0 ? (
          <p>No members found. Add your first member to get started.</p>
        ) : (
          <table className="members-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Role</th>
                <th>Status</th>
                <th>Monthly Limit</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {members.map((member) => (
                <tr key={member.userId}>
                  <td>{member.name}</td>
                  <td>{member.email}</td>
                  <td>{member.phone}</td>
                  <td>
                    <span className={`role-badge ${member.role.toLowerCase()}`}>
                      {member.role}
                    </span>
                  </td>
                  <td>
                    <span className={`status-badge ${member.isActive ? 'active' : 'inactive'}`}>
                      {member.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>{member.monthlyLimit || 'N/A'}</td>
                  <td>
                    <button 
                      className="btn-sm" 
                      onClick={() => setEditingMember(member)}
                      title="Edit member details"
                    >
                      Edit
                    </button>
                    <button 
                      className="btn-sm" 
                      onClick={() => setManagingSkillsFor(member)}
                      title="Manage member skills"
                    >
                      Skills
                    </button>
                    <button 
                      className="btn-sm btn-danger" 
                      onClick={() => handleDelete(member)}
                      title="Delete member"
                      style={{ 
                        backgroundColor: '#dc2626', 
                        color: 'white',
                        marginLeft: '4px'
                      }}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showAddModal && (
        <AddMemberModal
          onClose={() => setShowAddModal(false)}
          onSuccess={() => {
            setShowAddModal(false);
            fetchMembers();
          }}
        />
      )}

      {editingMember && (
        <EditMemberModal
          member={editingMember}
          onClose={() => setEditingMember(null)}
          onSuccess={() => {
            setEditingMember(null);
            fetchMembers();
          }}
        />
      )}

      {managingSkillsFor && (
        <ManageSkillsModal
          member={managingSkillsFor}
          onClose={() => setManagingSkillsFor(null)}
        />
      )}
    </div>
  );
};

// Add Member Modal Component
const AddMemberModal: React.FC<{ onClose: () => void; onSuccess: () => void }> = ({
  onClose,
  onSuccess
}) => {
  const [formData, setFormData] = useState<{
    name: string;
    email: string;
    phone: string;
    password: string;
    role: 'Admin' | 'Member';
    monthlyLimit: number;
    isActive: boolean;
    sendInvitationEmail: boolean;
  }>({
    name: '',
    email: '',
    phone: '',
    password: '',
    role: 'Member',
    monthlyLimit: 4,
    isActive: true,
    sendInvitationEmail: false
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    // If sending invitation email, password is not required
    if (!formData.sendInvitationEmail) {
      // Validate password only if not sending invitation
      if (formData.password.length < 8) {
        setError('Password must be at least 8 characters long');
        setLoading(false);
        return;
      }

      const hasUpperCase = /[A-Z]/.test(formData.password);
      const hasLowerCase = /[a-z]/.test(formData.password);
      const hasNumber = /[0-9]/.test(formData.password);

      if (!hasUpperCase || !hasLowerCase || !hasNumber) {
        setError('Password must contain uppercase, lowercase, and a number');
        setLoading(false);
        return;
      }
    }

    try {
      if (formData.sendInvitationEmail) {
        // Send invitation email instead of creating member directly
        console.log('📧 Sending invitation email to:', formData.email);
        const result = await invitationService.sendInvitation({
          email: formData.email,
          name: formData.name,
          phone: formData.phone,
          role: formData.role
        });
        console.log('✅ Invitation sent successfully:', result);
        alert(`✅ Invitation email sent successfully to ${formData.email}!\n\nThe member will receive an email with a link to set up their account.\n\nCheck your email spam folder if not received within a few minutes.`);
      } else {
        // Create member with password
        console.log('👤 Creating member with password:', formData.email);
        await memberService.create(formData);
        alert(`Member created successfully!\n\nEmail: ${formData.email}\nTemporary Password: ${formData.password}\n\nPlease share this password with the member securely and ask them to change it on first login.`);
      }
      onSuccess();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      console.error('❌ Error:', error);
      const errorMessage = error.response?.data?.message || 'Failed to process request';
      setError(errorMessage);
      alert(`❌ Error: ${errorMessage}\n\nPlease check the console for details.`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Add New Member</h2>
          <button onClick={onClose} className="close-button">×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="name">Full Name *</label>
            <input
              id="name"
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email *</label>
            <input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="phone">Phone</label>
            <input
              id="phone"
              type="tel"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Temporary Password {!formData.sendInvitationEmail && '*'}</label>
            <input
              id="password"
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              required={!formData.sendInvitationEmail}
              disabled={formData.sendInvitationEmail}
              autoComplete="new-password"
            />
            <small style={{ color: '#6B7280', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              {formData.sendInvitationEmail 
                ? 'Member will set their own password via invitation email' 
                : 'Min 8 characters, with uppercase, lowercase, and number'}
            </small>
          </div>

          <div className="form-group">
            <label>
              <input
                type="checkbox"
                checked={formData.sendInvitationEmail}
                onChange={(e) => setFormData({ ...formData, sendInvitationEmail: e.target.checked, password: e.target.checked ? '' : formData.password })}
              />
              {' '}Send invitation email instead of setting password
            </label>
            <small style={{ color: '#6B7280', fontSize: '12px', marginTop: '4px', display: 'block' }}>
              If checked, the member will receive an email with a secure link to set up their account
            </small>
          </div>

          <div className="form-group">
            <label htmlFor="role">Role *</label>
            <select
              id="role"
              value={formData.role}
              onChange={(e) => setFormData({ ...formData, role: e.target.value as 'Admin' | 'Member' })}
            >
              <option value="Member">Member</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="monthlyLimit">Monthly Assignment Limit</label>
            <input
              id="monthlyLimit"
              type="number"
              min="1"
              max="20"
              value={formData.monthlyLimit}
              onChange={(e) => setFormData({ ...formData, monthlyLimit: parseInt(e.target.value) })}
            />
          </div>

          <div className="form-group">
            <label>
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              />
              {' '}Active
            </label>
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="secondary-button">
              Cancel
            </button>
            <button type="submit" disabled={loading} className="primary-button">
              {loading ? (formData.sendInvitationEmail ? 'Sending...' : 'Adding...') : (formData.sendInvitationEmail ? 'Send Invitation' : 'Add Member')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Edit Member Modal Component
const EditMemberModal: React.FC<{ 
  member: User; 
  onClose: () => void; 
  onSuccess: () => void 
}> = ({ member, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    name: member.name,
    email: member.email,
    phone: member.phone,
    role: member.role as 'Admin' | 'Member',
    monthlyLimit: member.monthlyLimit || 4,
    isActive: member.isActive
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      await memberService.update(member.userId, formData);
      alert(`${formData.name} updated successfully!`);
      onSuccess();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || 'Failed to update member');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit Member: {member.name}</h2>
          <button onClick={onClose} className="close-button">×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="edit-name">Full Name *</label>
            <input
              id="edit-name"
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="edit-email">Email *</label>
            <input
              id="edit-email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="edit-phone">Phone</label>
            <input
              id="edit-phone"
              type="tel"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label htmlFor="edit-role">Role *</label>
            <select
              id="edit-role"
              value={formData.role}
              onChange={(e) => setFormData({ ...formData, role: e.target.value as 'Admin' | 'Member' })}
            >
              <option value="Member">Member</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="edit-monthlyLimit">Monthly Assignment Limit</label>
            <input
              id="edit-monthlyLimit"
              type="number"
              min="1"
              max="20"
              value={formData.monthlyLimit}
              onChange={(e) => setFormData({ ...formData, monthlyLimit: parseInt(e.target.value) })}
            />
          </div>

          <div className="form-group">
            <label>
              <input
                type="checkbox"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              />
              {' '}Active
            </label>
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="secondary-button">
              Cancel
            </button>
            <button type="submit" disabled={loading} className="primary-button">
              {loading ? 'Updating...' : 'Update Member'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Manage Skills Modal Component
const ManageSkillsModal: React.FC<{ 
  member: User; 
  onClose: () => void;
}> = ({ member, onClose }) => {
  const [availableSkills, setAvailableSkills] = useState<Skill[]>([]);
  const [memberSkills, setMemberSkills] = useState<Skill[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      // Import skillService
      const { default: skillService } = await import('../services/skill.service');
      const [allSkills, userSkills] = await Promise.all([
        skillService.getAll(),
        memberService.getMemberSkills(member.userId)
      ]);
      setAvailableSkills(allSkills);
      setMemberSkills(userSkills);
    } catch (err) {
      setError('Failed to load skills');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddSkill = async (skillId: number) => {
    try {
      await memberService.assignSkillToMember(member.userId, skillId);
      await fetchData();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      alert(`Failed to add skill: ${error.response?.data?.message || 'Unknown error'}`);
    }
  };

  const handleRemoveSkill = async (skillId: number) => {
    try {
      await memberService.removeSkillFromMember(member.userId, skillId);
      await fetchData();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      alert(`Failed to remove skill: ${error.response?.data?.message || 'Unknown error'}`);
    }
  };

  const memberSkillIds = memberSkills.map(s => s.skillId);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '600px' }}>
        <div className="modal-header">
          <h2>Manage Skills: {member.name}</h2>
          <button onClick={onClose} className="close-button">×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        {loading ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>Loading skills...</div>
        ) : (
          <div>
            <div style={{ marginBottom: '20px' }}>
              <h3 style={{ marginBottom: '10px' }}>Current Skills ({memberSkills.length})</h3>
              {memberSkills.length === 0 ? (
                <p style={{ color: '#6B7280' }}>No skills assigned yet</p>
              ) : (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  {memberSkills.map((skill) => (
                    <span
                      key={skill.skillId}
                      style={{
                        padding: '6px 12px',
                        backgroundColor: '#EFF6FF',
                        color: '#1E40AF',
                        borderRadius: '20px',
                        fontSize: '14px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '6px'
                      }}
                    >
                      {skill.skillName}
                      <button
                        onClick={() => handleRemoveSkill(skill.skillId)}
                        style={{
                          background: 'none',
                          border: 'none',
                          color: '#DC2626',
                          cursor: 'pointer',
                          padding: '0 4px',
                          fontSize: '16px',
                          fontWeight: 'bold'
                        }}
                        title="Remove skill"
                      >
                        ×
                      </button>
                    </span>
                  ))}
                </div>
              )}
            </div>

            <div>
              <h3 style={{ marginBottom: '10px' }}>Available Skills</h3>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                {availableSkills
                  .filter(skill => !memberSkillIds.includes(skill.skillId))
                  .map((skill) => (
                    <button
                      key={skill.skillId}
                      onClick={() => handleAddSkill(skill.skillId)}
                      style={{
                        padding: '6px 12px',
                        backgroundColor: '#F3F4F6',
                        color: '#374151',
                        border: '1px solid #D1D5DB',
                        borderRadius: '20px',
                        fontSize: '14px',
                        cursor: 'pointer'
                      }}
                      title="Click to add skill"
                    >
                      + {skill.skillName}
                    </button>
                  ))}
              </div>
              {availableSkills.filter(skill => !memberSkillIds.includes(skill.skillId)).length === 0 && (
                <p style={{ color: '#6B7280' }}>All skills have been assigned</p>
              )}
            </div>
          </div>
        )}

        <div className="modal-actions" style={{ marginTop: '20px' }}>
          <button onClick={onClose} className="primary-button">
            Done
          </button>
        </div>
      </div>
    </div>
  );
};

export default Members;
