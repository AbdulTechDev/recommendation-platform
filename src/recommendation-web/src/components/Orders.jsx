import React, { useState } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography } from '@mui/material'

export default function Orders(){
  const [userId, setUserId] = useState('1')
  const [total, setTotal] = useState('0')

  const submit = async () => {
    try {
      await client.post('/api/orders', { userId: parseInt(userId,10), total: parseFloat(total) })
      alert('Order created')
    } catch (e) {
      alert('Order failed')
    }
  }

  return (
    <Box sx={{ maxWidth: 420 }}>
      <Typography variant="h5">Create Order</Typography>
      <TextField label="UserId" fullWidth value={userId} onChange={e => setUserId(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Total" fullWidth value={total} onChange={e => setTotal(e.target.value)} sx={{ mt:2 }} />
      <Button variant="contained" sx={{ mt:2 }} onClick={submit}>Create</Button>
    </Box>
  )
}
