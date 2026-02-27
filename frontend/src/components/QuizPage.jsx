import { useParams } from 'react-router-dom'
import { useApi } from '../context/ApiContext'
import { useState, useEffect } from 'react'
import './QuizPage.css'

function QuizPage() {
  const { id } = useParams()
  const { axiosInstance } = useApi()

  const [quiz, setQuiz] = useState(null)
  const [loading, setLoading] = useState(false)

  const [current, setCurrent] = useState(0)
  const [answers, setAnswers] = useState([])
  const [showResults, setShowResults] = useState(false)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    axiosInstance.get(`/quizzes/${id}`)
      .then(res => {
        setQuiz(res.data)
        setAnswers(new Array(res.data?.questions?.length || 0).fill(null))
        setCurrent(0)
        setShowResults(false)
      })
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [id, axiosInstance])

  if (loading) return <div className="qp-center">Завантаження...</div>
  if (!quiz) return <div className="qp-center">Квіз не знайдено</div>

  const questions = quiz.questions || []

  const selectOption = (qIdx, optIdx) => {
    setAnswers(a => {
      const copy = [...a]
      copy[qIdx] = optIdx
      return copy
    })
  }

  const next = () => {
    if (current < questions.length - 1) setCurrent(c => c + 1)
    else finish()
  }

  const prev = () => {
    if (current > 0) setCurrent(c => c - 1)
  }

  const finish = () => {
    setShowResults(true)
  }

  const computeScore = () => {
    if (!questions.length) return { correct: 0, total: 0 }
    let correct = 0
    for (let i = 0; i < questions.length; i++) {
      const q = questions[i]
      const sel = answers[i]
      if (sel == null) continue
      // Best-effort correctness check: compare against common keys
      if (q.correctOptionIndex !== undefined) {
        if (q.correctOptionIndex === sel) correct++
      } else if (q.correctAnswer !== undefined) {
        const opt = q.options?.[sel]
        const val = opt?.id ?? opt?.value ?? opt ?? sel
        if (val === q.correctAnswer) correct++
      } else if (q.correctOptionId !== undefined) {
        const opt = q.options?.[sel]
        if (opt?.id === q.correctOptionId) correct++
      }
    }
    return { correct, total: questions.length }
  }

  const { correct, total } = computeScore()

  if (showResults) {
    return (
      <div className="qp-wrap">
        <div className="qp-card results">
          <h2 className="qp-title">{quiz.title}</h2>
          <p className="qp-sub">Результати</p>
          <div className="qp-score">{correct} / {total} правильних</div>

          <ul className="qp-review">
            {questions.map((q, i) => {
              const sel = answers[i]
              const userOpt = q.options?.[sel]
              return (
                <li key={i} className="qp-review-item">
                  <div className="qp-q">{i+1}. {q.text || q.question || 'Запитання'}</div>
                  <div className="qp-your">Ваша відповідь: {userOpt ? (userOpt.title ?? userOpt.text ?? String(userOpt)) : '—'}</div>
                  {/* show correct if available */}
                  { (q.correctAnswer !== undefined || q.correctOptionIndex !== undefined || q.correctOptionId !== undefined) && (
                    <div className="qp-correct">Правильна: {(() => {
                      const idx = q.correctOptionIndex ?? q.correctOptionIndex
                      if (q.correctOptionIndex !== undefined) return q.options?.[q.correctOptionIndex]?.title ?? q.options?.[q.correctOptionIndex]?.text ?? String(q.options?.[q.correctOptionIndex])
                      if (q.correctAnswer !== undefined) return String(q.correctAnswer)
                      if (q.correctOptionId !== undefined) return String(q.correctOptionId)
                      return '—'
                    })()}</div>
                  )}
                </li>
              )
            })}
          </ul>

          <div className="qp-actions">
            <button className="btn" onClick={() => { setShowResults(false); setCurrent(0); }}>Retry</button>
            <a className="btn btn-link" href="/">Back Home</a>
          </div>
        </div>
      </div>
    )
  }

  const q = questions[current]
  const opts = q?.options || []

  return (
    <div className="qp-wrap">
      <div className="qp-card">
        <h2 className="qp-title">{quiz.title}</h2>
        {quiz.description && <p className="qp-sub">{quiz.description}</p>}

        <div className="qp-progress">
          <div className="qp-progress-bar" style={{ width: `${((current+1)/Math.max(1, questions.length))*100}%` }} />
        </div>

        <div className="qp-question">
          <div className="qp-qnum">Питання {current + 1} / {questions.length}</div>
          <div className="qp-qtext">{q?.text ?? q?.question ?? 'Без тексту питання'}</div>

          <div className="qp-options">
            {opts.map((opt, i) => {
              const label = opt?.title ?? opt?.text ?? String(opt)
              const selected = answers[current] === i
              return (
                <button
                  key={i}
                  className={`qp-opt ${selected ? 'selected' : ''}`}
                  onClick={() => selectOption(current, i)}
                >
                  <span className="qp-opt-label">{label}</span>
                </button>
              )
            })}
          </div>

          <div className="qp-nav">
            <button className="btn" onClick={prev} disabled={current === 0}>Prev</button>
            <button className="btn primary" onClick={next} disabled={answers[current] == null}>{current === questions.length - 1 ? 'Finish' : 'Next'}</button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default QuizPage

