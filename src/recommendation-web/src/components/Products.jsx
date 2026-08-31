import React, { useEffect, useState } from 'react'
import client from '../api'
import { Card, CardContent, Typography, Grid } from '@mui/material'

export default function Products() {
  const [products, setProducts] = useState([])

  useEffect(() => {
    client.get('/api/products').then(r => setProducts(r.data)).catch(() => setProducts([]))
  }, [])

  return (
    <Grid container spacing={2}>
      {products.map(p => (
        <Grid item xs={12} sm={6} md={4} key={p.id}>
          <Card>
            <CardContent>
              <Typography variant="h6">{p.name}</Typography>
              <Typography color="text.secondary">{p.category}</Typography>
              <Typography>{p.description}</Typography>
              <Typography sx={{ mt: 1 }}>${p.price}</Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  )
}
