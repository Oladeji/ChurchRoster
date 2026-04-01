import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Dashboard: React.FC = () => {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
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

      {isAdmin ? (
        <div className="admin-dashboard">
          <h2>Admin Dashboard</h2>
          <div className="dashboard-grid">
            <div className="dashboard-card">
              <h3>👥 Members</h3>
              <p>Manage church members and their skills</p>
            </div>
            <div className="dashboard-card">
              <h3>📋 Assignments</h3>
              <p>Create and manage task assignments</p>
            </div>
            <div className="dashboard-card">
              <h3>📅 Calendar</h3>
              <p>View and manage ministry schedule</p>
            </div>
            <div className="dashboard-card">
              <h3>📊 Reports</h3>
              <p>Generate and print ministry reports</p>
            </div>
          </div>
        </div>
      ) : (
        <div className="member-dashboard">
          <h2>Member Dashboard</h2>
          <div className="dashboard-grid">
            <div className="dashboard-card">
              <h3>📝 My Assignments</h3>
              <p>View your upcoming ministry tasks</p>
            </div>
            <div className="dashboard-card">
              <h3>📅 Calendar</h3>
              <p>View ministry schedule</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
