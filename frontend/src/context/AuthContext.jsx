import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';
import { API_BASE_URL } from './ApiContext';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [accessToken, setAccessToken] = useState(localStorage.getItem('accessToken'));
    const [refreshToken, setRefreshToken] = useState(localStorage.getItem('refreshToken'));
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (accessToken) {
            const storedNickname = localStorage.getItem('nickname');
            const storedEmail = localStorage.getItem('userEmail');
            setUser({ email: storedEmail, nickname: storedNickname });
        }
        setLoading(false);
    }, [accessToken]);

    const login = async (email, password) => {
        try {
            const response = await axios.post(`${API_BASE_URL}/api/Auth/login`, { email, password });
            const { accessToken: newAccessToken, refreshToken: newRefreshToken, nickname } = response.data;

            setAccessToken(newAccessToken);
            setRefreshToken(newRefreshToken);
            localStorage.setItem('accessToken', newAccessToken);
            localStorage.setItem('refreshToken', newRefreshToken);
            localStorage.setItem('nickname', nickname);
            localStorage.setItem('userEmail', email);
            setUser({ email, nickname });
            return { success: true };
        } catch (error) {
            console.error('Login error:', error);
            return { success: false, error: error.response?.data || 'Login failed' };
        }
    };

    const register = async (email, nickname, password) => {
        try {
            await axios.post(`${API_BASE_URL}/api/Auth/register`, { email, nickname, password });
            return { success: true };
        } catch (error) {
            console.error('Registration error:', error);
            return { success: false, error: error.response?.data || 'Registration failed' };
        }
    };

    const logout = () => {
        setAccessToken(null);
        setRefreshToken(null);
        setUser(null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('nickname');
        localStorage.removeItem('userEmail');
    };

    const value = {
        user,
        accessToken,
        loading,
        login,
        register,
        logout
    };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
