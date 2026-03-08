import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../context/ApiContext';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import { useBlockchain } from '../context/BlockchainContext';
import './QuizPlayer.css';

const QuizPlayer = () => {
    const { quizId } = useParams();
    const navigate = useNavigate();
    const { user, accessToken, loading: authLoading } = useAuth();
    const { account, connectWallet, checkPaymentStatus, payForQuiz, loading: blockchainLoading, error: blockchainError } = useBlockchain();

    const [hasPaid, setHasPaid] = useState(false);
    const [checkingPayment, setCheckingPayment] = useState(true);

    const [quiz, setQuiz] = useState(null);
    const [questions, setQuestions] = useState([]);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [correctAnswers, setCorrectAnswers] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [selectedAnswerId, setSelectedAnswerId] = useState(null);
    const [showCorrection, setShowCorrection] = useState(false);

    const [timeLeft, setTimeLeft] = useState(30);
    const [timerActive, setTimerActive] = useState(false);
    const [startTime] = useState(Date.now());
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        if (!authLoading && !accessToken) {
            navigate('/login', { state: { from: `/quizzes/${quizId}` } });
        }
    }, [authLoading, accessToken, navigate, quizId]);

    useEffect(() => {
        if (quizId && account) {
            verifyPayment();
        } else if (!blockchainLoading && !account) {
            setCheckingPayment(false);
        }
    }, [quizId, account, blockchainLoading]);

    const verifyPayment = async () => {
        try {
            setCheckingPayment(true);
            const paid = await checkPaymentStatus(parseInt(quizId));
            setHasPaid(paid);
        } catch (err) {
            console.error("Error verifying payment:", err);
        } finally {
            setCheckingPayment(false);
        }
    };

    const handlePayment = async () => {
        if (!account) {
            await connectWallet();
            return;
        }
        const success = await payForQuiz(parseInt(quizId));
        if (success) {
            setHasPaid(true);
        }
    };

    useEffect(() => {
        if (hasPaid) {
            fetchQuizDetails();
        }
    }, [quizId, hasPaid]);

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
            // Quiz finished - save to leaderboard
            saveQuizAttempt(score + (wasCorrect ? 1 : 0), correctAnswers + (wasCorrect ? 1 : 0));
        }
    };

    const saveQuizAttempt = async (finalScore, finalCorrectAnswers) => {
        if (isSubmitting) return;

        if (!accessToken) {
            console.error('No access token available');
            navigate('/login', { state: { from: `/quizzes/${quizId}` } });
            return;
        }

        setIsSubmitting(true);

        try {
            const duration = Math.round((Date.now() - startTime) / 1000); // Duration in seconds

            const response = await axios.post(
                `${API_BASE_URL}/leaderboard/attempt`,
                {
                    quizId: parseInt(quizId),
                    score: finalScore,
                    correctAnswers: finalCorrectAnswers,
                    totalQuestions: questions.length,
                    duration: duration,
                    rating: 0 // Default rating, user can rate later
                },
                {
                    headers: {
                        Authorization: `Bearer ${accessToken}`
                    }
                }
            );

            console.log('Quiz attempt saved successfully:', response.data);

            // Navigate to results with data
            navigate('/quizzes/results', {
                state: {
                    score: finalScore,
                    total: questions.length,
                    quizId: parseInt(quizId),
                    duration: duration,
                    correctAnswers: finalCorrectAnswers,
                    attemptId: response.data.id
                }
            });
        } catch (err) {
            console.error('Error saving quiz attempt:', err);
            const serverError = err.response?.data?.details || err.response?.data?.message || 'Failed to save to leaderboard';
            console.error('Server error details:', serverError);

            if (err.response?.status === 401) {
                console.error('Token expired or invalid. Redirecting to login.');
                navigate('/login', { state: { from: `/quizzes/${quizId}`, message: 'Your session has expired. Please login again.' } });
                return;
            }

            navigate('/quizzes/results', {
                state: {
                    score: finalScore,
                    total: questions.length,
                    quizId: parseInt(quizId),
                    correctAnswers: finalCorrectAnswers,
                    error: serverError
                }
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    const fetchQuizDetails = async () => {
        try {
            setLoading(true);
            const quizResponse = await axios.get(`${API_BASE_URL}/Quizzes/${quizId}`);
            setQuiz(quizResponse.data);

            const questionsResponse = await axios.get(`${API_BASE_URL}/Questions/quiz/${quizId}`);
            const qData = await Promise.all(questionsResponse.data.map(async (q) => {
                const answersResponse = await axios.get(`${API_BASE_URL}/Answers/question/${q.id}`);
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
            setCorrectAnswers(correctAnswers + 1);
        }

        setShowCorrection(true);

        setTimeout(() => {
            moveToNextQuestion(isCorrect);
        }, 1500);
    };

    if (authLoading || checkingPayment || (blockchainLoading && !account))
        return <div className="player-container"><div className="loading-card"><p>Checking status...</p></div></div>;

    if (!account) {
        return (
            <div className="player-container">
                <div className="payment-card">
                    <h3>Blockchain Connection Required</h3>
                    <p>To participate in this quiz, you need to connect your MetaMask wallet and pay the entry fee.</p>
                    <button className="btn-connect" onClick={connectWallet}>Connect Wallet</button>
                    {blockchainError && <p className="error-text">{blockchainError}</p>}
                </div>
            </div>
        );
    }

    if (!hasPaid) {
        return (
            <div className="player-container">
                <div className="payment-card">
                    <h3>Entry Fee Required</h3>
                    <p>This quiz requires a 1 ETH entry fee to participate.</p>
                    <div className="payment-details">
                        <span>Fee: 1.0 ETH</span>
                        <span>Quiz ID: {quizId}</span>
                    </div>
                    <button className="btn-pay" onClick={handlePayment} disabled={blockchainLoading}>
                        {blockchainLoading ? 'Processing...' : 'Pay 1 ETH to Start'}
                    </button>
                    {blockchainError && <p className="error-text">{blockchainError}</p>}
                </div>
            </div>
        );
    }

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
