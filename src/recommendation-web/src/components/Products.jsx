import React, { useEffect, useState } from 'react'
import client from '../api'
import { Card, CardContent, Typography, Grid, Button } from '@mui/material'
import ConfirmDialog from './ConfirmDialog'
import { Snackbar, Alert } from '@mui/material'

function getRoleFromToken(){
  try{
    const token = localStorage.getItem('token')
    if(!token) return null
    const parts = token.split('.')
    if(parts.length < 2) return null
    const payload = JSON.parse(decodeURIComponent(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')).split('').map(function(c){return '%'+('00'+c.charCodeAt(0).toString(16)).slice(-2)}).join('')))
    return payload.role || payload.roles || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null
  }catch{ return null }
}

export default function Products() {
  const [products, setProducts] = useState([])
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [selectedId, setSelectedId] = useState(null)
  const [snack, setSnack] = useState({ open: false, message: '', severity: 'success' })

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
              <Button size="small" onClick={() => window.location = `/product/${p.id}`} data-cy={`product-view-${p.id}`}>View</Button>
              {getRoleFromToken() === 'Admin' && (
                <>
                  <Button size="small" onClick={() => window.location = `/edit-product/${p.id}`}>Edit</Button>
                  <Button size="small" onClick={()=>{ setSelectedId(p.id); setConfirmOpen(true) }}>Delete</Button>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>
      ))}
      <ConfirmDialog
        open={confirmOpen}
        title="Delete Product"
        message="Are you sure you want to delete this product?"
        onClose={(v)=>setConfirmOpen(v)}
        onConfirm={async ()=>{
          try{
            await client.delete(`/api/products/${selectedId}`)
            setProducts(products.filter(x=>x.id !== selectedId))
            setSnack({ open:true, message: 'Product deleted', severity: 'success' })
          }catch(e){
            setSnack({ open:true, message: 'Delete failed', severity: 'error' })
          }
        }}
      />
      <Snackbar open={snack.open} autoHideDuration={3000} onClose={()=>setSnack({...snack, open:false})}>
        <Alert severity={snack.severity} onClose={()=>setSnack({...snack, open:false})}>{snack.message}</Alert>
      </Snackbar>
    </Grid>
  )
}
