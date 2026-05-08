<template>
  <q-page class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <h2 class="q-my-none">Orders</h2>
      <q-btn
        color="primary"
        label="New Order"
        icon="add"
        to="/orders/new"
      />
    </div>

    <q-table
      title="Order Management"
      :rows="orders"
      :columns="columns"
      row-key="orderID"
      :loading="loading"
      v-model:pagination="pagination"
      @request="onRequest"
      class="q-mt-md"
    >
      <template v-slot:body-cell-actions="props">
        <q-td :props="props">
          <q-btn
            flat
            dense
            round
            icon="edit"
            size="sm"
            color="primary"
            @click="editOrder(props.row.orderID)"
            title="Edit"
          />
          <q-btn
            flat
            dense
            round
            icon="delete"
            size="sm"
            color="negative"
            @click="deleteOrder(props.row.orderID)"
            title="Delete"
          />
        </q-td>
      </template>

      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <q-chip
            :label="props.row.status || 'Pending'"
            :color="getStatusColor(props.row.status)"
            text-color="white"
            dense
            outline
          />
        </q-td>
      </template>

      <template v-slot:no-data>
        <div class="full-width row flex-center q-gutter-sm">
          <span>No orders found</span>
        </div>
      </template>
    </q-table>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { apiService } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()
const router = useRouter()

const orders = ref([])
const loading = ref(false)
const pagination = ref({
  sortBy: 'orderDate',
  descending: true,
  page: 1,
  rowsPerPage: 10
})

const columns = [
  {
    name: 'orderID',
    label: 'Order ID',
    field: 'orderID',
    align: 'left'
  },
  {
    name: 'customerName',
    label: 'Customer',
    field: 'customerName',
    align: 'left'
  },
  {
    name: 'orderDate',
    label: 'Order Date',
    field: 'orderDate',
    align: 'left',
    format: val => new Date(val).toLocaleDateString()
  },
  {
    name: 'requiredDate',
    label: 'Required Date',
    field: 'requiredDate',
    align: 'left',
    format: val => val ? new Date(val).toLocaleDateString() : ''
  },
  {
    name: 'shippedDate',
    label: 'Shipped Date',
    field: 'shippedDate',
    align: 'left',
    format: val => val ? new Date(val).toLocaleDateString() : ''
  },
  {
    name: 'status',
    label: 'Status',
    field: 'status',
    align: 'left'
  },
  {
    name: 'totalAmount',
    label: 'Total Amount',
    field: 'totalAmount',
    align: 'right',
    format: val => `$${val?.toFixed(2) || 0}`
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'center'
  }
]

const loadOrders = async () => {
  loading.value = true
  try {
    orders.value = await apiService.getOrders()
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load orders',
      caption: error.message
    })
  } finally {
    loading.value = false
  }
}

const editOrder = (orderId) => {
  router.push(`/orders/${orderId}/edit`)
}

const getStatusColor = (status) => {
  switch ((status || '').trim().toLowerCase()) {
    case 'shipped':
    case 'delivered':
      return 'positive'
    case 'delayed':
      return 'negative'
    case 'pending':
    default:
      return 'warning'
  }
}

const deleteOrder = async (orderId) => {
  // Use browser confirm as fallback since $q.dialog not working
  if (!confirm(`Are you sure you want to delete order #${orderId}?`)) {
    return
  }

  try {
    await apiService.deleteOrder(orderId)
    alert('Order deleted successfully')
    loadOrders()
  } catch (error) {
    alert(`Failed to delete order: ${error.message}`)
  }
}

const onRequest = () => {
  loadOrders()
}

onMounted(() => {
  loadOrders()
})
</script>
