import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { API_BASE_URL } from '../context/ApiContext';
import axios from 'axios';
import './Profile.css';

const Profile = () => {
    const { user, logout } = useAuth();
    const [stats, setStats] = useState({
        quizzesCreated: 0,
        gamesPlayed: 0,
        totalScore: 0
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (user && user.id) {
            fetchUserStats();
        }
    }, [user]);

    const fetchUserStats = async () => {
        try {
            setLoading(true);
            const response = await axios.get(`${API_BASE_URL}/Quizzes/user/${user.id}`);
            setStats(prev => ({
                ...prev,
                quizzesCreated: response.data.length
            }));
            setLoading(false);
        } catch (err) {
            console.error('Error fetching user stats:', err);
            setLoading(false);
        }
    };

    if (!user) {
        return (
            <div className="profile-container">
                <div className="profile-card">
                    <p>Please login to view your profile.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="profile-container">
            <div className="profile-card">
                <div className="profile-header">
                    <div className="profile-avatar">
                        {user.nickname?.charAt(0).toUpperCase() || 'U'}
                    </div>
                    <h2>{user.nickname}</h2>
                    <p className="profile-email">{user.email}</p>
                </div>

                <div className="profile-stats">
                    <div className="stat-item">
                        <span className="stat-value">{loading ? '...' : stats.quizzesCreated}</span>
                        <span className="stat-label">Quizzes Created</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-value">{stats.gamesPlayed}</span>
                        <span className="stat-label">Games Played</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-value">{stats.totalScore}</span>
                        <span className="stat-label">Total Score</span>
                    </div>
                </div>

                <div className="profile-actions">
                    <button className="btn-edit-profile">Edit Profile</button>
                    <button onClick={logout} className="btn-logout-alt">Logout</button>
                </div>
            </div>
        </div>
    );
};

export default Profile;
