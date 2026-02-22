import React from 'react';
import { useAuth } from '../context/AuthContext';
import './Profile.css';

const Profile = () => {
    const { user, logout } = useAuth();

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
                        <span className="stat-value">0</span>
                        <span className="stat-label">Quizzes Created</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-value">0</span>
                        <span className="stat-label">Games Played</span>
                    </div>
                    <div className="stat-item">
                        <span className="stat-value">0</span>
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
