import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { ApiProvider } from './context/ApiContext.jsx'
import { BlockchainProvider } from './context/BlockchainContext.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ApiProvider>
      <BlockchainProvider>
        <App />
      </BlockchainProvider>
    </ApiProvider>
  </StrictMode>,
)
