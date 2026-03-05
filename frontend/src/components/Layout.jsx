import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useApi } from '../context/ApiContext'
import { useState, useEffect, useRef } from 'react'
import './Layout.css'

function Layout({ children }) {
  const { user, logout } = useAuth()
  const { axiosInstance } = useApi()
  const navigate = useNavigate()

  const [query, setQuery] = useState('')
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)
  const debounceRef = useRef(null)


  // Функція пошуку
  const fetchSearch = async (searchTerm) => {
    if (!searchTerm.trim()) {
      setResults([])
      return
    }
    setLoading(true)
    try {
      const res = await axiosInstance.get(`/quizzes?search=${encodeURIComponent(searchTerm)}`)
      setResults(res.data)
    } catch (err) {
      console.error('Search error:', err)
    } finally {
      setLoading(false)
    }
  }

  // Debounce при вводі
  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      fetchSearch(query)
    }, 300)
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [query])

  const handleSelect = (id) => {
    navigate(`/quiz/${id}`)
    setQuery('')
    setResults([])
  }

  // Enter → сторінка search
  const handleSubmit = (e) => {
    e.preventDefault()
    if (!query.trim()) return
    navigate(`/search?q=${encodeURIComponent(query)}`)
    setResults([])
  }

  return (
    <div className="layout-container">
      <header className="layout-header">
        <div className="header-content">
          <Link to="/" className="logo-link">
            <h1 className="logo">QuizRush</h1>
          </Link>

          <form className="search-wrapper" onSubmit={handleSubmit}>
            <input
              type="text"
              placeholder="Search quizzes..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              className="search-input"
            />

            {query.trim() && (
              loading ? (
                <div className="loading">Завантаження...</div>
              ) : results.length > 0 ? (
                <ul className="search-dropdown">
                  {results.map((quiz) => (
                    <li key={quiz.id} onClick={() => handleSelect(quiz.id)}>
                      {quiz.title}
                    </li>
                  ))}
                </ul>
              ) : (
                <div className="no-results">Сорян, таких квізів немає 😕</div>
              )
            )}
          </form>

          <nav className="nav-menu">
            <Link to="/" className="nav-link">Home</Link>
            <a href="#features" className="nav-link">Features</a>
            <a href="#about" className="nav-link">About</a>

            {user ? (
              <div className="user-nav">
                <Link to="/admin/quizzes" className="nav-link" style={{ marginRight: '1.5rem' }}>Admin</Link>
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

      <main className="layout-main">{children}</main>

      <footer className="layout-footer">
        <p>&copy; 2026 QuizRush. All rights reserved.</p>
      </footer>
    </div>
  )
}

export default Layout