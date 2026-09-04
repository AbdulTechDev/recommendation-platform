import React, { useEffect, useState } from 'react'
import client from '../api'
import { Box, Typography, TextField, Button, List, ListItem } from '@mui/material'

export default function CategoryManagement(){
  const [categories, setCategories] = useState([])
  const [name, setName] = useState('')

  const load = async ()=>{
    try{
      const res = await client.get('/api/categories')
      setCategories(res.data)
    }catch{ setCategories([]) }
  }

  useEffect(()=>{ load() }, [])

  const create = async ()=>{
    try{
      await client.post('/api/categories', { name })
      setName('')
      load()
    }catch(e){ alert('Create failed') }
  }

  const remove = async (id)=>{
    if(!confirm('Delete category?')) return
    try{ await client.delete(`/api/categories/${id}`); load() }catch{ alert('Delete failed') }
  }

  return (
    <Box sx={{ maxWidth: 720 }}>
      <Typography variant="h5">Category Management</Typography>
      <TextField label="Name" value={name} onChange={e=>setName(e.target.value)} sx={{ mt:2 }} />
      <Button sx={{ mt:2 }} onClick={create}>Create</Button>

      <List>
        {categories.map(c => (
          <ListItem key={c.id} secondaryAction={<Button onClick={()=>remove(c.id)}>Delete</Button>}>
            {c.name}
          </ListItem>
        ))}
      </List>
    </Box>
  )
}
