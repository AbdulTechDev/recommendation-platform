describe('API user flow', () => {
  it('registers, logs in, adds to cart and checks out via API', () => {
    const api = 'http://api:5000'
    const username = `e2eapi_${Date.now()}`
    const password = 'e2e-pass'

    // get a product to add
    cy.request(`${api}/api/products`).then(pr => {
      expect(pr.status).to.eq(200)
      const product = pr.body[0]
      expect(product).to.exist

      // register user
      cy.request('POST', `${api}/api/users`, { username, email: `${username}@example.com`, password }).then(res => {
        expect(res.status).to.be.oneOf([201,200])
        const user = res.body
        expect(user).to.have.property('id')

        // login
        cy.request('POST', `${api}/api/auth/token`, { username, password }).then(tok => {
          expect(tok.status).to.eq(200)
          const token = tok.body.token
          const auth = { Authorization: `Bearer ${token}` }

          // add item to cart
          const item = { productId: product.id, quantity: 1, unitPrice: product.price }
          cy.request({ method: 'POST', url: `${api}/api/cart/user/${user.id}/items`, headers: auth, body: item }).then(add => {
            expect(add.status).to.eq(201)

            // checkout
            cy.request({ method: 'POST', url: `${api}/api/cart/user/${user.id}/checkout`, headers: auth }).then(co => {
              expect(co.status).to.eq(200)
              expect(co.body).to.have.property('orderId')
            })
          })
        })
      })
    })
  })
})
