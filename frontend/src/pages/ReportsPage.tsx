import React, { useState, useEffect } from 'react';
import { DocumentArrowDownIcon, CalendarIcon, UserIcon, ClipboardDocumentListIcon } from '@heroicons/react/24/outline';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7288/api';

interface User {
  userId: number;
  name: string;
  email: string;
  role: string;
}

const ReportsPage: React.FC = () => {
  const [loading, setLoading] = useState<string | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [monthlyYear, setMonthlyYear] = useState(new Date().getFullYear());
  const [monthlyMonth, setMonthlyMonth] = useState(new Date().getMonth() + 1);
  const [scheduleUserId, setScheduleUserId] = useState('');
  const [scheduleStartDate, setScheduleStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [scheduleEndDate, setScheduleEndDate] = useState(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
  const [taskStartDate, setTaskStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [taskEndDate, setTaskEndDate] = useState(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    try {
      const token = localStorage.getItem('authToken');
      const tenantId = localStorage.getItem('tenantId');
      const response = await fetch(API_URL + '/members', {
        headers: {
          Authorization: 'Bearer ' + token,
          ...(tenantId ? { 'X-Tenant-Id': tenantId } : {})
        }
      });
      if (response.ok) {
        const data = await response.json();
        setUsers(data);
      }
    } catch (error) {
      console.error('Failed to fetch users:', error);
    }
  };

  const downloadPDF = async (url: string, filename: string, reportType: string) => {
    try {
      setLoading(reportType);
      const token = localStorage.getItem('authToken');
      const tenantId = localStorage.getItem('tenantId');
      const response = await fetch(url, {
        headers: {
          Authorization: 'Bearer ' + token,
          ...(tenantId ? { 'X-Tenant-Id': tenantId } : {})
        }
      });
      if (!response.ok) throw new Error('Failed');
      const blob = await response.blob();
      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(downloadUrl);
    } catch (error) {
      console.error('Error:', error);
      alert('Failed to generate PDF');
    } finally {
      setLoading(null);
    }
  };

  const months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' }, { value: 3, label: 'March' },
    { value: 4, label: 'April' }, { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' }, { value: 9, label: 'September' },
    { value: 10, label: 'October' }, { value: 11, label: 'November' }, { value: 12, label: 'December' }
  ];
  const years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - 1 + i);

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #EFF6FF 0%, #FFFFFF 50%, #E0E7FF 100%)' }}>
      <div style={{ maxWidth: '1280px', margin: '0 auto', padding: '32px 16px' }}>
        <div style={{ textAlign: 'center', marginBottom: '48px' }}>
          <div style={{ display: 'inline-flex', width: '64px', height: '64px', background: 'linear-gradient(135deg, #3B82F6, #6366F1)', borderRadius: '16px', marginBottom: '16px', alignItems: 'center', justifyContent: 'center' }}>
            <DocumentArrowDownIcon style={{ width: '32px', height: '32px', color: 'white' }} />
          </div>
          <h1 style={{ fontSize: '36px', fontWeight: 'bold', color: '#111827', marginBottom: '12px' }}>PDF Reports</h1>
          <p style={{ fontSize: '18px', color: '#6B7280' }}>Generate professional PDF reports</p>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '24px', marginBottom: '32px' }}>
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.08)', overflow: 'hidden' }}>
            <div style={{ background: 'linear-gradient(135deg, #3B82F6, #2563EB)', padding: '16px 24px', display: 'flex', alignItems: 'center', color: 'white' }}>
              <div style={{ width: '40px', height: '40px', background: 'rgba(255,255,255,0.2)', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', marginRight: '12px' }}>
                <CalendarIcon style={{ width: '24px', height: '24px' }} />
              </div>
              <div>
                <h2 style={{ fontSize: '20px', fontWeight: 'bold', margin: 0 }}>Monthly Roster</h2>
                <p style={{ fontSize: '14px', margin: 0, opacity: 0.9 }}>Organized by week</p>
              </div>
            </div>
            <div style={{ padding: '24px' }}>
              <p style={{ fontSize: '14px', color: '#6B7280', marginBottom: '16px' }}>Complete monthly schedule</p>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '16px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>Year</label>
                  <select value={monthlyYear} onChange={(e) => setMonthlyYear(Number(e.target.value))} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }}>
                    {years.map(y => <option key={y} value={y}>{y}</option>)}
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>Month</label>
                  <select value={monthlyMonth} onChange={(e) => setMonthlyMonth(Number(e.target.value))} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }}>
                    {months.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
                  </select>
                </div>
              </div>
              <button onClick={() => downloadPDF(API_URL + '/reports/monthly-roster?year=' + monthlyYear + '&month=' + monthlyMonth, 'Monthly_Roster_' + monthlyYear + '_' + monthlyMonth + '.pdf', 'monthly')} disabled={loading === 'monthly'} style={{ width: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '12px', background: loading === 'monthly' ? '#9CA3AF' : 'linear-gradient(135deg, #2563EB, #1D4ED8)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: '500', cursor: loading === 'monthly' ? 'not-allowed' : 'pointer' }}>
                {loading === 'monthly' ? 'Generating...' : 'Generate PDF'}
              </button>
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.08)', overflow: 'hidden' }}>
            <div style={{ background: 'linear-gradient(135deg, #10B981, #059669)', padding: '16px 24px', display: 'flex', alignItems: 'center', color: 'white' }}>
              <div style={{ width: '40px', height: '40px', background: 'rgba(255,255,255,0.2)', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', marginRight: '12px' }}>
                <UserIcon style={{ width: '24px', height: '24px' }} />
              </div>
              <div>
                <h2 style={{ fontSize: '20px', fontWeight: 'bold', margin: 0 }}>Member Schedule</h2>
                <p style={{ fontSize: '14px', margin: 0, opacity: 0.9 }}>Personal calendar</p>
              </div>
            </div>
            <div style={{ padding: '24px' }}>
              <p style={{ fontSize: '14px', color: '#6B7280', marginBottom: '16px' }}>Individual member schedule</p>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>Select Member</label>
                <select value={scheduleUserId} onChange={(e) => setScheduleUserId(e.target.value)} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px', background: 'white' }}>
                  <option value="">-- Select a member --</option>
                  {users.map(user => (
                    <option key={user.userId} value={user.userId}>{user.name} ({user.role})</option>
                  ))}
                </select>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '16px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>Start</label>
                  <input type="date" value={scheduleStartDate} onChange={(e) => setScheduleStartDate(e.target.value)} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }} />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>End</label>
                  <input type="date" value={scheduleEndDate} onChange={(e) => setScheduleEndDate(e.target.value)} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }} />
                </div>
              </div>
              <button onClick={() => { let url = API_URL + '/reports/member-schedule?startDate=' + scheduleStartDate + '&endDate=' + scheduleEndDate; if (scheduleUserId) url += '&userId=' + scheduleUserId; downloadPDF(url, 'Member_Schedule.pdf', 'member'); }} disabled={loading === 'member'} style={{ width: '100%', padding: '12px', background: loading === 'member' ? '#9CA3AF' : 'linear-gradient(135deg, #059669, #047857)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: '500', cursor: loading === 'member' ? 'not-allowed' : 'pointer' }}>
                {loading === 'member' ? 'Generating...' : 'Generate PDF'}
              </button>
            </div>
          </div>
          <div style={{ background: 'white', borderRadius: '16px', boxShadow: '0 4px 15px rgba(0,0,0,0.08)', overflow: 'hidden' }}>
            <div style={{ background: 'linear-gradient(135deg, #8B5CF6, #6366F1)', padding: '16px 24px', display: 'flex', alignItems: 'center', color: 'white' }}>
              <div style={{ width: '40px', height: '40px', background: 'rgba(255,255,255,0.2)', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', marginRight: '12px' }}>
                <ClipboardDocumentListIcon style={{ width: '24px', height: '24px' }} />
              </div>
              <div>
                <h2 style={{ fontSize: '20px', fontWeight: 'bold', margin: 0 }}>Task Assignments</h2>
                <p style={{ fontSize: '14px', margin: 0, opacity: 0.9 }}>Grouped by task</p>
              </div>
            </div>
            <div style={{ padding: '24px' }}>
              <p style={{ fontSize: '14px', color: '#6B7280', marginBottom: '16px' }}>Assignments by task type</p>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '16px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>Start</label>
                  <input type="date" value={taskStartDate} onChange={(e) => setTaskStartDate(e.target.value)} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }} />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '12px', fontWeight: '600', marginBottom: '6px' }}>End</label>
                  <input type="date" value={taskEndDate} onChange={(e) => setTaskEndDate(e.target.value)} style={{ width: '100%', padding: '8px', fontSize: '14px', border: '1px solid #D1D5DB', borderRadius: '8px' }} />
                </div>
              </div>
              <button onClick={() => downloadPDF(API_URL + '/reports/task-assignments?startDate=' + taskStartDate + '&endDate=' + taskEndDate, 'Task_Assignments.pdf', 'task')} disabled={loading === 'task'} style={{ width: '100%', padding: '12px', background: loading === 'task' ? '#9CA3AF' : 'linear-gradient(135deg, #8B5CF6, #7C3AED)', color: 'white', border: 'none', borderRadius: '8px', fontWeight: '500', cursor: loading === 'task' ? 'not-allowed' : 'pointer' }}>
                {loading === 'task' ? 'Generating...' : 'Generate PDF'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ReportsPage;