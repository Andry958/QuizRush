import { useLocation, useNavigate } from 'react-router-dom';
import './QuizResults.css';

const QuizResults = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { score, total } = location.state || { score: 0, total: 0 };
    const percentage = total > 0 ? Math.round((score / total) * 100) : 0;

    return (
        <div className="results-container">
            <div className="results-card">
                <h1>Quiz Completed!</h1>
                <div className="score-circle">
                    <span className="score-text">{score} / {total}</span>
                    <span className="percentage-text">{percentage}%</span>
                </div>
                <div className="feedback-text">
                    {percentage >= 80 ? "Excellent job! You're a pro!" :
                        percentage >= 50 ? "Good effort! Keep learning!" :
                            "Better luck next time! Practice makes perfect."}
                </div>
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
