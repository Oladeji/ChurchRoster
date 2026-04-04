import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  WrenchScrewdriverIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  XMarkIcon,
  MagnifyingGlassIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/react/24/solid';
import skillService from '../services/skill.service';
import type { Skill } from '../types';

const SkillsPage: React.FC = () => {
  const navigate = useNavigate();
  const [skills, setSkills] = useState<Skill[]>([]);
  const [filteredSkills, setFilteredSkills] = useState<Skill[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);
  const [formData, setFormData] = useState({
    skillName: '',
    description: '',
    isActive: true,
  });

  useEffect(() => {
    loadSkills();
  }, []);

  useEffect(() => {
    if (searchTerm.trim() === '') {
      setFilteredSkills(skills);
    } else {
      const filtered = skills.filter(skill =>
        skill.skillName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (skill.description && skill.description.toLowerCase().includes(searchTerm.toLowerCase()))
      );
      setFilteredSkills(filtered);
    }
  }, [searchTerm, skills]);

  const loadSkills = async () => {
    try {
      setLoading(true);
      const data = await skillService.getSkills();
      setSkills(data);
      setFilteredSkills(data);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Failed to load skills');
    } finally {
      setLoading(false);
    }
  };

  const handleAddSkill = () => {
    setSelectedSkill(null);
    setFormData({
      skillName: '',
      description: '',
      isActive: true,
    });
    setShowModal(true);
  };

  const handleEditSkill = (skill: Skill) => {
    setSelectedSkill(skill);
    setFormData({
      skillName: skill.skillName,
      description: skill.description || '',
      isActive: skill.isActive,
    });
    setShowModal(true);
  };

  const handleDeleteClick = (skill: Skill) => {
    setSelectedSkill(skill);
    setShowDeleteModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.skillName.trim()) {
      alert('Skill name is required');
      return;
    }

    try {
      if (selectedSkill) {
        await skillService.updateSkill(selectedSkill.skillId, formData);
      } else {
        await skillService.createSkill(formData);
      }
      setShowModal(false);
      loadSkills();
    } catch (err: any) {
      alert(err.message || 'Failed to save skill');
    }
  };

  const handleDelete = async () => {
    if (!selectedSkill) return;

    try {
      await skillService.deleteSkill(selectedSkill.skillId);
      setShowDeleteModal(false);
      setSelectedSkill(null);
      loadSkills();
    } catch (err: any) {
      alert(err.message || 'Failed to delete skill');
    }
  };

  if (loading && skills.length === 0) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', backgroundColor: '#f9fafb' }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{ width: '48px', height: '48px', border: '3px solid #e5e7eb', borderTop: '3px solid #7c3aed', borderRadius: '50%', animation: 'spin 1s linear infinite', margin: '0 auto' }}></div>
          <p style={{ marginTop: '16px', color: '#6b7280' }}>Loading skills...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f9fafb', padding: '32px' }}>
      <div style={{ maxWidth: '1280px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <button onClick={() => navigate('/dashboard')} style={{ color: '#6b7280', fontSize: '16px', background: 'none', border: 'none', cursor: 'pointer', padding: '8px' }}>
              ← Back
            </button>
            <WrenchScrewdriverIcon style={{ width: '32px', height: '32px', color: '#7c3aed' }} />
            <h1 style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: 0 }}>Skills Management</h1>
          </div>
          <button onClick={handleAddSkill} style={{ display: 'flex', alignItems: 'center', gap: '8px', background: '#7c3aed', color: 'white', padding: '12px 24px', borderRadius: '8px', border: 'none', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>
            <PlusIcon style={{ width: '20px', height: '20px' }} />
            Add Skill
          </button>
        </div>

        {error && (
          <div style={{ background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', padding: '16px', borderRadius: '8px', marginBottom: '24px' }}>
            {error}
          </div>
        )}

        <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px', marginBottom: '24px' }}>
          <div style={{ position: 'relative' }}>
            <MagnifyingGlassIcon style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', width: '20px', height: '20px', color: '#9ca3af' }} />
            <input type="text" placeholder="Search skills..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} style={{ width: '100%', paddingLeft: '40px', paddingRight: '16px', paddingTop: '12px', paddingBottom: '12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box' }} />
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '24px', marginBottom: '24px' }}>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Total Skills</p>
                <p style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: '8px 0 0 0' }}>{skills.length}</p>
              </div>
              <WrenchScrewdriverIcon style={{ width: '48px', height: '48px', color: '#7c3aed', opacity: 0.2 }} />
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Active Skills</p>
                <p style={{ fontSize: '30px', fontWeight: 'bold', color: '#10b981', margin: '8px 0 0 0' }}>{skills.filter(s => s.isActive).length}</p>
              </div>
              <CheckCircleIcon style={{ width: '48px', height: '48px', color: '#10b981', opacity: 0.2 }} />
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Inactive Skills</p>
                <p style={{ fontSize: '30px', fontWeight: 'bold', color: '#9ca3af', margin: '8px 0 0 0' }}>{skills.filter(s => !s.isActive).length}</p>
              </div>
              <XCircleIcon style={{ width: '48px', height: '48px', color: '#9ca3af', opacity: 0.2 }} />
            </div>
          </div>
        </div>

        <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead style={{ background: '#f9fafb' }}>
              <tr>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Skill Name</th>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Description</th>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Status</th>
                <th style={{ padding: '12px 24px', textAlign: 'right', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredSkills.length === 0 ? (
                <tr>
                  <td colSpan={4} style={{ padding: '48px 24px', textAlign: 'center', color: '#6b7280' }}>
                    {searchTerm ? 'No skills found' : 'No skills yet. Click "Add Skill" to create one.'}
                  </td>
                </tr>
              ) : (
                filteredSkills.map((skill) => (
                  <tr key={skill.skillId} style={{ borderTop: '1px solid #e5e7eb' }}>
                    <td style={{ padding: '16px 24px' }}>
                      <div style={{ display: 'flex', alignItems: 'center' }}>
                        <WrenchScrewdriverIcon style={{ width: '20px', height: '20px', color: '#7c3aed', marginRight: '12px' }} />
                        <span style={{ fontWeight: '500', color: '#1f2937' }}>{skill.skillName}</span>
                      </div>
                    </td>
                    <td style={{ padding: '16px 24px', color: '#6b7280' }}>{skill.description || '—'}</td>
                    <td style={{ padding: '16px 24px' }}>
                      {skill.isActive ? (
                        <span style={{ display: 'inline-flex', alignItems: 'center', padding: '4px 12px', borderRadius: '9999px', fontSize: '14px', fontWeight: '500', background: '#d1fae5', color: '#065f46' }}>
                          <CheckCircleIcon style={{ width: '16px', height: '16px', marginRight: '4px' }} />Active
                        </span>
                      ) : (
                        <span style={{ display: 'inline-flex', alignItems: 'center', padding: '4px 12px', borderRadius: '9999px', fontSize: '14px', fontWeight: '500', background: '#f3f4f6', color: '#374151' }}>
                          <XCircleIcon style={{ width: '16px', height: '16px', marginRight: '4px' }} />Inactive
                        </span>
                      )}
                    </td>
                    <td style={{ padding: '16px 24px', textAlign: 'right' }}>
                      <button onClick={() => handleEditSkill(skill)} style={{ color: '#7c3aed', background: 'none', border: 'none', cursor: 'pointer', padding: '4px 8px', marginRight: '8px' }}>
                        <PencilIcon style={{ width: '20px', height: '20px' }} />
                      </button>
                      <button onClick={() => handleDeleteClick(skill)} style={{ color: '#dc2626', background: 'none', border: 'none', cursor: 'pointer', padding: '4px 8px' }}>
                        <TrashIcon style={{ width: '20px', height: '20px' }} />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0, 0, 0, 0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '16px', zIndex: 50 }}>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)', maxWidth: '500px', width: '100%' }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '24px', borderBottom: '1px solid #e5e7eb' }}>
              <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', margin: 0 }}>{selectedSkill ? 'Edit Skill' : 'Add New Skill'}</h3>
              <button onClick={() => setShowModal(false)} style={{ color: '#9ca3af', background: 'none', border: 'none', cursor: 'pointer', padding: '4px' }}>
                <XMarkIcon style={{ width: '24px', height: '24px' }} />
              </button>
            </div>
            <form onSubmit={handleSubmit} style={{ padding: '24px' }}>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Skill Name *</label>
                <input type="text" value={formData.skillName} onChange={(e) => setFormData({ ...formData, skillName: e.target.value })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box' }} placeholder="e.g., Audio Engineering" required />
              </div>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Description</label>
                <textarea value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box', minHeight: '80px' }} placeholder="Optional description" />
              </div>
              <div style={{ marginBottom: '24px', display: 'flex', alignItems: 'center' }}>
                <input type="checkbox" id="isActive" checked={formData.isActive} onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })} style={{ width: '16px', height: '16px', cursor: 'pointer' }} />
                <label htmlFor="isActive" style={{ marginLeft: '8px', fontSize: '14px', color: '#374151' }}>Active skill (available for assignment)</label>
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
                <button type="button" onClick={() => setShowModal(false)} style={{ padding: '8px 16px', color: '#374151', background: '#f3f4f6', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px' }}>Cancel</button>
                <button type="submit" style={{ padding: '8px 16px', background: '#7c3aed', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>{selectedSkill ? 'Update' : 'Create'}</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showDeleteModal && selectedSkill && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0, 0, 0, 0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '16px', zIndex: 50 }}>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)', maxWidth: '500px', width: '100%', padding: '24px' }}>
            <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>Delete Skill</h3>
            <p style={{ color: '#6b7280', marginBottom: '24px' }}>Are you sure you want to delete "<strong>{selectedSkill.skillName}</strong>"? This action cannot be undone.</p>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <button onClick={() => setShowDeleteModal(false)} style={{ padding: '8px 16px', color: '#374151', background: '#f3f4f6', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px' }}>Cancel</button>
              <button onClick={handleDelete} style={{ padding: '8px 16px', background: '#dc2626', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>Delete</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SkillsPage;
