import { useLocation, useNavigate } from 'react-router-dom';
import './QuizResults.css';

const QuizResults = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { 
        score, 
        total, 
        quizId, 
        duration = 0, 
        correctAnswers = 0,
        attemptId = null,
        error = null
    } = location.state || { score: 0, total: 0 };
    
    const percentage = total > 0 ? Math.round((score / total) * 100) : 0;
    const minutes = Math.floor(duration / 60);
    const seconds = duration % 60;

    return (
        <div className="results-container">
            <div className="results-card">
                <h1>✅ Quiz Completed!</h1>
                <div className="score-circle">
                    <span className="score-text">{score} / {total}</span>
                    <span className="percentage-text">{percentage}%</span>
                </div>
                <div className="feedback-text">
                    {percentage >= 80 ? "🎉 Excellent job! You're a pro!" :
                        percentage >= 50 ? "👍 Good effort! Keep learning!" :
                            "💪 Better luck next time! Practice makes perfect."}
                </div>

                <div className="results-details">
                    <div className="detail-item">
                        <span className="detail-label">Correct Answers:</span>
                        <span className="detail-value">{correctAnswers} / {total}</span>
                    </div>
                    <div className="detail-item">
                        <span className="detail-label">Time Spent:</span>
                        <span className="detail-value">{minutes}m {seconds}s</span>
                    </div>
                    {attemptId && (
                        <div className="detail-item">
                            <span className="detail-label">Attempt ID:</span>
                            <span className="detail-value">#{attemptId}</span>
                        </div>
                    )}
                </div>

                {error && (
                    <div className="error-message">
                        ⚠️ {error}
                    </div>
                )}

                <div className="results-actions">
                    <button className="btn-primary" onClick={() => navigate('/quizzes/join')}>
                        Try Another Quiz
                    </button>
                    <button className="btn-secondary" onClick={() => navigate('/')}>
                        Back to Home
                    </button>
                </div>
            </div>
        </div>
    );
};

export default QuizResults;
