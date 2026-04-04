import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { DocumentArrowDownIcon } from '@heroicons/react/24/outline';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7288/api';

const MemberReportPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [scheduleStartDate, setScheduleStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [scheduleEndDate, setScheduleEndDate] = useState(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );

  const downloadPDF = async () => {
    if (!user?.userId) {
      alert('User not found. Please log in again.');
      return;
    }

    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      const url = `${API_URL}/reports/member-schedule?userId=${user.userId}&startDate=${scheduleStartDate}&endDate=${scheduleEndDate}`;

      const response = await fetch(url, {
        headers: { Authorization: 'Bearer ' + token }
      });

      if (!response.ok) throw new Error('Failed to generate report');

      const blob = await response.blob();
      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = `My_Schedule_${user.name.replace(/\s+/g, '_')}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(downloadUrl);
    } catch (error) {
      console.error('Error:', error);
      alert('Failed to generate PDF report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ minHeight: '100vh', background: 'linear-gradient(135deg, #EFF6FF 0%, #FFFFFF 50%, #E0E7FF 100%)' }}>
      <div style={{ maxWidth: '800px', margin: '0 auto', padding: '32px 16px' }}>
        <button
          onClick={() => navigate('/members')}
          style={{
            marginBottom: '24px',
            padding: '8px 16px',
            background: 'white',
            border: '1px solid #D1D5DB',
            borderRadius: '8px',
            cursor: 'pointer',
            fontSize: '14px',
            fontWeight: '500'
          }}
        >
          ← Back to Members
        </button>

        <div style={{ textAlign: 'center', marginBottom: '48px' }}>
          <div style={{
            display: 'inline-flex',
            width: '64px',
            height: '64px',
            background: 'linear-gradient(135deg, #10B981, #059669)',
            borderRadius: '16px',
            marginBottom: '16px',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <DocumentArrowDownIcon style={{ width: '32px', height: '32px', color: 'white' }} />
          </div>
          <h1 style={{ fontSize: '36px', fontWeight: 'bold', color: '#111827', marginBottom: '12px' }}>
            My Schedule Report
          </h1>
          <p style={{ fontSize: '18px', color: '#6B7280' }}>
            Generate your personal ministry schedule
          </p>
        </div>

        <div style={{
          background: 'white',
          borderRadius: '16px',
          boxShadow: '0 4px 15px rgba(0,0,0,0.08)',
          padding: '32px'
        }}>
          <div style={{ marginBottom: '24px' }}>
            <h2 style={{ fontSize: '20px', fontWeight: 'bold', marginBottom: '8px' }}>
              {user?.name}'s Schedule
            </h2>
            <p style={{ fontSize: '14px', color: '#6B7280' }}>
              Your personal ministry assignments and tasks
            </p>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '24px' }}>
            <div>
              <label style={{
                display: 'block',
                fontSize: '12px',
                fontWeight: '600',
                marginBottom: '6px',
                color: '#374151'
              }}>
                Start Date
              </label>
              <input
                type="date"
                value={scheduleStartDate}
                onChange={(e) => setScheduleStartDate(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px',
                  fontSize: '14px',
                  border: '1px solid #D1D5DB',
                  borderRadius: '8px'
                }}
              />
            </div>
            <div>
              <label style={{
                display: 'block',
                fontSize: '12px',
                fontWeight: '600',
                marginBottom: '6px',
                color: '#374151'
              }}>
                End Date
              </label>
              <input
                type="date"
                value={scheduleEndDate}
                onChange={(e) => setScheduleEndDate(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px',
                  fontSize: '14px',
                  border: '1px solid #D1D5DB',
                  borderRadius: '8px'
                }}
              />
            </div>
          </div>

          <button
            onClick={downloadPDF}
            disabled={loading}
            style={{
              width: '100%',
              padding: '14px',
              background: loading ? '#9CA3AF' : 'linear-gradient(135deg, #10B981, #059669)',
              color: 'white',
              border: 'none',
              borderRadius: '8px',
              fontWeight: '600',
              fontSize: '16px',
              cursor: loading ? 'not-allowed' : 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px'
            }}
          >
            <DocumentArrowDownIcon style={{ width: '20px', height: '20px' }} />
            {loading ? 'Generating PDF...' : 'Download My Schedule PDF'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default MemberReportPage;
