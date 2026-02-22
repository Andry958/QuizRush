import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import './Layout.css'

function Layout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };
  return (
    <div className="layout-container">
      <header className="layout-header">
        <div className="header-content">
          <Link to="/" className="logo-link">
            <h1 className="logo">QuizRush</h1>
          </Link>
          <nav className="nav-menu">
            <Link to="/" className="nav-link">Home</Link>
            <a href="#features" className="nav-link">Features</a>
            <a href="#about" className="nav-link">About</a>
            {user ? (
              <div className="user-nav">
                <Link to="/profile" className="avatar-link">
                  <div className="header-avatar">
                    {user.nickname?.charAt(0).toUpperCase() || 'U'}
                  </div>
                </Link>
              </div>
            ) : (
              <div className="auth-nav">
                <Link to="/login" className="btn-login-link">
                  <button className="btn-login">Login</button>
                </Link>
              </div>
            )}
          </nav>
        </div>
      </header>

      <main className="layout-main">
        {children}
      </main>

      <footer className="layout-footer">
        <p>&copy; 2026 QuizRush. All rights reserved.</p>
      </footer>
    </div>
  )
}

export default Layout
