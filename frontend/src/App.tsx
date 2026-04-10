import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Members from './pages/Members';
import SkillsPage from './pages/SkillsPage';
import TasksPage from './pages/TasksPage';
import CalendarPage from './pages/CalendarPage';
import AssignmentsPage from './pages/AssignmentsPage';
import MyAssignmentsPage from './pages/MyAssignmentsPage';
import AcceptInvitation from './pages/AcceptInvitation';
import ReportsPage from './pages/ReportsPage';
import MemberReportPage from './pages/MemberReportPage';
import ProposalsPage from './pages/ProposalsPage';
import GenerateProposalPage from './pages/GenerateProposalPage';
import ProposalDetailPage from './pages/ProposalDetailPage';
import './App.css';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/accept-invitation" element={<AcceptInvitation />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/members"
            element={
              <ProtectedRoute requireAdmin={true}>
                <Members />
              </ProtectedRoute>
            }
          />

          <Route
            path="/skills"
            element={
              <ProtectedRoute requireAdmin={true}>
                <SkillsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/tasks"
            element={
              <ProtectedRoute requireAdmin={true}>
                <TasksPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/assignments"
            element={
              <ProtectedRoute requireAdmin={true}>
                <AssignmentsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/calendar"
            element={
              <ProtectedRoute>
                <CalendarPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/reports"
            element={
              <ProtectedRoute requireAdmin={true}>
                <ReportsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/my-assignments"
            element={
              <ProtectedRoute>
                <MyAssignmentsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/member-report"
            element={
              <ProtectedRoute>
                <MemberReportPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/proposals"
            element={
              <ProtectedRoute requireAdmin={true}>
                <ProposalsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/proposals/generate"
            element={
              <ProtectedRoute requireAdmin={true}>
                <GenerateProposalPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/proposals/:id"
            element={
              <ProtectedRoute requireAdmin={true}>
                <ProposalDetailPage />
              </ProtectedRoute>
            }
          />

          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}
export default App;

