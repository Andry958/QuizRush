import { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import { API_BASE_URL } from '../context/ApiContext';
import noImage from '../assets/no-image.jpg';
import './AdminQuizList.css';

const AdminQuizList = () => {
    const [quizzes, setQuizzes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

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
            setError('Failed to load quizzes. Please check if the backend is running.');
            setLoading(false);
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Are you sure you want to delete this quiz?')) {
            try {
                await axios.delete(`${API_BASE_URL}/Quizzes/${id}`);
                setQuizzes(quizzes.filter(q => q.id !== id));
            } catch (err) {
                console.error('Error deleting quiz:', err);
                alert('Failed to delete quiz.');
            }
        }
    };

    if (loading) return <div className="admin-status">Loading quizzes...</div>;
    if (error) return <div className="admin-status error">{error}</div>;

    return (
        <div className="admin-container">
            <div className="admin-header">
                <h1>Manage Quizzes</h1>
                <Link to="/admin/quizzes/create" className="btn-create">
                    <span className="icon">+</span> Create New Quiz
                </Link>
            </div>

            <div className="quiz-list">
                {quizzes.length === 0 ? (
                    <p className="no-quizzes">No quizzes found. Start by creating one!</p>
                ) : (
                    quizzes.map(quiz => (
                        <div key={quiz.id} className="quiz-item">
                            <div className="quiz-info">
                                <div className="quiz-image-container">
                                    <img
                                        src={quiz.imageUrl || noImage}
                                        alt={quiz.title}
                                        className="quiz-thumbnail"
                                    />
                                </div>
                                <div className="quiz-details">
                                    <h3>{quiz.title}</h3>
                                    <p>{quiz.description || 'No description available.'}</p>
                                </div>
                            </div>
                            <div className="quiz-actions">
                                <Link to={`/admin/quizzes/edit/${quiz.id}`} className="btn-action edit">
                                    Edit
                                </Link>
                                <button
                                    onClick={() => handleDelete(quiz.id)}
                                    className="btn-action delete"
                                >
                                    Delete
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default AdminQuizList;
