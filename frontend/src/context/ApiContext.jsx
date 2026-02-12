import { createContext, useContext } from 'react'
import axios from 'axios'

const ApiContext = createContext()

export const API_BASE_URL = 'http://localhost:5026'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  }
})

export const ApiProvider = ({ children }) => {
  const value = {
    baseURL: API_BASE_URL,
    axiosInstance: apiClient
  }

  return (
    <ApiContext.Provider value={value}>
      {children}
    </ApiContext.Provider>
  )
}

export const useApi = () => {
  const context = useContext(ApiContext)
  if (!context) {
    throw new Error('useApi must be used within an ApiProvider')
  }
  return context
}
