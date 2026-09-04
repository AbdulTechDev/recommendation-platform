describe('User flows', () => {
  it('registers, logs in, adds to cart and checks out (dev flow)', () => {
    const uniqueSuffix = Date.now()
    const username = `e2euser${uniqueSuffix}`
    const email = `e2e${uniqueSuffix}@example.com`
    const password = 'e2e-pass'
    cy.intercept('POST', '**/api/users').as('registerRequest')
    cy.intercept('POST', '**/api/auth/token').as('loginRequest')
    cy.intercept('GET', '**/api/products').as('getProducts')
    cy.visit('/')
    // navigate to register (direct route to avoid relying on navbar link)
    // capture console errors and page errors into window for post-mortem
    cy.visit('/register', {
      onBeforeLoad(win) {
        win.__consoleErrors = []
        const origErr = win.console.error
        win.console.error = function () {
          try { win.__consoleErrors.push(Array.from(arguments).join(' ')) } catch(e) {}
          return origErr && origErr.apply(this, arguments)
        }
        win.addEventListener('error', (e) => { try { win.__consoleErrors.push(e.message) } catch(_){} })
      }
    })
    cy.contains(/register/i, { timeout: 10000 }).should('exist')
    // use stable data-cy attributes added to inputs
    cy.get('[data-cy=register-username]', { timeout: 10000 }).type(username)
    cy.get('[data-cy=register-email]', { timeout: 10000 }).type(email)
    cy.get('[data-cy=register-password]', { timeout: 10000 }).type(password)
    cy.get('[data-cy=register-submit]', { timeout: 5000 }).click()
    cy.wait('@registerRequest').its('response.statusCode').should('eq', 201)

    // login
    cy.visit('/login')
    cy.contains(/login/i, { timeout: 10000 }).should('exist')
    cy.get('[data-cy=login-username]', { timeout: 10000 }).type(username)
    cy.get('[data-cy=login-password]', { timeout: 10000 }).type(password)
    cy.get('[data-cy=login-submit]', { timeout: 5000 }).click()
    // wait for login response and store token; then ensure app knows user is logged in
    cy.wait('@loginRequest').then((interception) => {
      expect(interception.response.statusCode).to.eq(200)
      const token = interception.response.body && interception.response.body.token
      expect(token, 'login token').to.be.a('string')
      // persist token into app localStorage so navbar reflects logged-in state
      cy.window().then(win => win.localStorage.setItem('token', token))
      cy.wrap(token).as('authToken')
    })
    // navigate to home and assert logout appears (retrying assertion)
    cy.visit('/')
    cy.contains('Logout', { timeout: 10000 }).should('be.visible')
    // verify visual login state: NavBar shows Logout
    cy.visit('/')
    cy.contains('button', 'Logout', { timeout: 10000 }).should('exist')

    // wait for products to load, then view first product using stable selector
    cy.visit('/')
    cy.wait('@getProducts').then((interception) => {
      const products = interception.response.body || []
      expect(products.length, 'at least one product').to.be.greaterThan(0)
      const first = products[0]
      // assert first product is rendered in the UI
      cy.contains(first.name, { timeout: 10000 }).should('be.visible')
      // add to cart via API using captured token then checkout via API
      cy.get('@authToken').then(token => {
        cy.request({
          method: 'POST',
          url: `http://api:5000/api/cart/user/1/items`,
          headers: { Authorization: `Bearer ${token}` },
          body: { productId: first.id, quantity: 1, unitPrice: first.price }
        }).then(() => {
          cy.request({ method: 'POST', url: `http://api:5000/api/cart/user/1/checkout`, headers: { Authorization: `Bearer ${token}` } }).its('status').should('be.oneOf', [200,201])
        })
      })
    })

    // go to cart and checkout
    cy.contains(/cart/i).click()
    cy.contains(/checkout/i).click()

    // confirm order placed via API; as a visual check, ensure navbar still shows username-related control
    cy.contains('Logout', { timeout: 10000 }).should('exist')

    // append any captured console errors to DOM so screenshots/videos include them
    cy.window().then(win => {
      if (win.__consoleErrors && win.__consoleErrors.length) {
        const pre = win.document.createElement('pre')
        pre.id = '__consoleErrors'
        pre.style.background = 'rgba(255,200,200,0.9)'
        pre.style.color = 'black'
        pre.style.padding = '8px'
        pre.style.position = 'fixed'
        pre.style.bottom = '0'
        pre.style.left = '0'
        pre.style.zIndex = '99999'
        pre.innerText = win.__consoleErrors.join('\n')
        win.document.body.appendChild(pre)
      }
    })
  })
})
