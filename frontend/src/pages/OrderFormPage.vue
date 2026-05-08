<template>
  <q-page class="q-pa-md">
    <div class="row items-center q-mb-md">
      <h2 class="q-my-none flex-grow">{{ isEdit ? 'Edit Order' : 'New Order' }}</h2>
      <q-btn
        flat
        icon="close"
        to="/orders"
      />
    </div>

    <q-form
      @submit="onSubmit"
      class="q-gutter-md"
      ref="formRef"
    >
      <div class="row q-col-gutter-md">
        <!-- Customer Selection -->
        <div class="col-12 col-sm-6">
          <q-select
            v-model="form.customerID"
            :options="customers"
            option-value="customerID"
            option-label="companyName"
            label="Customer *"
            outlined
            dense
            :rules="[val => !!val || 'Customer is required']"
            emit-value
            map-options
          />
        </div>

        <!-- Order Date -->
        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.orderDate"
            type="date"
            label="Order Date *"
            outlined
            dense
            :rules="[val => !!val || 'Order date is required']"
          />
        </div>

        <!-- Required Date -->
        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.requiredDate"
            type="date"
            label="Required Date"
            outlined
            dense
          />
        </div>

        <!-- Shipped Date -->
        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.shippedDate"
            type="date"
            label="Shipped Date"
            outlined
            dense
          />
        </div>

        <!-- Order Status (computed) -->
        <div class="col-12 col-sm-6" v-if="isEdit">
          <q-input
            v-model="form.status"
            label="Order Status"
            outlined
            dense
            readonly
            disable
          />
        </div>

        <!-- Employee Selection -->
        <div class="col-12 col-sm-6">
          <q-select
            v-model="form.employeeID"
            :options="employees"
            option-value="employeeID"
            option-label="employeeName"
            label="Employee"
            outlined
            dense
            clearable
            emit-value
            map-options
          />
        </div>

        <!-- Shipper Selection -->
        <div class="col-12 col-sm-6">
          <q-select
            v-model="form.shipVia"
            :options="shippers"
            option-value="shipperID"
            option-label="companyName"
            label="Shipper"
            outlined
            dense
            clearable
            emit-value
            map-options
          />
        </div>

        <!-- Freight -->
        <div class="col-12 col-sm-6">
          <q-input
            v-model.number="form.freight"
            type="number"
            label="Freight"
            outlined
            dense
            step="0.01"
            min="0"
            :rules="[nonNegativeNumberRule]"
          />
        </div>

        <!-- Shipping Address -->
        <div class="col-12">
          <q-input
            v-model="form.shipName"
            label="Ship Name"
            outlined
            dense
            maxlength="40"
            counter
            :rules="[maxLengthRule(40, 'Ship Name')]"
          />
        </div>

        <div class="col-12">
          <q-input
            v-model="form.shipAddress"
            label="Ship Address"
            outlined
            dense
            maxlength="60"
            counter
            :rules="[maxLengthRule(60, 'Ship Address')]"
          />
        </div>

        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.shipCity"
            label="Ship City"
            outlined
            dense
            maxlength="15"
            counter
            hint="Max 15 characters"
            :rules="[maxLengthRule(15, 'Ship City')]"
          />
        </div>

        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.shipPostalCode"
            label="Ship Postal Code"
            outlined
            dense
            maxlength="10"
            counter
            :rules="[maxLengthRule(10, 'Ship Postal Code')]"
          />
        </div>

        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.shipCountry"
            label="Ship Country"
            outlined
            dense
            maxlength="15"
            counter
            :rules="[maxLengthRule(15, 'Ship Country')]"
          />
        </div>

        <div class="col-12 col-sm-6">
          <q-input
            v-model="form.shipRegion"
            label="Ship Region"
            outlined
            dense
            maxlength="15"
            counter
            :rules="[maxLengthRule(15, 'Ship Region')]"
          />
        </div>

        <!-- Address Validation + Map -->
        <div class="col-12 q-mt-md">
          <q-card flat bordered>
            <q-card-section class="row items-center q-gutter-sm">
              <div class="text-subtitle1">Address Validation and Map</div>
              <q-space />
              <q-btn
                v-if="validatedAddress"
                outline
                color="primary"
                label="Use Validated Address"
                icon="content_copy"
                @click="applyValidatedAddress"
              />
              <q-btn
                color="primary"
                label="Validate Address"
                icon="verified"
                :loading="validatingAddress"
                :disable="!canValidateAddress"
                @click="validateShippingAddress"
              />
            </q-card-section>

            <q-card-section class="q-pt-none">
              <q-badge
                v-if="validatedAddress"
                :color="validatedAddress.isFallback ? 'warning' : 'positive'"
                :text-color="validatedAddress.isFallback ? 'dark' : 'white'"
                class="q-mb-sm"
              >
                {{ validatedAddress.isFallback ? 'Fallback address in use' : 'Validated address ready' }}
              </q-badge>
              <div v-if="validatedAddress" class="q-mb-sm text-positive">
                Validated: {{ validatedAddress.formattedAddress }}
              </div>
              <div v-if="validatedAddress" class="q-mb-sm text-caption text-grey-8">
                Coordinates: {{ validatedAddress.latitude.toFixed(6) }}, {{ validatedAddress.longitude.toFixed(6) }}
              </div>
              <div v-if="validatedAddress" class="q-mb-md text-caption text-grey-7">
                {{ validatedAddress.isFallback ? 'Google did not return geocoded coordinates, so the map is showing the typed address.' : 'Google validated this address and returned coordinates.' }}
              </div>

              <div v-if="mapEmbedUrl" class="map-frame-wrapper">
                <iframe
                  :src="mapEmbedUrl"
                  width="100%"
                  height="320"
                  style="border: 0"
                  loading="lazy"
                  referrerpolicy="no-referrer-when-downgrade"
                  title="Shipping Address Map"
                />
              </div>
              <div v-else class="text-grey-7">
                Enter Ship Address and Ship City to preview the map.
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Line Items -->
      <div class="q-mt-lg">
        <div class="row items-center q-mb-md">
          <h4 class="q-my-none flex-grow">Order Details</h4>
          <q-btn
            color="primary"
            label="Add Item"
            icon="add"
            size="sm"
            @click="addLineItem"
            :disable="!form.customerID"
          />
        </div>

        <div v-if="form.orderDetails.length === 0" class="q-pa-md text-center text-grey">
          No items added. Click "Add Item" to add products to this order.
        </div>

        <q-table
          v-if="form.orderDetails.length > 0"
          :rows="form.orderDetails"
          :columns="lineItemColumns"
          row-key="tempId"
          flat
          bordered
          class="q-mt-md"
        >
          <template v-slot:body-cell-productName="props">
            <q-td :props="props">
              <q-select
                :model-value="props.row.productID"
                :options="products"
                option-value="productID"
                option-label="productName"
                outlined
                dense
                emit-value
                map-options
                @update:model-value="updateLineItemProduct(props.row.tempId, $event)"
              />
            </q-td>
          </template>

          <template v-slot:body-cell-quantity="props">
            <q-td :props="props">
              <q-input
                :model-value="props.row.quantity"
                type="number"
                outlined
                dense
                min="1"
                @update:model-value="updateLineItem(props.row.tempId, 'quantity', parseInt($event) || 0)"
              />
            </q-td>
          </template>

          <template v-slot:body-cell-unitPrice="props">
            <q-td :props="props">
              ${{ parseFloat(props.row.unitPrice || 0).toFixed(2) }}
            </q-td>
          </template>

          <template v-slot:body-cell-discount="props">
            <q-td :props="props">
              <q-input
                :model-value="props.row.discount"
                type="number"
                outlined
                dense
                step="0.01"
                min="0"
                max="1"
                @update:model-value="updateLineItem(props.row.tempId, 'discount', parseFloat($event) || 0)"
              />
            </q-td>
          </template>

          <template v-slot:body-cell-lineTotal="props">
            <q-td :props="props">
              ${{ calculateLineTotal(props.row).toFixed(2) }}
            </q-td>
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn
                flat
                dense
                round
                icon="delete"
                size="sm"
                color="negative"
                @click="removeLineItem(props.row.tempId)"
              />
            </q-td>
          </template>
        </q-table>
      </div>

      <!-- Order Total -->
      <div v-if="form.orderDetails.length > 0" class="row justify-end q-mt-md">
        <div class="text-h6">
          Order Total: <span class="text-primary">${{ orderTotal.toFixed(2) }}</span>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="row q-gutter-md q-mt-lg">
        <q-btn
          label="Cancel"
          outline
          to="/orders"
        />
        <q-btn
          label="Save"
          type="submit"
          color="primary"
          :loading="saving"
          :disable="!form.customerID || form.orderDetails.length === 0"
        />
      </div>
    </q-form>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { apiService } from 'src/services/apiService'
const route = useRoute()
const router = useRouter()
const $q = useQuasar()

const isEdit = computed(() => !!route.params.id)
const saving = ref(false)

const customers = ref([])
const employees = ref([])
const shippers = ref([])
const products = ref([])
const formRef = ref(null)
const validatingAddress = ref(false)
const validatedAddress = ref(null)

const maxLengthRule = (max, label) => (val) => {
  if (!val) return true
  return val.length <= max || `${label} must be ${max} characters or less`
}

const nonNegativeNumberRule = (val) => {
  if (val === null || val === undefined || val === '') return true
  return Number(val) >= 0 || 'Value must be 0 or greater'
}

const form = ref({
  customerID: null,
  orderDate: new Date().toISOString().split('T')[0],
  requiredDate: null,
  shippedDate: null,
  status: null,
  employeeID: null,
  shipVia: null,
  freight: 0,
  shipName: '',
  shipAddress: '',
  shipCity: '',
  shipPostalCode: '',
  shipCountry: '',
  shipRegion: '',
  orderDetails: []
})

const lineItemColumns = [
  { name: 'productName', label: 'Product', field: 'productName', align: 'left' },
  { name: 'quantity', label: 'Quantity', field: 'quantity', align: 'center' },
  { name: 'unitPrice', label: 'Unit Price', field: 'unitPrice', align: 'right' },
  { name: 'discount', label: 'Discount', field: 'discount', align: 'center' },
  { name: 'lineTotal', label: 'Line Total', field: 'lineTotal', align: 'right' },
  { name: 'actions', label: '', field: 'actions', align: 'center' }
]

let lineItemCounter = 0

const orderTotal = computed(() => {
  return form.value.orderDetails.reduce((sum, item) => sum + calculateLineTotal(item), 0)
})

const canValidateAddress = computed(() => {
  return !!form.value.shipAddress?.trim() && !!form.value.shipCity?.trim()
})

const notify = (type, message, caption = '') => {
  if ($q && typeof $q.notify === 'function') {
    $q.notify({
      type,
      message,
      caption
    })
    return
  }

  const fallbackMessage = caption ? `${message}: ${caption}` : message
  if (type === 'positive') {
    console.log(fallbackMessage)
    return
  }

  window.alert(fallbackMessage)
}

const mapQuery = computed(() => {
  if (validatedAddress.value?.formattedAddress) {
    return validatedAddress.value.formattedAddress
  }

  const parts = [
    form.value.shipAddress,
    form.value.shipCity,
    form.value.shipRegion,
    form.value.shipPostalCode,
    form.value.shipCountry
  ]
    .filter(Boolean)
    .map(part => String(part).trim())
    .filter(part => part.length > 0)

  return parts.join(', ')
})

const mapEmbedUrl = computed(() => {
  if (!mapQuery.value) return ''
  return `https://www.google.com/maps?q=${encodeURIComponent(mapQuery.value)}&output=embed`
})

const calculateLineTotal = (item) => {
  return (item.quantity * item.unitPrice) - item.discount
}

const loadLookupData = async () => {
  try {
    console.log('Loading lookup data...')
    const [customersData, employeesData, shippersData, productsData] = await Promise.all([
      apiService.getCustomers(),
      apiService.getEmployees(),
      apiService.getShippers(),
      apiService.getProducts()
    ])

    console.log('Customers:', customersData)
    console.log('Employees:', employeesData)
    console.log('Shippers:', shippersData)
    console.log('Products:', productsData)

    customers.value = customersData
    employees.value = employeesData.map(e => ({
      ...e,
      employeeName: `${e.firstName} ${e.lastName}`
    }))
    shippers.value = shippersData
    products.value = productsData

    console.log('Lookup data loaded successfully')
  } catch (error) {
    console.error('Failed to load lookup data:', error)
    alert(`Failed to load lookup data: ${error.message}`)
  }
}

const loadOrder = async (orderId) => {
  try {
    const order = await apiService.getOrderById(orderId)
    form.value = {
      customerID: order.customerID,
      orderDate: order.orderDate ? order.orderDate.split('T')[0] : '',
      requiredDate: order.requiredDate ? order.requiredDate.split('T')[0] : null,
      shippedDate: order.shippedDate ? order.shippedDate.split('T')[0] : null,
      employeeID: order.employeeID || null,
      shipVia: order.shipVia || null,
      freight: order.freight || 0,
      shipName: order.shipName || '',
      shipAddress: order.shipAddress || '',
      shipCity: order.shipCity || '',
      shipPostalCode: order.shipPostalCode || '',
      shipCountry: order.shipCountry || '',
      status: order.status || null,
      shipRegion: order.shipRegion || '',
      orderDetails: (order.orderDetails || []).map((od) => ({
        tempId: `${od.orderID}-${od.productID}`,
        productID: od.productID,
        quantity: od.quantity,
        unitPrice: od.unitPrice,
        discount: od.discount
      }))
    }
  } catch (error) {
    console.error('Failed to load order:', error)
    alert(`Failed to load order: ${error.message}`)
  }
}

const addLineItem = () => {
  form.value.orderDetails.push({
    tempId: `temp-${lineItemCounter++}`,
    productID: null,
    quantity: 1,
    unitPrice: 0,
    discount: 0
  })
}

const removeLineItem = (tempId) => {
  const index = form.value.orderDetails.findIndex(item => item.tempId === tempId)
  if (index > -1) {
    form.value.orderDetails.splice(index, 1)
  }
}

const updateLineItem = (tempId, field, value) => {
  const item = form.value.orderDetails.find(i => i.tempId === tempId)
  if (item) {
    item[field] = value
  }
}

const updateLineItemProduct = (tempId, productID) => {
  const item = form.value.orderDetails.find(i => i.tempId === tempId)
  if (item) {
    item.productID = productID
    const product = products.value.find(p => p.productID === productID)
    if (product) {
      item.unitPrice = product.unitPrice || 0
    }
  }
}

const validateShippingAddress = async () => {
  if (!canValidateAddress.value) {
    notify('warning', 'Ship Address and Ship City are required to validate address')
    return
  }

  validatingAddress.value = true
  try {
    const response = await apiService.validateAddress({
      address: form.value.shipAddress,
      city: form.value.shipCity,
      region: form.value.shipRegion || null,
      postalCode: form.value.shipPostalCode || null,
      country: form.value.shipCountry || null
    })

    validatedAddress.value = response
    notify('positive', 'Address validated successfully')
  } catch (error) {
    console.error('Address validation failed:', error)
    validatedAddress.value = null
    notify('negative', 'Address validation failed', error.message)
  } finally {
    validatingAddress.value = false
  }
}

const applyValidatedAddress = () => {
  if (!validatedAddress.value?.formattedAddress) return

  const [addressLine = '', city = '', region = '', postalCode = '', country = ''] = validatedAddress.value.formattedAddress
    .split(',')
    .map(part => part.trim())

  form.value.shipAddress = addressLine || form.value.shipAddress
  form.value.shipCity = city || form.value.shipCity
  form.value.shipRegion = region || form.value.shipRegion
  form.value.shipPostalCode = postalCode || form.value.shipPostalCode
  form.value.shipCountry = country || form.value.shipCountry

  notify('positive', 'Validated address applied to form')
}

const onSubmit = async () => {
  // Validate form
  if (!formRef.value) return
  const isValid = await formRef.value.validate()
  if (!isValid) return

  // Validate line items
  if (form.value.orderDetails.length === 0) {
    notify('warning', 'Please add at least one item to the order')
    return
  }

  if (!form.value.customerID) {
    notify('warning', 'Please select a customer')
    return
  }

  const invalidLine = form.value.orderDetails.find((od) => {
    const quantity = parseInt(od.quantity)
    const unitPrice = parseFloat(od.unitPrice)
    const discount = parseFloat(od.discount)

    return !od.productID ||
      !Number.isInteger(quantity) || quantity <= 0 ||
      Number.isNaN(unitPrice) || unitPrice < 0 ||
      Number.isNaN(discount) || discount < 0 || discount > 1
  })

  if (invalidLine) {
    notify('warning', 'Each line item must have product, quantity > 0, unit price >= 0, and discount between 0 and 1')
    return
  }

  saving.value = true
  try {
    const orderData = {
      customerID: form.value.customerID,
      orderDate: form.value.orderDate,
      requiredDate: form.value.requiredDate || null,
      shippedDate: form.value.shippedDate || null,
      clearRequiredDate: !form.value.requiredDate,
      clearShippedDate: !form.value.shippedDate,
      employeeID: form.value.employeeID || null,
      shipVia: form.value.shipVia || null,
      freight: parseFloat(form.value.freight) || 0,
      shipName: form.value.shipName || '',
      shipAddress: form.value.shipAddress || '',
      shipCity: form.value.shipCity || '',
      shipPostalCode: form.value.shipPostalCode || '',
      shipCountry: form.value.shipCountry || '',
      shipRegion: form.value.shipRegion || '',
      orderDetails: form.value.orderDetails.map(od => ({
        productID: od.productID,
        quantity: parseInt(od.quantity) || 0,
        unitPrice: parseFloat(od.unitPrice) || 0,
        discount: parseFloat(od.discount) || 0
      }))
    }

    if (isEdit.value) {
      await apiService.updateOrder(route.params.id, orderData)
      notify('positive', 'Order updated successfully')
    } else {
      await apiService.createOrder(orderData)
      notify('positive', 'Order created successfully')
    }

    router.push('/orders')
  } catch (error) {
    console.error('Failed to save order:', error)
    notify('negative', `Failed to ${isEdit.value ? 'update' : 'create'} order`, error.message)
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  await loadLookupData()
  if (isEdit.value) {
    await loadOrder(route.params.id)
  }
})
</script>

<style scoped>
.q-table {
  background-color: white;
}

.map-frame-wrapper {
  border-radius: 8px;
  overflow: hidden;
}
</style>
