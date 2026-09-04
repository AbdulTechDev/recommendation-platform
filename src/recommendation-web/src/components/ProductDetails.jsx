import React, { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import client from '../api'
import { Typography, Button, Box, TextField } from '@mui/material'

export default function ProductDetails(){
  const { id } = useParams()
  const [product, setProduct] = useState(null)
  const [qty, setQty] = useState(1)
  const nav = useNavigate()

  useEffect(()=>{
    client.get(`/api/products/${id}`).then(r=>setProduct(r.data)).catch(()=>setProduct(null))
  },[id])

  const addToCart = async () => {
    try{
      // using userId=1 for dev; replace with real auth-derived id
      await client.post(`/api/cart/user/1/items`, { productId: parseInt(id,10), quantity: parseInt(qty,10), unitPrice: product.price })
      nav('/cart')
    }catch(e){
      alert('Add to cart failed')
    }
  }

  if(!product) return <Typography>Loading...</Typography>

  return (
    <Box>
      <Typography variant="h5">{product.name}</Typography>
      <Typography color="text.secondary">{product.category}</Typography>
      <Typography sx={{ mt:1 }}>{product.description}</Typography>
      <Typography sx={{ mt:1 }}>${product.price}</Typography>
      <TextField label="Quantity" type="number" value={qty} onChange={e=>setQty(e.target.value)} sx={{ mt:2 }} />
      <Button variant="contained" sx={{ mt:2 }} onClick={addToCart}>Add to cart</Button>
    </Box>
  )
}
