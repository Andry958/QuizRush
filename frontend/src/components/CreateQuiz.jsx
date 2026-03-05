import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';
import { API_BASE_URL } from '../context/ApiContext';
import { useAuth } from '../context/AuthContext';
import './CreateQuiz.css';

const CreateQuiz = () => {
    const { user } = useAuth();
    const { id } = useParams();
    const isEditMode = !!id;
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [imageUrl, setImageUrl] = useState('');
    const [questions, setQuestions] = useState([
        { id: null, text: '', imageUrl: '', timeLimit: 30, answers: [{ text: '', isCorrect: true }, { text: '', isCorrect: false }] }
    ]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        if (isEditMode) {
            fetchQuizData();
        }
    }, [id]);

    const fetchQuizData = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`${API_BASE_URL}/Quizzes/${id}`);
            const quiz = response.data;
            setTitle(quiz.title);
            setDescription(quiz.description || '');
            setImageUrl(quiz.imageUrl || '');

            const questionsResponse = await axios.get(`${API_BASE_URL}/Questions/quiz/${id}`);
            const questionsData = await Promise.all(questionsResponse.data.map(async (q) => {
                const answersResponse = await axios.get(`${API_BASE_URL}/Answers/question/${q.id}`);
                return {
                    id: q.id,
                    text: q.text,
                    imageUrl: q.imageUrl || '',
                    timeLimit: q.timeLimit,
                    answers: answersResponse.data.map(a => ({ id: a.id, text: a.text, isCorrect: a.isCorrect }))
                };
            }));

            if (questionsData.length > 0) {
                setQuestions(questionsData);
            }
            setLoading(false);
        } catch (err) {
            console.error('Error fetching quiz data:', err);
            setError('Failed to load quiz data.');
            setLoading(false);
        }
    };

    const handleAddQuestion = () => {
        setQuestions([...questions, { id: null, text: '', imageUrl: '', timeLimit: 30, answers: [{ text: '', isCorrect: true }, { text: '', isCorrect: false }] }]);
    };

    const handleRemoveQuestion = (index) => {
        const newQuestions = questions.filter((_, i) => i !== index);
        setQuestions(newQuestions);
    };

    const handleQuestionChange = (index, field, value) => {
        const newQuestions = [...questions];
        newQuestions[index][field] = value;
        setQuestions(newQuestions);
    };

    const handleAddAnswer = (qIndex) => {
        const newQuestions = [...questions];
        newQuestions[qIndex].answers.push({ text: '', isCorrect: false });
        setQuestions(newQuestions);
    };

    const handleRemoveAnswer = (qIndex, aIndex) => {
        const newQuestions = [...questions];
        newQuestions[qIndex].answers = newQuestions[qIndex].answers.filter((_, i) => i !== aIndex);
        setQuestions(newQuestions);
    };

    const handleAnswerChange = (qIndex, aIndex, field, value) => {
        const newQuestions = [...questions];
        if (field === 'isCorrect' && value === true) {
            newQuestions[qIndex].answers.forEach((ans, i) => {
                ans.isCorrect = i === aIndex;
            });
        } else {
            newQuestions[qIndex].answers[aIndex][field] = value;
        }
        setQuestions(newQuestions);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        try {
            if (!user || !user.id) {
                setError('User session expired or invalid. Please logout and login again.');
                setLoading(false);
                return;
            }

            const quizData = {
                id: isEditMode ? parseInt(id) : 0,
                title,
                description,
                imageUrl,
                createdById: user.id
            };

            console.log('Sending quiz data:', quizData);

            let quizId = id;
            if (isEditMode) {
                await axios.put(`${API_BASE_URL}/Quizzes/${id}`, quizData);
            } else {
                const response = await axios.post(`${API_BASE_URL}/Quizzes`, quizData);
                quizId = response.data.id;
            }

            console.log('Quiz saved with ID:', quizId);

            for (const q of questions) {
                const questionData = {
                    id: q.id || 0,
                    text: q.text,
                    imageUrl: q.imageUrl,
                    quizId: parseInt(quizId),
                    timeLimit: q.timeLimit
                };

                let qId = q.id;
                if (q.id) {
                    await axios.put(`${API_BASE_URL}/Questions/${q.id}`, questionData);
                } else {
                    const qResponse = await axios.post(`${API_BASE_URL}/Questions`, questionData);
                    qId = qResponse.data.id;
                }

                for (const a of q.answers) {
                    const answerData = {
                        id: a.id || 0,
                        text: a.text,
                        isCorrect: a.isCorrect,
                        questionId: parseInt(qId)
                    };

                    if (a.id) {
                        await axios.put(`${API_BASE_URL}/Answers/${a.id}`, answerData);
                    } else {
                        await axios.post(`${API_BASE_URL}/Answers`, answerData);
                    }
                }
            }

            navigate('/admin/quizzes');
        } catch (err) {
            console.error('Error saving quiz:', err);
            if (err.response) {
                console.error('Response data:', err.response.data);
                console.error('Response status:', err.response.status);
            }
            setError(`Failed to save quiz: ${err.response?.data?.message || err.message}`);
        } finally {
            setLoading(false);
        }
    };

    if (loading && isEditMode) return <div className="create-quiz-container"><p>Loading quiz...</p></div>;

    return (
        <div className="create-quiz-container">
            <div className="create-quiz-card full-width">
                <h1>{isEditMode ? 'Edit Quiz' : 'Create New Quiz'}</h1>
                <form onSubmit={handleSubmit} className="quiz-form">
                    <section className="quiz-metadata">
                        <div className="form-group">
                            <label htmlFor="title">Quiz Title</label>
                            <input
                                type="text"
                                id="title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                placeholder="Enter quiz title"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="description">Description</label>
                            <textarea
                                id="description"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                placeholder="Briefly describe what this quiz is about"
                                rows="3"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="imageUrl">Image URL (Optional)</label>
                            <input
                                type="text"
                                id="imageUrl"
                                value={imageUrl}
                                onChange={(e) => setImageUrl(e.target.value)}
                                placeholder="https://example.com/image.jpg"
                            />
                        </div>
                    </section>

                    <hr />

                    <section className="questions-section">
                        <h2>Questions</h2>
                        {questions.map((q, qIndex) => (
                            <div key={qIndex} className="question-block">
                                <div className="question-header">
                                    <h3>Question {qIndex + 1}</h3>
                                    {questions.length > 1 && (
                                        <button type="button" onClick={() => handleRemoveQuestion(qIndex)} className="btn-remove">Remove</button>
                                    )}
                                </div>
                                <div className="form-group">
                                    <input
                                        type="text"
                                        value={q.text}
                                        onChange={(e) => handleQuestionChange(qIndex, 'text', e.target.value)}
                                        placeholder="Enter question text"
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <input
                                        type="text"
                                        value={q.imageUrl || ''}
                                        onChange={(e) => handleQuestionChange(qIndex, 'imageUrl', e.target.value)}
                                        placeholder="Question Image URL (Optional)"
                                    />
                                </div>
                                <div className="form-group-inline">
                                    <label>Time Limit (sec):</label>
                                    <input
                                        type="number"
                                        value={q.timeLimit}
                                        onChange={(e) => handleQuestionChange(qIndex, 'timeLimit', parseInt(e.target.value))}
                                        min="5"
                                        max="300"
                                    />
                                </div>

                                <div className="answers-block">
                                    <h4>Answers</h4>
                                    {q.answers.map((a, aIndex) => (
                                        <div key={aIndex} className="answer-row">
                                            <input
                                                type="radio"
                                                name={`correct-${qIndex}`}
                                                checked={a.isCorrect}
                                                onChange={() => handleAnswerChange(qIndex, aIndex, 'isCorrect', true)}
                                                title="Mark as correct"
                                            />
                                            <input
                                                type="text"
                                                value={a.text}
                                                onChange={(e) => handleAnswerChange(qIndex, aIndex, 'text', e.target.value)}
                                                placeholder={`Answer ${aIndex + 1}`}
                                                required
                                            />
                                            {q.answers.length > 2 && (
                                                <button type="button" onClick={() => handleRemoveAnswer(qIndex, aIndex)} className="btn-remove-small">×</button>
                                            )}
                                        </div>
                                    ))}
                                    <button type="button" onClick={() => handleAddAnswer(qIndex)} className="btn-add-small">+ Add Answer</button>
                                </div>
                            </div>
                        ))}
                        <button type="button" onClick={handleAddQuestion} className="btn-add">Add Question</button>
                    </section>

                    <div className="form-actions">
                        <button
                            type="button"
                            className="btn-create-cancel"
                            onClick={() => navigate('/admin/quizzes')}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="btn-create-submit"
                            disabled={loading}
                        >
                            {loading ? 'Saving...' : (isEditMode ? 'Update Quiz' : 'Create Quiz')}
                        </button>
                    </div>
                    {error && <p className="error-message">{error}</p>}
                </form>
            </div>
        </div>
    );
};

export default CreateQuiz;
