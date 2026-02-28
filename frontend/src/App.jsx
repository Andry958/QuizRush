import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Home from './components/Home'
import Login from './components/Login'
import Register from './components/Register'
import Profile from './components/Profile'
import AdminQuizList from './components/AdminQuizList'
import CreateQuiz from './components/CreateQuiz'
import JoinGame from './components/JoinGame'
import QuizPlayer from './components/QuizPlayer'
import QuizResults from './components/QuizResults'
import Layout from './components/Layout'
import { AuthProvider } from './context/AuthContext'
import './App.css'

function App() {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/admin/quizzes" element={<AdminQuizList />} />
            <Route path="/admin/quizzes/create" element={<CreateQuiz />} />
            <Route path="/admin/quizzes/edit/:id" element={<CreateQuiz />} />
            <Route path="/quizzes/join" element={<JoinGame />} />
            <Route path="/quizzes/play/:quizId" element={<QuizPlayer />} />
            <Route path="/quizzes/results" element={<QuizResults />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  )
}

export default App
