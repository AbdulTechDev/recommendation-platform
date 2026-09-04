import React, { useState, useEffect } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography } from '@mui/material'
import { useNavigate, useParams } from 'react-router-dom'
import { Snackbar, Alert } from '@mui/material'

export default function EditProduct(){
  const { id } = useParams()
  const [name, setName] = useState('')
  const [category, setCategory] = useState('')
  const [categories, setCategories] = useState([])
  const [description, setDescription] = useState('')
  const [price, setPrice] = useState('0')
  const [error, setError] = useState(null)
  const [snackOpen, setSnackOpen] = useState(false)
  const nav = useNavigate()

  useEffect(()=>{
    client.get(`/api/products/${id}`).then(r=>{
      setName(r.data.name)
      setCategory(r.data.category)
      setDescription(r.data.description)
      setPrice(r.data.price)
    }).catch(()=>setError('Failed to load product'))
    client.get('/api/categories').then(r=>setCategories(r.data)).catch(()=>setCategories([]))
  },[id])

  const submit = async () => {
    try{
      const payload = { id: parseInt(id,10), name, category, description, price: parseFloat(price) }
      await client.put(`/api/products/${id}`, payload)
      setSnackOpen(true)
      setTimeout(()=>nav('/'),600)
    }catch(e){
      setError('Update failed (are you an admin?)')
    }
  }

  return (
    <Box sx={{ maxWidth: 720 }}>
      <Typography variant="h5">Edit Product</Typography>
      <TextField label="Name" fullWidth value={name} onChange={e=>setName(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Category" fullWidth value={category} onChange={e=>setCategory(e.target.value)} sx={{ mt:2 }} helperText={categories.length ? 'Select or type a category' : ''} />
      <TextField label="Description" fullWidth value={description} onChange={e=>setDescription(e.target.value)} sx={{ mt:2 }} />
      <TextField label="Price" fullWidth value={price} onChange={e=>setPrice(e.target.value)} sx={{ mt:2 }} />
      {error && <Typography color="error">{error}</Typography>}
      <Button variant="contained" sx={{ mt:2 }} onClick={submit}>Save</Button>
      <Snackbar open={snackOpen} autoHideDuration={2000} onClose={()=>setSnackOpen(false)}>
        <Alert severity="success" onClose={()=>setSnackOpen(false)}>Product updated</Alert>
      </Snackbar>
    </Box>
  )
}
