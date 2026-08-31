import React, { useState } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function CreateProduct(){
  const [name, setName] = useState('')
  const [category, setCategory] = useState('')
  const [description, setDescription] = useState('')
  const [price, setPrice] = useState('0')
  const [error, setError] = useState(null)
  const nav = useNavigate()

  const submit = async () => {
    try{
      const payload = { name, category, description, price: parseFloat(price) }
      const res = await client.post('/api/products', payload)
      if(res.status === 201) nav('/')
    }catch(e){
      setError('Create product failed (are you an admin?)')
    }
  }

  return (
    <Box sx={{ maxWidth: 720 }}>
      <Typography variant="h5">Create Product</Typography>
      <TextField label="Name" fullWidth value={name} onChange={e=>setName(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Category" fullWidth value={category} onChange={e=>setCategory(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Description" fullWidth value={description} onChange={e=>setDescription(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Price" fullWidth value={price} onChange={e=>setPrice(e.target.value)} sx={{ mt:2 }} />
      {error && <Typography color="error">{error}</Typography>}
      <Button variant="contained" sx={{ mt:2 }} onClick={submit}>Create</Button>
    </Box>
  )
}
