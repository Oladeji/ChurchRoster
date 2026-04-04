import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ClipboardDocumentListIcon, PlusIcon, PencilIcon, TrashIcon, XMarkIcon, MagnifyingGlassIcon, WrenchScrewdriverIcon, CalendarIcon } from '@heroicons/react/24/solid';
import taskService from '../services/task.service';
import skillService from '../services/skill.service';
import type { Task, Skill } from '../types';

const TasksPage: React.FC = () => {
  const navigate = useNavigate();
  const [tasks, setTasks] = useState<Task[]>([]);
  const [skills, setSkills] = useState<Skill[]>([]);
  const [filteredTasks, setFilteredTasks] = useState<Task[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterFrequency, setFilterFrequency] = useState<string>('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [formData, setFormData] = useState({
    taskName: '',
    frequency: 'Weekly' as 'Weekly' | 'Monthly',
    dayRule: '',
    requiredSkillId: null as number | null,
    isRestricted: false,
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    filterTasks();
  }, [searchTerm, filterFrequency, tasks]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [tasksData, skillsData] = await Promise.all([taskService.getTasks(), skillService.getSkills()]);
      setTasks(tasksData);
      setSkills(skillsData.filter(s => s.isActive));
      setFilteredTasks(tasksData);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const filterTasks = () => {
    let filtered = tasks;
    if (searchTerm.trim() !== '') {
      filtered = filtered.filter(task => task.taskName.toLowerCase().includes(searchTerm.toLowerCase()) || task.dayRule.toLowerCase().includes(searchTerm.toLowerCase()));
    }
    if (filterFrequency !== 'all') {
      filtered = filtered.filter(task => task.frequency === filterFrequency);
    }
    setFilteredTasks(filtered);
  };

  const handleAddTask = () => {
    setSelectedTask(null);
    setFormData({ taskName: '', frequency: 'Weekly', dayRule: '', requiredSkillId: null, isRestricted: false });
    setShowModal(true);
  };

  const handleEditTask = (task: Task) => {
    setSelectedTask(task);
    setFormData({ taskName: task.taskName, frequency: task.frequency, dayRule: task.dayRule, requiredSkillId: task.requiredSkillId || null, isRestricted: task.isRestricted });
    setShowModal(true);
  };

  const handleDeleteClick = (task: Task) => {
    setSelectedTask(task);
    setShowDeleteModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.taskName.trim() || !formData.dayRule.trim()) {
      alert('Task name and day rule are required');
      return;
    }
    try {
      const submitData = { ...formData, requiredSkillId: formData.requiredSkillId || undefined };
      if (selectedTask) {
        await taskService.updateTask(selectedTask.taskId, submitData);
      } else {
        await taskService.createTask(submitData);
      }
      setShowModal(false);
      loadData();
    } catch (err: any) {
      alert(err.message || 'Failed to save task');
    }
  };

  const handleDelete = async () => {
    if (!selectedTask) return;
    try {
      await taskService.deleteTask(selectedTask.taskId);
      setShowDeleteModal(false);
      setSelectedTask(null);
      loadData();
    } catch (err: any) {
      alert(err.message || 'Failed to delete task');
    }
  };

  const getSkillName = (skillId: number | undefined) => {
    if (!skillId) return 'No skill required';
    const skill = skills.find(s => s.skillId === skillId);
    return skill ? skill.skillName : 'Unknown skill';
  };

  if (loading && tasks.length === 0) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', backgroundColor: '#f9fafb' }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{ width: '48px', height: '48px', border: '3px solid #e5e7eb', borderTop: '3px solid #7c3aed', borderRadius: '50%', animation: 'spin 1s linear infinite', margin: '0 auto' }}></div>
          <p style={{ marginTop: '16px', color: '#6b7280' }}>Loading tasks...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f9fafb', padding: '32px' }}>
      <div style={{ maxWidth: '1280px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <button onClick={() => navigate('/dashboard')} style={{ color: '#6b7280', fontSize: '16px', background: 'none', border: 'none', cursor: 'pointer', padding: '8px' }}>← Back</button>
            <ClipboardDocumentListIcon style={{ width: '32px', height: '32px', color: '#7c3aed' }} />
            <h1 style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: 0 }}>Tasks Management</h1>
          </div>
          <button onClick={handleAddTask} style={{ display: 'flex', alignItems: 'center', gap: '8px', background: '#7c3aed', color: 'white', padding: '12px 24px', borderRadius: '8px', border: 'none', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>
            <PlusIcon style={{ width: '20px', height: '20px' }} />Add Task
          </button>
        </div>

        {error && <div style={{ background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', padding: '16px', borderRadius: '8px', marginBottom: '24px' }}>{error}</div>}

        <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px', marginBottom: '24px' }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '16px' }}>
            <div style={{ position: 'relative' }}>
              <MagnifyingGlassIcon style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', width: '20px', height: '20px', color: '#9ca3af' }} />
              <input type="text" placeholder="Search tasks..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} style={{ width: '100%', paddingLeft: '40px', paddingRight: '16px', paddingTop: '12px', paddingBottom: '12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box' }} />
            </div>
            <select value={filterFrequency} onChange={(e) => setFilterFrequency(e.target.value)} style={{ padding: '12px 16px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', cursor: 'pointer' }}>
              <option value="all">All Frequencies</option>
              <option value="Weekly">Weekly</option>
              <option value="Monthly">Monthly</option>
            </select>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '24px', marginBottom: '24px' }}>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div><p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Total Tasks</p><p style={{ fontSize: '30px', fontWeight: 'bold', color: '#1f2937', margin: '8px 0 0 0' }}>{tasks.length}</p></div>
              <ClipboardDocumentListIcon style={{ width: '48px', height: '48px', color: '#7c3aed', opacity: 0.2 }} />
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div><p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Weekly</p><p style={{ fontSize: '30px', fontWeight: 'bold', color: '#3b82f6', margin: '8px 0 0 0' }}>{tasks.filter(t => t.frequency === 'Weekly').length}</p></div>
              <CalendarIcon style={{ width: '48px', height: '48px', color: '#3b82f6', opacity: 0.2 }} />
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div><p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>Monthly</p><p style={{ fontSize: '30px', fontWeight: 'bold', color: '#10b981', margin: '8px 0 0 0' }}>{tasks.filter(t => t.frequency === 'Monthly').length}</p></div>
              <CalendarIcon style={{ width: '48px', height: '48px', color: '#10b981', opacity: 0.2 }} />
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', padding: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div><p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>With Skills</p><p style={{ fontSize: '30px', fontWeight: 'bold', color: '#f59e0b', margin: '8px 0 0 0' }}>{tasks.filter(t => t.requiredSkillId).length}</p></div>
              <WrenchScrewdriverIcon style={{ width: '48px', height: '48px', color: '#f59e0b', opacity: 0.2 }} />
            </div>
          </div>
        </div>

        <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 1px 3px rgba(0,0,0,0.1)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead style={{ background: '#f9fafb' }}>
              <tr>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Task Name</th>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Frequency</th>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Day Rule</th>
                <th style={{ padding: '12px 24px', textAlign: 'left', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Required Skill</th>
                <th style={{ padding: '12px 24px', textAlign: 'right', fontSize: '12px', fontWeight: '500', color: '#6b7280', textTransform: 'uppercase' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredTasks.length === 0 ? (
                <tr><td colSpan={5} style={{ padding: '48px 24px', textAlign: 'center', color: '#6b7280' }}>{searchTerm || filterFrequency !== 'all' ? 'No tasks found' : 'No tasks yet. Click "Add Task" to create one.'}</td></tr>
              ) : (
                filteredTasks.map((task) => (
                  <tr key={task.taskId} style={{ borderTop: '1px solid #e5e7eb' }}>
                    <td style={{ padding: '16px 24px' }}>
                      <div style={{ display: 'flex', alignItems: 'center' }}>
                        <ClipboardDocumentListIcon style={{ width: '20px', height: '20px', color: '#7c3aed', marginRight: '12px' }} />
                        <span style={{ fontWeight: '500', color: '#1f2937' }}>{task.taskName}</span>
                      </div>
                    </td>
                    <td style={{ padding: '16px 24px' }}>
                      <span style={{ display: 'inline-flex', alignItems: 'center', padding: '4px 12px', borderRadius: '9999px', fontSize: '14px', fontWeight: '500', background: task.frequency === 'Weekly' ? '#dbeafe' : '#d1fae5', color: task.frequency === 'Weekly' ? '#1e40af' : '#065f46' }}>
                        {task.frequency}
                      </span>
                    </td>
                    <td style={{ padding: '16px 24px', color: '#6b7280' }}>{task.dayRule}</td>
                    <td style={{ padding: '16px 24px' }}>
                      {task.requiredSkillId ? (
                        <span style={{ display: 'inline-flex', alignItems: 'center', padding: '4px 12px', borderRadius: '9999px', fontSize: '14px', fontWeight: '500', background: '#fed7aa', color: '#92400e' }}>
                          <WrenchScrewdriverIcon style={{ width: '16px', height: '16px', marginRight: '4px' }} />
                          {getSkillName(task.requiredSkillId)}
                        </span>
                      ) : (
                        <span style={{ color: '#9ca3af' }}>—</span>
                      )}
                    </td>
                    <td style={{ padding: '16px 24px', textAlign: 'right' }}>
                      <button onClick={() => handleEditTask(task)} style={{ color: '#7c3aed', background: 'none', border: 'none', cursor: 'pointer', padding: '4px 8px', marginRight: '8px' }}>
                        <PencilIcon style={{ width: '20px', height: '20px' }} />
                      </button>
                      <button onClick={() => handleDeleteClick(task)} style={{ color: '#dc2626', background: 'none', border: 'none', cursor: 'pointer', padding: '4px 8px' }}>
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
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)', maxWidth: '600px', width: '100%', maxHeight: '90vh', overflow: 'auto' }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '24px', borderBottom: '1px solid #e5e7eb', position: 'sticky', top: 0, background: 'white' }}>
              <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', margin: 0 }}>{selectedTask ? 'Edit Task' : 'Add New Task'}</h3>
              <button onClick={() => setShowModal(false)} style={{ color: '#9ca3af', background: 'none', border: 'none', cursor: 'pointer', padding: '4px' }}>
                <XMarkIcon style={{ width: '24px', height: '24px' }} />
              </button>
            </div>
            <form onSubmit={handleSubmit} style={{ padding: '24px' }}>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Task Name *</label>
                <input type="text" value={formData.taskName} onChange={(e) => setFormData({ ...formData, taskName: e.target.value })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box' }} placeholder="e.g., Sunday Worship Leader" required />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Frequency *</label>
                  <select value={formData.frequency} onChange={(e) => setFormData({ ...formData, frequency: e.target.value as 'Weekly' | 'Monthly' })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', cursor: 'pointer' }} required>
                    <option value="Weekly">Weekly</option>
                    <option value="Monthly">Monthly</option>
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Day Rule *</label>
                  <input type="text" value={formData.dayRule} onChange={(e) => setFormData({ ...formData, dayRule: e.target.value })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', boxSizing: 'border-box' }} placeholder="e.g., Sunday, 1st Sunday" required />
                </div>
              </div>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '14px', fontWeight: '500', color: '#374151', marginBottom: '4px' }}>Required Skill</label>
                <select value={formData.requiredSkillId || ''} onChange={(e) => setFormData({ ...formData, requiredSkillId: e.target.value ? parseInt(e.target.value) : null })} style={{ width: '100%', padding: '8px 12px', border: '1px solid #d1d5db', borderRadius: '8px', fontSize: '16px', cursor: 'pointer' }}>
                  <option value="">No skill required</option>
                  {skills.map(skill => <option key={skill.skillId} value={skill.skillId}>{skill.skillName}</option>)}
                </select>
                <p style={{ marginTop: '4px', fontSize: '14px', color: '#6b7280' }}>Only members with this skill can be assigned</p>
              </div>
              <div style={{ marginBottom: '24px', display: 'flex', alignItems: 'center' }}>
                <input type="checkbox" id="isRestricted" checked={formData.isRestricted} onChange={(e) => setFormData({ ...formData, isRestricted: e.target.checked })} style={{ width: '16px', height: '16px', cursor: 'pointer' }} />
                <label htmlFor="isRestricted" style={{ marginLeft: '8px', fontSize: '14px', color: '#374151' }}>Restricted task (special permissions)</label>
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', paddingTop: '24px', borderTop: '1px solid #e5e7eb' }}>
                <button type="button" onClick={() => setShowModal(false)} style={{ padding: '8px 16px', color: '#374151', background: '#f3f4f6', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px' }}>Cancel</button>
                <button type="submit" style={{ padding: '8px 16px', background: '#7c3aed', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>{selectedTask ? 'Update Task' : 'Create Task'}</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showDeleteModal && selectedTask && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0, 0, 0, 0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '16px', zIndex: 50 }}>
          <div style={{ background: 'white', borderRadius: '8px', boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)', maxWidth: '500px', width: '100%', padding: '24px' }}>
            <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>Delete Task</h3>
            <p style={{ color: '#6b7280', marginBottom: '24px' }}>Are you sure you want to delete "<strong>{selectedTask.taskName}</strong>"? This will remove all assignments. This action cannot be undone.</p>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <button onClick={() => setShowDeleteModal(false)} style={{ padding: '8px 16px', color: '#374151', background: '#f3f4f6', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px' }}>Cancel</button>
              <button onClick={handleDelete} style={{ padding: '8px 16px', background: '#dc2626', color: 'white', border: 'none', borderRadius: '8px', cursor: 'pointer', fontSize: '16px', fontWeight: '500' }}>Delete Task</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TasksPage;
