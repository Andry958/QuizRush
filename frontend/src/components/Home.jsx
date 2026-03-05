import { useState, useEffect } from 'react'
import axios from 'axios'
import { API_BASE_URL } from '../context/ApiContext'
import noImage from '../assets/no-image.jpg'
import { Link } from 'react-router-dom'
import Layout from './Layout'
import './Home.css'

function Home() {
  const [recommendedQuizzes, setRecommendedQuizzes] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchRecommended = async () => {
      try {
        const response = await axios.get(`${API_BASE_URL}/Quizzes`)
        // Take latest 10 quizzes
        const latest = response.data.reverse().slice(0, 10)
        setRecommendedQuizzes(latest)
      } catch (error) {
        console.error('Error fetching recommended quizzes:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchRecommended()
  }, [])
  return (
    <>
      <section className="hero-section">
        <div className="hero-content">
          <h2 className="hero-title">Test Your Knowledge, Challenge Your Friends</h2>
          <p></p>
          <p className="hero-subtitle">

            Create custom quizzes, join game sessions.
            Improve your knowledge and have fun!
          </p>
          <div className="hero-buttons">
            <Link to="/admin/quizzes/create" className="btn-primary" style={{ textDecoration: 'none' }}>
              Create Quiz
            </Link>
            <Link to="/quizzes/join" className="btn-secondary" style={{ textDecoration: 'none' }}>
              Join Game
            </Link>
          </div>
        </div>
      </section>

      <section className="recommendations-section">
        <div className="section-header-row">
          <h2 className="section-title-small">Featured Quizzes</h2>
          <Link to="/quizzes/join" className="view-all-link">View All →</Link>
        </div>
        <div className="quiz-ribbon-container">
          {loading ? (
            <div className="ribbon-loading">Loading quizzes...</div>
          ) : (
            <div className="quiz-ribbon">
              {recommendedQuizzes.map((quiz) => (
                <Link key={quiz.id} to={`/quiz/${quiz.id}`} className="ribbon-card">
                  <div className="ribbon-card-image">
                    <img src={quiz.imageUrl || noImage} alt={quiz.title} />
                  </div>
                  <div className="ribbon-card-content">
                    <h3>{quiz.title}</h3>
                    <p>{quiz.description || 'No description'}</p>
                  </div>
                </Link>
              ))}
              {recommendedQuizzes.length === 0 && !loading && (
                <div className="no-quizzes-message">No quizzes available yet.</div>
              )}
            </div>
          )}
        </div>
      </section>

      <section id="features" className="features-section">
        <h2 className="section-title">Why Choose QuizRush?</h2>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">📝</div>
            <h3>Create Custom Quizzes</h3>
            <p>Build your own quizzes with multiple choice questions and share them with others.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">🎮</div>
            <h3>Game Sessions</h3>
            <p>Jqoin game sessions and compete against other players in exciting matches.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">🏆</div>
            <h3>Track Your Progress</h3>
            <p>Monitor your performance, view game history, and see how you rank among players.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">👥</div>
            <h3>Multiplayer Fun</h3>
            <p>Play with friends or challenge players worldwide in competitive quiz battles.</p>
          </div>
        </div>
      </section>

      <section id="about" className="about-section">
        <div className="about-content">
          <h2 className="section-title">About QuizRush</h2>
          <p className="about-text">
            QuizRush is a modern, interactive quiz platform designed to make learning and competition fun.
            Whether you're an educator creating engaging content, a student testing your knowledge,
            or a trivia enthusiast looking for a challenge, QuizRush provides the tools and platform
            for an exceptional quiz experience.
          </p>
        </div>
      </section>
    </>
  )
}

export default Home
