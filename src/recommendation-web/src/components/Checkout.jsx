import React from 'react'
import client from '../api'
import { Box, Typography, Button } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function Checkout(){
  const nav = useNavigate()
  const pay = async ()=>{
    try{
      // quick simulate processing payment for orderId=1
      await client.post('/api/payments/process', { orderId: 1, amount: 0 })
      alert('Payment processed')
      nav('/')
    }catch(e){
      alert('Payment failed')
    }
  }

  return (
    <Box>
      <Typography variant="h5">Checkout</Typography>
      <Button variant="contained" onClick={pay}>Process Payment (dev)</Button>
    </Box>
  )
}
