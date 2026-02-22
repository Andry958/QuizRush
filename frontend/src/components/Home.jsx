import Layout from './Layout'
import './Home.css'

function Home() {
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
            <button className="btn-primary">
              Create Quiz
            </button>
            <button className="btn-secondary">
              Join Game
            </button>
          </div>
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
