import { useLocation, Link } from 'react-router-dom'
import { useApi } from '../context/ApiContext'
import { useEffect, useState } from 'react'

function SearchResults() {
  const { axiosInstance } = useApi()
  const location = useLocation()
  const params = new URLSearchParams(location.search)
  const query = params.get('q') || ''

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
    <div style={{ padding: '40px' }}>
      <h2>Результати пошуку: "{query}"</h2>

      {loading && <p>Завантаження...</p>}

      {!loading && results.length === 0 && (
        <div style={{ marginTop: '20px', color: '#d33' }}>
          Сорян 😕 Квізів за вашим запитом не знайдено
        </div>
      )}

      {!loading && results.length > 0 && (
        <ul>
          {results.map(q => (
            <li key={q.id}>
              <Link to={`/quiz/${q.id}`}>{q.title}</Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default SearchResults;