import React, { useState } from 'react'
import client from '../api'
import { TextField, Button, Box, Typography, List, ListItem } from '@mui/material'

export default function Recommendations(){
  const [query, setQuery] = useState('')
  const [items, setItems] = useState([])

  const submit = async () => {
    try {
      const res = await client.post('/api/recommendations', { query, topN: 5 })
      setItems(res.data.recommendations || [])
    } catch (e) {
      setItems([])
    }
  }

  return (
    <Box sx={{ maxWidth: 720 }}>
      <Typography variant="h5">Recommendations</Typography>
      <TextField label="Query" fullWidth value={query} onChange={e => setQuery(e.target.value)} sx={{ mt:2 }} />
      <Button variant="contained" sx={{ mt:2 }} onClick={submit}>Get</Button>
      <List>
        {items.map((it, i) => <ListItem key={i}>{it}</ListItem>)}
      </List>
    </Box>
  )
}
