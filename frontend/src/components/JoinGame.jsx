import { useState, useEffect } from 'react';
import axios from 'axios';
import { API_BASE_URL } from '../context/ApiContext';
import { useNavigate } from 'react-router-dom';
import noImage from '../assets/no-image.jpg';
import './JoinGame.css';

const JoinGame = () => {
    const [quizzes, setQuizzes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        fetchQuizzes();
    }, []);

    const fetchQuizzes = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`${API_BASE_URL}/Quizzes`);
            setQuizzes(response.data);
            setLoading(false);
        } catch (err) {
            console.error('Error fetching quizzes:', err);
            setError('Failed to load quizzes.');
            setLoading(false);
        }
    };

    const handlePlay = (quizId) => {
        navigate(`/quizzes/play/${quizId}`);
    };

    if (loading) return <div className="join-container"><p>Loading quizzes...</p></div>;
    if (error) return <div className="join-container"><p className="error">{error}</p></div>;

    return (
        <div className="join-container">
            <h1>Select a Quiz to Play</h1>
            <div className="quiz-grid">
                {quizzes.length === 0 ? (
                    <p>No quizzes available yet.</p>
                ) : (
                    quizzes.map(quiz => (
                        <div key={quiz.id} className="quiz-card" onClick={() => handlePlay(quiz.id)}>
                            <div className="quiz-image">
                                <img src={quiz.imageUrl || noImage} alt={quiz.title} />
                            </div>
                            <div className="quiz-content">
                                <h3>{quiz.title}</h3>
                                <p>{quiz.description || 'No description'}</p>
                                <button className="btn-play">Play Now</button>
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default JoinGame;
