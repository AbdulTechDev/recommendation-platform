import React, { useEffect, useState } from 'react'
import client from '../api'
import { Box, Typography, Button, List, ListItem, TextField } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function Cart(){
  const [cart, setCart] = useState(null)
  const nav = useNavigate()

  const load = async ()=>{
    try{
      const res = await client.get('/api/cart/user/1')
      setCart(res.data)
    }catch(e){
      setCart({ items: [] })
    }
  }

  useEffect(()=>{ load() }, [])

  const remove = async (id)=>{
    await client.delete(`/api/cart/items/${id}`)
    load()
  }

  const checkout = async ()=>{
    try{
      const res = await client.post('/api/cart/user/1/checkout')
      alert('Order placed')
      nav('/')
    }catch(e){
      alert('Checkout failed')
    }
  }

  if(!cart) return <Typography>Loading cart...</Typography>

  return (
    <Box>
      <Typography variant="h5">Cart</Typography>
      <List>
        {cart.items.map(it=> (
          <ListItem key={it.id}>
            <Typography sx={{ flex: 1 }}>{it.productId} x {it.quantity} @ ${it.unitPrice}</Typography>
            <Button onClick={()=>remove(it.id)}>Remove</Button>
          </ListItem>
        ))}
      </List>
      <Button variant="contained" onClick={checkout}>Checkout</Button>
    </Box>
  )
}
