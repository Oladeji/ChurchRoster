import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useNotifications } from '../hooks/useNotifications';
import ProposalDashboardWidget from '../components/ProposalDashboardWidget';

const Dashboard: React.FC = () => {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const { notificationPermission, requestPermission } = useNotifications();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navigateTo = (path: string) => {
    navigate(path);
  };

  return (
    <div className="dashboard">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '32px' }}>
        <div>
          <h1>Welcome, {user?.name}!</h1>
          <p>Role: {user?.role}</p>
        </div>
        <button 
          onClick={handleLogout}
          style={{
            padding: '10px 24px',
            background: '#EF4444',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            fontWeight: '500'
          }}
        >
          Logout
        </button>
      </div>

      {/* Notification Permission Prompt */}
      {notificationPermission !== 'granted' && (
        <div style={{
          background: '#fef3c7',
          border: '1px solid #f59e0b',
          borderRadius: '8px',
          padding: '16px',
          marginBottom: '24px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <span style={{ fontSize: '24px' }}>🔔</span>
            <div>
              <p style={{ margin: 0, fontWeight: '600', color: '#92400e' }}>
                Enable Notifications
              </p>
              <p style={{ margin: 0, fontSize: '14px', color: '#92400e' }}>
                Get notified when you're assigned to ministry tasks
              </p>
            </div>
          </div>
          <button
            onClick={requestPermission}
            style={{
              background: '#f59e0b',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '8px 16px',
              cursor: 'pointer',
              fontWeight: '600'
            }}
          >
            Enable
          </button>
        </div>
      )}

      {isAdmin ? (
        <div className="admin-dashboard">
          <h2>Admin Dashboard</h2>
          <div className="dashboard-grid">
            <div className="dashboard-card" onClick={() => navigateTo('/members')}>
              <h3>👥 Members</h3>
              <p>Manage church members and their skills</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/skills')}>
              <h3>🔧 Skills</h3>
              <p>Create and manage skills/qualifications</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/tasks')}>
              <h3>📝 Tasks</h3>
              <p>Create and manage ministry tasks</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/assignments')}>
              <h3>📋 Assignments</h3>
              <p>Create and manage task assignments</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/calendar')}>
              <h3>📅 Calendar</h3>
              <p>View and manage ministry schedule</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/reports')}>
              <h3>📊 Reports</h3>
              <p>Generate and print ministry reports</p>
            </div>
            <ProposalDashboardWidget />
          </div>
        </div>
      ) : (
        <div className="member-dashboard">
          <h2>Member Dashboard</h2>
          <div className="dashboard-grid">
            <div className="dashboard-card" onClick={() => navigateTo('/my-assignments')}>
              <h3>📝 My Assignments</h3>
              <p>View your upcoming ministry tasks</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/calendar')}>
              <h3>📅 Calendar</h3>
              <p>View ministry schedule</p>
            </div>
            <div className="dashboard-card" onClick={() => navigateTo('/member-report')}>
              <h3>📊 My Reports</h3>
              <p>Download your personal schedule PDF</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
