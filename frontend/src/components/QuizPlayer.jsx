import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import './QuizPlayer.css';

const QuizPlayer = () => {
    const { quizId } = useParams();
    const navigate = useNavigate();
    const [quiz, setQuiz] = useState(null);
    const [questions, setQuestions] = useState([]);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [selectedAnswerId, setSelectedAnswerId] = useState(null);
    const [showCorrection, setShowCorrection] = useState(false);

    const [timeLeft, setTimeLeft] = useState(30);
    const [timerActive, setTimerActive] = useState(false);

    useEffect(() => {
        fetchQuizDetails();
    }, [quizId]);

    useEffect(() => {
        if (questions.length > 0 && currentQuestionIndex < questions.length) {
            const time = questions[currentQuestionIndex].timeLimit || 30;
            setTimeLeft(time);
            setTimerActive(true);
        }
    }, [currentQuestionIndex, questions]);

    useEffect(() => {
        let timer;
        if (timerActive && timeLeft > 0 && !showCorrection) {
            timer = setInterval(() => {
                setTimeLeft(prev => prev - 1);
            }, 1000);
        } else if (timeLeft === 0 && !showCorrection) {
            handleTimeUp();
        }
        return () => clearInterval(timer);
    }, [timerActive, timeLeft, showCorrection]);

    const handleTimeUp = () => {
        setTimerActive(false);
        const currentQuestion = questions[currentQuestionIndex];
        const correctAnswer = currentQuestion.answers.find(a => a.isCorrect);

        // If no answer selected, we just show the correct one
        setShowCorrection(true);

        setTimeout(() => {
            moveToNextQuestion(false);
        }, 2000);
    };

    const moveToNextQuestion = (wasCorrect) => {
        if (currentQuestionIndex + 1 < questions.length) {
            setCurrentQuestionIndex(currentQuestionIndex + 1);
            setSelectedAnswerId(null);
            setShowCorrection(false);
            setTimerActive(true);
        } else {
            navigate('/quizzes/results', {
                state: {
                    score: score + (wasCorrect ? 1 : 0),
                    total: questions.length
                }
            });
        }
    };

    const fetchQuizDetails = async () => {
        try {
            setLoading(true);
            const quizResponse = await axios.get(`http://localhost:5026/api/Quizzes/${quizId}`);
            setQuiz(quizResponse.data);

            const questionsResponse = await axios.get(`http://localhost:5026/api/Questions/quiz/${quizId}`);
            const qData = await Promise.all(questionsResponse.data.map(async (q) => {
                const answersResponse = await axios.get(`http://localhost:5026/api/Answers/question/${q.id}`);
                return { ...q, answers: answersResponse.data };
            }));

            setQuestions(qData);
            setLoading(false);
        } catch (err) {
            console.error('Error fetching quiz details:', err);
            setError('Failed to load quiz.');
            setLoading(false);
        }
    };

    const handleAnswerSelect = (answerId) => {
        if (showCorrection || timeLeft === 0) return;
        setSelectedAnswerId(answerId);
    };

    const handleNext = () => {
        if (selectedAnswerId === null || showCorrection) return;

        setTimerActive(false);
        const currentQuestion = questions[currentQuestionIndex];
        const selectedAnswer = currentQuestion.answers.find(a => a.id === selectedAnswerId);

        const isCorrect = selectedAnswer.isCorrect;
        if (isCorrect) {
            setScore(score + 1);
        }

        setShowCorrection(true);

        setTimeout(() => {
            moveToNextQuestion(isCorrect);
        }, 1500);
    };

    if (loading) return <div className="player-container"><p>Preparing quiz...</p></div>;
    if (error) return <div className="player-container"><p className="error">{error}</p></div>;
    if (questions.length === 0) return <div className="player-container"><p>This quiz has no questions.</p></div>;

    const currentQuestion = questions[currentQuestionIndex];

    return (
        <div className="player-container">
            <div className="player-card">
                <div className="player-header">
                    <h2>{quiz.title}</h2>
                    <div className="player-stats">
                        <div className="progress">Question {currentQuestionIndex + 1} of {questions.length}</div>
                        <div className={`timer ${timeLeft < 10 ? 'warning' : ''}`}>
                            Time: {timeLeft}s
                        </div>
                    </div>
                </div>

                {currentQuestion.imageUrl && (
                    <div className="question-image-container">
                        <img src={currentQuestion.imageUrl} alt="Question" className="question-image" />
                    </div>
                )}

                <div className="question-text">
                    {currentQuestion.text}
                </div>

                <div className="answers-grid">
                    {currentQuestion.answers.map(answer => {
                        let className = "answer-button";
                        if (selectedAnswerId === answer.id) className += " selected";
                        if (showCorrection) {
                            if (answer.isCorrect) className += " correct";
                            else if (selectedAnswerId === answer.id) className += " incorrect";
                        }

                        return (
                            <button
                                key={answer.id}
                                className={className}
                                onClick={() => handleAnswerSelect(answer.id)}
                                disabled={showCorrection || timeLeft === 0}
                            >
                                {answer.text}
                            </button>
                        );
                    })}
                </div>

                <div className="player-actions">
                    <button
                        className="btn-next"
                        onClick={handleNext}
                        disabled={selectedAnswerId === null || showCorrection || timeLeft === 0}
                    >
                        {currentQuestionIndex + 1 === questions.length ? 'Finish' : 'Next Question'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default QuizPlayer;
