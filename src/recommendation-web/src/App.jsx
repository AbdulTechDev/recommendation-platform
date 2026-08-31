import React from 'react'
import { Routes, Route } from 'react-router-dom'
import { Container } from '@mui/material'
import NavBar from './components/NavBar'
import Products from './components/Products'
import Login from './components/Login'
import Register from './components/Register'
import Recommendations from './components/Recommendations'
import Orders from './components/Orders'
import CreateProduct from './components/CreateProduct'
import ProtectedAdminRoute from './components/ProtectedAdminRoute'

export default function App() {
  return (
    <div>
      <NavBar />
      <Container sx={{ mt: 4 }}>
        <Routes>
          <Route path="/" element={<Products />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/recommendations" element={<Recommendations />} />
          <Route path="/orders" element={<Orders />} />
          <Route path="/create-product" element={<ProtectedAdminRoute><CreateProduct /></ProtectedAdminRoute>} />
        </Routes>
      </Container>
    </div>
  )
}
