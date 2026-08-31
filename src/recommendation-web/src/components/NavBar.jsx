import React from 'react'
import { AppBar, Toolbar, Typography, Button } from '@mui/material'
import { Link, useNavigate } from 'react-router-dom'

function getRoleFromToken(){
  try{
    const token = localStorage.getItem('token')
    if(!token) return null
    const parts = token.split('.')
    if(parts.length < 2) return null
    const payload = JSON.parse(decodeURIComponent(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')).split('').map(function(c){return '%'+('00'+c.charCodeAt(0).toString(16)).slice(-2)}).join('')))
    return payload.role || payload.roles || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null
  }catch{
    return null
  }
}

export default function NavBar() {
  const navigate = useNavigate()
  const token = localStorage.getItem('token')
  const role = getRoleFromToken()

  const logout = () => {
    localStorage.removeItem('token')
    navigate('/login')
  }

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component={Link} to="/" sx={{ color: 'inherit', textDecoration: 'none', flexGrow: 1 }}>
          Recommendation Platform
        </Typography>
        <Button color="inherit" component={Link} to="/recommendations">Recommendations</Button>
        <Button color="inherit" component={Link} to="/orders">Orders</Button>
        {role === 'Admin' && <Button color="inherit" component={Link} to="/create-product">Create Product</Button>}
        {!token ? (
          <Button color="inherit" component={Link} to="/login">Login</Button>
        ) : (
          <Button color="inherit" onClick={logout}>Logout</Button>
        )}
      </Toolbar>
    </AppBar>
  )
}
