import './Layout.css'

function Layout({ children }) {
  return (
    <div className="layout-container">
      <header className="layout-header">
        <div className="header-content">
          <h1 className="logo">QuizRush</h1>
          <nav className="nav-menu">
            <a href="#features" className="nav-link">Features</a>
            <a href="#about" className="nav-link">About</a>
            <button className="btn-login">Login</button>
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
