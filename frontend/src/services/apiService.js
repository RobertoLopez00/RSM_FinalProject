import { api } from 'boot/axios'

/**
 * API Service
 * Central service for all HTTP requests to the backend API
 * Handles Order CRUD, Lookups, and Address Validation
 * All methods return response data directly
 */
export const apiService = {
  // ============================================
  // ORDERS CRUD OPERATIONS
  // ============================================

  /**
   * Fetch all orders from the database
   * @returns {Promise} List of all orders with customer and employee details
   */
  async getOrders() {
    const response = await api.get('/orders')
    return response.data
  },

  /**
   * Fetch a single order by ID
   * @param {number} id - Order ID to retrieve
   * @returns {Promise} Order details including line items
   */
  async getOrderById(id) {
    const response = await api.get(`/orders/${id}`)
    return response.data
  },

  /**
   * Create a new order with line items
   * @param {Object} orderData - Order object containing customer, dates, and order details
   * @returns {Promise} Created order with generated OrderID
   */
  async createOrder(orderData) {
    const response = await api.post('/orders', orderData)
    return response.data
  },

  /**
   * Update an existing order
   * @param {number} id - Order ID to update
   * @param {Object} orderData - Updated order data
   * @returns {Promise} Updated order details
   */
  async updateOrder(id, orderData) {
    const response = await api.put(`/orders/${id}`, orderData)
    return response.data
  },

  /**
   * Delete an order
   * @param {number} id - Order ID to delete
   * @returns {Promise} No content response (204)
   */
  async deleteOrder(id) {
    await api.delete(`/orders/${id}`)
  },

  /**
   * Fetch all orders for a specific customer
   * @param {string} customerId - Customer ID to filter by
   * @returns {Promise} List of orders belonging to the customer
   */
  async getOrdersByCustomer(customerId) {
    const response = await api.get(`/orders/customer/${customerId}`)
    return response.data
  },

  /**
   * Fetch orders within a date range
   * @param {string} startDate - Start date for filtering (ISO format)
   * @param {string} endDate - End date for filtering (ISO format)
   * @returns {Promise} List of orders within the specified date range
   */
  async getOrdersByDateRange(startDate, endDate) {
    const response = await api.get('/orders/date-range', {
      params: { startDate, endDate }
    })
    return response.data
  },

  // ============================================
  // LOOKUP DATA OPERATIONS
  // ============================================

  /**
   * Fetch all customers for dropdown/select lists
   * @returns {Promise} List of all customers with ID and name
   */
  async getCustomers() {
    const response = await api.get('/lookups/customers')
    return response.data
  },

  /**
   * Fetch all employees for dropdown/select lists
   * @returns {Promise} List of all employees with ID and name
   */
  async getEmployees() {
    const response = await api.get('/lookups/employees')
    return response.data
  },

  /**
   * Fetch all shippers for dropdown/select lists
   * @returns {Promise} List of all shippers with ID and name
   */
  async getShippers() {
    const response = await api.get('/lookups/shippers')
    return response.data
  },

  /**
   * Fetch all products for order details
   * @returns {Promise} List of all products with price and availability
   */
  async getProducts() {
    const response = await api.get('/lookups/products')
    return response.data
  },

  // ============================================
  // ADDRESS VALIDATION
  // ============================================

  /**
   * Validate and geocode an address using Google Maps Address Validation API
   * @param {Object} addressData - Address object with street, city, region, postal code, country
   * @returns {Promise} Validated address with formatted address and coordinates (latitude, longitude)
   */
  async validateAddress(addressData) {
    const response = await api.post('/addresses/validate', addressData)
    return response.data
  }
}
