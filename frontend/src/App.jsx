import { useState, useEffect } from 'react'
import { useApi } from './context/ApiContext'
import './App.css'

function App() {
  const { api } = useApi()
  const [status, setStatus] = useState('Initializing...')
  const [response, setResponse] = useState(null)
  const [loading, setLoading] = useState(false)

  // Check a test endpoint
  const testConnection = async () => {
    setLoading(true)
    setStatus('Sending request...')
    try {
      const res = await api.get('/weatherforecast')
      setStatus('✓ Request Successful!')
      setResponse(res.data)
    } catch (error) {
      setStatus(`✗ Error: ${error.message}`)
      setResponse(null)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    testConnection()
  }, [])

  return (
    <div className="app">
      <h1>QuizRush Frontend - Backend Connection Test</h1>
      
      <div className="status-box">
        <h2>Status: {status}</h2>
        
        <button onClick={testConnection} disabled={loading}>
          {loading ? 'Testing...' : 'Test Connection'}
        </button>
      </div>

      {response && (
        <div className="response-box">
          <h3>Response from Backend get</h3>
        </div>
      )}
    </div>
  )
}

export default App
