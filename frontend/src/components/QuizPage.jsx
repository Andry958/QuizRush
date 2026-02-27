import { useParams } from 'react-router-dom'
import { useApi } from '../context/ApiContext'
import { useState, useEffect } from 'react'

function QuizPage() {
  const { id } = useParams()
  const { axiosInstance } = useApi()

  const [quiz, setQuiz] = useState(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    axiosInstance.get(`/quizzes/${id}`)
      .then(res => setQuiz(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [id, axiosInstance])

  if (loading) return <p>Завантаження...</p>
  if (!quiz) return <p>Квіз не знайдено</p>

  return (
    <div style={{ padding: '40px' }}>
      <h2>{quiz.title}</h2>
      {quiz.description && <p>{quiz.description}</p>}
    </div>
  )
}

export default QuizPage

