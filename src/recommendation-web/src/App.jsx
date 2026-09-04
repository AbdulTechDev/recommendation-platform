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
import EditProduct from './components/EditProduct'
import CategoryManagement from './components/CategoryManagement'
import ProtectedAdminRoute from './components/ProtectedAdminRoute'
import ProtectedRoute from './components/ProtectedRoute'
import ProductDetails from './components/ProductDetails'
import Cart from './components/Cart'
import Checkout from './components/Checkout'

export default function App() {
  return (
    <div>
      <NavBar />
      <Container sx={{ mt: 4 }}>
        <Routes>
          <Route path="/" element={<Products />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/recommendations" element={<ProtectedRoute><Recommendations /></ProtectedRoute>} />
          <Route path="/orders" element={<ProtectedRoute><Orders /></ProtectedRoute>} />
          <Route path="/product/:id" element={<ProductDetails />} />
          <Route path="/cart" element={<ProtectedRoute><Cart /></ProtectedRoute>} />
          <Route path="/checkout" element={<ProtectedRoute><Checkout /></ProtectedRoute>} />
          <Route path="/create-product" element={<ProtectedAdminRoute><CreateProduct /></ProtectedAdminRoute>} />
          <Route path="/edit-product/:id" element={<ProtectedAdminRoute><EditProduct /></ProtectedAdminRoute>} />
          <Route path="/admin/categories" element={<ProtectedAdminRoute><CategoryManagement /></ProtectedAdminRoute>} />
        </Routes>
      </Container>
    </div>
  )
}
