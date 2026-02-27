import { useLocation, Link } from 'react-router-dom'
import { useApi } from '../context/ApiContext'
import { useEffect, useState } from 'react'
import './SearchResults.css'

function SearchResults() {
  const { axiosInstance } = useApi()
  const location = useLocation()
  const params = new URLSearchParams(location.search)
  const query = params.get('q') || ''

  // convert question count ➜ star string (out of 5)
  const getStars = (count=0) => {
    const levels = [5,10,15,20] // thresholds
    let n = 1
    for (let i=0;i<levels.length;i++) if (count > levels[i]) n = i+2
    if (n>5) n=5
    return '★'.repeat(n) + '☆'.repeat(5-n)
  }

  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!query.trim()) return

    setLoading(true)
    axiosInstance.get(`/quizzes?search=${encodeURIComponent(query)}`)
      .then(res => setResults(res.data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false))
  }, [query, axiosInstance])

  return (
    <div className="sr-wrap">
      <h2 className="sr-title">Результати пошуку: "{query}"</h2>

      {loading && <div className="sr-loading">Завантаження...</div>}

      {!loading && results.length === 0 && (
        <div className="sr-empty">Сорян 😕 Квізів за вашим запитом не знайдено</div>
      )}

      {!loading && results.length > 0 && (
        <div className="sr-grid">
          {results.map(q => {
            const count = (q.questions?.length) || 0
            const stars = getStars(count)
            return (
              <article key={q.id} className="sr-card">
                <div className="sr-card-body">
                  <div className="sr-stars">{stars}</div>
                  <h3 className="sr-card-title">
                    <Link to={`/quiz/${q.id}`} className="sr-link">{q.title}</Link>
                  </h3>
                  { (q.description || q.summary) && <p className="sr-desc">{q.description ?? q.summary}</p> }
                </div>

                <div className="sr-card-actions">
                  <Link to={`/quiz/${q.id}`} className="btn btn-sm">Open Quiz</Link>
                  <Link to={`/create-room?quizId=${encodeURIComponent(q.id)}`} className="btn btn-sm primary">Create match room</Link>
                </div>
              </article>
            )
          })}
        </div>
      )}
    </div>
  )
}

export default SearchResults;