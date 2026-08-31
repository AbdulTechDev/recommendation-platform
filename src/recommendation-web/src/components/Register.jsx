import React, { useState } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function Register(){
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const nav = useNavigate()

  const submit = async () => {
    try {
      await client.post('/api/users', { username, email, password })
      nav('/login')
    } catch (e) {
      setError('Registration failed')
    }
  }

  return (
    <Box sx={{ maxWidth: 420 }}>
      <Typography variant="h5">Register</Typography>
      <TextField label="Username" fullWidth value={username} onChange={e => setUsername(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Email" fullWidth value={email} onChange={e => setEmail(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Password" fullWidth type="password" value={password} onChange={e => setPassword(e.target.value)} sx={{ mt:2 }} />
      {error && <Typography color="error">{error}</Typography>}
      <Button variant="contained" sx={{ mt:2 }} onClick={submit}>Register</Button>
    </Box>
  )
}
