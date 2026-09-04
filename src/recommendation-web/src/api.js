import axios from 'axios'

// Default to localhost for local testing; can be overridden via VITE_API_BASE
const API_BASE = import.meta.env.VITE_API_BASE || process.env.REACT_APP_API_BASE || 'http://localhost:5000'

const client = axios.create({ baseURL: API_BASE })

client.interceptors.request.use(cfg => {
  const token = localStorage.getItem('token')
  if (token) cfg.headers.Authorization = `Bearer ${token}`
  return cfg
})

client.interceptors.response.use(
  r => r,
  err => {
    if (err.response && err.response.status === 401) {
      localStorage.removeItem('token')
      // redirect to login for interactive clients
      if (typeof window !== 'undefined') window.location = '/login'
    }
    return Promise.reject(err)
  }
)

export default client
