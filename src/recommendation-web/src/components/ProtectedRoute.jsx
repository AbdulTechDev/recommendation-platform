import React from 'react'
import { Navigate } from 'react-router-dom'

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

export default function ProtectedRoute({ children, requiredRole }){
  const token = localStorage.getItem('token')
  if(!token) return <Navigate to="/login" replace />
  if(requiredRole){
    const role = getRoleFromToken()
    if(role !== requiredRole) return <Navigate to="/" replace />
  }
  return children
}
