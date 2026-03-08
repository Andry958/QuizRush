import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { API_BASE_URL } from '../context/ApiContext';
import './Leaderboard.css';

const Leaderboard = () => {
    const [players, setPlayers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchLeaderboard();
    }, []);

    const fetchLeaderboard = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`${API_BASE_URL}/Leaderboard/top10`);
            setPlayers(response.data);
            setLoading(false);
        } catch (err) {
            console.error('Error fetching leaderboard:', err);
            setError('Failed to load leaderboard data.');
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="loading-container">
                <span className="loader"></span>
                <p>Loading Champions...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="leaderboard-container">
                <div className="leaderboard-card">
                    <p className="error-message">⚠️ {error}</p>
                    <button className="btn-primary" onClick={fetchLeaderboard}>Retry</button>
                </div>
            </div>
        );
    }

    const podium = players.slice(0, 3);
    const tablePlayers = players.slice(3);

    // Sort podium to show 2nd, 1st, 3rd visually
    const sortedPodium = [];
    if (podium[1]) sortedPodium.push({ ...podium[1], rank: 2 });
    if (podium[0]) sortedPodium.push({ ...podium[0], rank: 1 });
    if (podium[2]) sortedPodium.push({ ...podium[2], rank: 3 });

    return (
        <div className="leaderboard-container">
            <div className="leaderboard-card">
                <div className="leaderboard-header">
                    <h1>Global Leaderboard</h1>
                    <p>The best players of QuizRush community</p>
                </div>

                {players.length > 0 ? (
                    <>
                        <div className="podium">
                            {sortedPodium.map((player) => (
                                <div key={player.userId} className={`podium-item rank-${player.rank}`}>
                                    <div className="podium-rank">{player.rank}</div>
                                    <div className="podium-avatar">
                                        {player.userName?.charAt(0).toUpperCase() || 'U'}
                                    </div>
                                    <div className="podium-info">
                                        <span className="podium-name">{player.userName}</span>
                                        <span className="podium-score">{player.averageScore} pts</span>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="leaderboard-table-wrapper">
                            <table className="leaderboard-table">
                                <thead>
                                    <tr>
                                        <th>Rank</th>
                                        <th>Player</th>
                                        <th>Games</th>
                                        <th>Avg Score</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tablePlayers.map((player, index) => (
                                        <tr key={player.userId} className="leaderboard-row">
                                            <td>{index + 4}</td>
                                            <td>
                                                <div className="player-cell">
                                                    <div className="player-mini-avatar">
                                                        {player.userName?.charAt(0).toUpperCase() || 'U'}
                                                    </div>
                                                    <span className="player-name">{player.userName}</span>
                                                </div>
                                            </td>
                                            <td className="stats-cell">{player.totalAttempts} games</td>
                                            <td className="score-cell">{player.averageScore} pts</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </>
                ) : (
                    <div className="no-data">
                        <p>No rankings available yet. Be the first to top the charts!</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Leaderboard;
