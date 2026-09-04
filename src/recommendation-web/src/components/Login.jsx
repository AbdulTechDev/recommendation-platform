import React, { useState } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function Login(){
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const nav = useNavigate()

  const submit = async () => {
    try {
      const res = await client.post('/api/auth/token', { username, password })
      localStorage.setItem('token', res.data.token)
      nav('/')
    } catch (e) {
      setError('Login failed')
    }
  }

  return (
    <Box sx={{ maxWidth: 420 }}>
      <Typography variant="h5">Login</Typography>
      <TextField name="username" label="Username" fullWidth value={username} onChange={e => setUsername(e.target.value)} sx={{ mt:2 }} inputProps={{ 'data-cy': 'login-username' }} />
      <TextField name="password" label="Password" fullWidth type="password" value={password} onChange={e => setPassword(e.target.value)} sx={{ mt:2 }} inputProps={{ 'data-cy': 'login-password' }} />
      {error && <Typography color="error">{error}</Typography>}
      <Button variant="contained" sx={{ mt:2 }} onClick={submit} data-cy="login-submit">Login</Button>
    </Box>
  )
}
