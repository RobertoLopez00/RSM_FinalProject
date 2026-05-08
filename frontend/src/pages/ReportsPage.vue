<template>
  <!-- Reports & Analytics Page -->
  <!-- Displays orders dashboard with charts, filters, exports, and a data table -->
  <!-- Features: Excel export, PDF export with branding, real-time filtering, and analytics -->
  <q-page class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <h2 class="q-my-none">Reports & Analytics</h2>
      <div class="row q-gutter-md">
        <q-btn
          label="Export Excel"
          color="primary"
          icon="download"
          outline
          @click="exportToExcel"
          :loading="exporting"
        />
        <q-btn
          label="Export PDF"
          color="primary"
          icon="picture_as_pdf"
          outline
          @click="exportToPDF"
          :loading="exporting"
        />
      </div>
    </div>

    <!-- Filters -->
    <q-card flat bordered class="q-mb-lg">
      <q-card-section>
        <div class="row items-center justify-between q-mb-sm">
          <div>
            <div class="text-h6">Filters</div>
            <div class="text-caption text-grey">Narrow the report before exporting or reviewing charts</div>
          </div>
          <q-btn
            flat
            color="primary"
            label="Clear Filters"
            icon="close"
            :disable="!hasFilters"
            @click="clearFilters"
          />
        </div>

        <div class="row q-col-gutter-md">
          <div class="col-12 col-sm-6 col-md-3">
            <q-select
              v-model="filters.year"
              :options="yearOptions"
              label="Year"
              outlined
              dense
              clearable
              emit-value
              map-options
            />
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-select
              v-model="filters.month"
              :options="monthOptions"
              label="Month"
              outlined
              dense
              clearable
              emit-value
              map-options
            />
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-select
              v-model="filters.week"
              :options="weekOptions"
              label="Week"
              outlined
              dense
              clearable
              emit-value
              map-options
            />
          </div>

          <div class="col-12 col-sm-6 col-md-3">
            <q-select
              v-model="filters.region"
              :options="regionOptions"
              label="Region"
              outlined
              dense
              clearable
              emit-value
              map-options
            />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <!-- Key Metrics -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered>
          <q-card-section class="text-center">
            <div class="text-h3 text-primary">{{ totalOrders }}</div>
            <div class="text-subtitle2 text-grey-7">Total Orders</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered>
          <q-card-section class="text-center">
            <div class="text-h3 text-positive">${{ totalRevenue.toFixed(2) }}</div>
            <div class="text-subtitle2 text-grey-7">Total Revenue</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered>
          <q-card-section class="text-center">
            <div class="text-h3 text-info">{{ averageOrderValue.toFixed(2) }}</div>
            <div class="text-subtitle2 text-grey-7">Avg Order Value</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered>
          <q-card-section class="text-center">
            <div class="text-h3 text-warning">{{ uniqueCustomers }}</div>
            <div class="text-subtitle2 text-grey-7">Customers</div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Charts -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-lg-8">
        <q-card flat bordered class="chart-card">
          <q-card-section>
            <div class="text-h6">Orders Over Time</div>
            <div class="text-caption text-grey">Monthly order counts</div>
          </q-card-section>
          <q-separator />
          <q-card-section class="chart-body">
            <canvas ref="ordersChartRef"></canvas>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-lg-4">
        <q-card flat bordered class="chart-card">
          <q-card-section>
            <div class="text-h6">Shipments by Region</div>
            <div class="text-caption text-grey">Orders grouped by shipping region</div>
          </q-card-section>
          <q-separator />
          <q-card-section class="chart-body">
            <canvas ref="regionChartRef"></canvas>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Orders Table -->
    <q-card class="q-mt-md">
      <q-card-section>
        <div class="text-h6">Order Details</div>
        <div class="text-caption text-grey">All orders for export</div>
      </q-card-section>

      <q-table
        :rows="filteredOrders"
        :columns="columns"
        row-key="orderID"
        :loading="loading"
        flat
        bordered
        class="q-mt-md"
        v-model:pagination="pagination"
        @request="onRequest"
      >
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
          <div class="full-width row flex-center q-pa-lg">
            <span>No orders found</span>
          </div>
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>

<script setup>
/**
 * Reports & Analytics Page Component
 *
 * Features:
 * - Real-time order data fetching from API
 * - Dynamic filtering by year, month, week, and region
 * - Interactive charts (Orders Over Time, Shipments by Region)
 * - Key metrics dashboard (total orders, revenue, average order value, unique customers)
 * - Excel export with summary and detailed sheets
 * - PDF export with branded header and formatted table
 * - Responsive table display with pagination
 *
 * Libraries Used:
 * - Chart.js for data visualization
 * - XLSX for Excel export
 * - jsPDF for PDF generation
 * - Quasar components for UI
 */

import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
import { apiService } from 'src/services/apiService'
import { useQuasar } from 'quasar'
import Chart from 'chart.js/auto'
import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'

const $q = useQuasar()

const orders = ref([])
const loading = ref(false)
const exporting = ref(false)
const ordersChartRef = ref(null)
const regionChartRef = ref(null)
const filters = ref({
  year: null,
  month: null,
  week: null,
  region: null
})
let ordersChart = null
let regionChart = null
const pagination = ref({
  sortBy: 'orderDate',
  descending: true,
  page: 1,
  rowsPerPage: 20
})

const columns = [
  {
    name: 'orderID',
    label: 'Order ID',
    field: 'orderID',
    align: 'left',
    sortable: true
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
    sortable: true,
    format: val => new Date(val).toLocaleDateString()
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
    sortable: true,
    format: val => `$${val?.toFixed(2) || 0}`
  },
  {
    name: 'employeeName',
    label: 'Employee',
    field: 'employeeName',
    align: 'left'
  },
  {
    name: 'shipperName',
    label: 'Shipper',
    field: 'shipperName',
    align: 'left'
  }
]

const monthNames = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
]

const getIsoWeekNumber = (date) => {
  const tempDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()))
  const dayNumber = tempDate.getUTCDay() || 7
  tempDate.setUTCDate(tempDate.getUTCDate() + 4 - dayNumber)
  const yearStart = new Date(Date.UTC(tempDate.getUTCFullYear(), 0, 1))
  return Math.ceil((((tempDate - yearStart) / 86400000) + 1) / 7)
}

const enrichedOrders = computed(() => orders.value.map((order) => {
  const date = new Date(order.orderDate)
  return {
    ...order,
    orderYear: date.getFullYear(),
    orderMonth: date.getMonth() + 1,
    orderWeek: getIsoWeekNumber(date),
    normalizedRegion: (order.shipRegion || order.shipCountry || 'Unknown').trim() || 'Unknown'
  }
}))

const filteredOrders = computed(() => {
  return enrichedOrders.value.filter((order) => {
    const matchesYear = !filters.value.year || order.orderYear === filters.value.year
    const matchesMonth = !filters.value.month || order.orderMonth === filters.value.month
    const matchesWeek = !filters.value.week || order.orderWeek === filters.value.week
    const matchesRegion = !filters.value.region || order.normalizedRegion === filters.value.region
    return matchesYear && matchesMonth && matchesWeek && matchesRegion
  })
})

const totalOrders = computed(() => filteredOrders.value.length)
const totalRevenue = computed(() =>
  filteredOrders.value.reduce((sum, order) => sum + (order.totalAmount || 0), 0)
)
const averageOrderValue = computed(() =>
  totalOrders.value > 0 ? totalRevenue.value / totalOrders.value : 0
)
const uniqueCustomers = computed(() =>
  new Set(filteredOrders.value.map(o => o.customerID)).size
)

const yearOptions = computed(() => {
  const years = [...new Set(enrichedOrders.value.map(order => order.orderYear))].sort((a, b) => b - a)
  return years.map(year => ({ label: String(year), value: year }))
})

const monthOptions = computed(() => {
  const months = [...new Set(enrichedOrders.value.map(order => order.orderMonth))].sort((a, b) => a - b)
  return months.map(month => ({ label: monthNames[month - 1], value: month }))
})

const weekOptions = computed(() => {
  const weeks = [...new Set(enrichedOrders.value.map(order => order.orderWeek))].sort((a, b) => a - b)
  return weeks.map(week => ({ label: `Week ${week}`, value: week }))
})

const regionOptions = computed(() => {
  const regions = [...new Set(enrichedOrders.value.map(order => order.normalizedRegion))].sort((a, b) => a.localeCompare(b))
  return regions.map(region => ({ label: region, value: region }))
})

const hasFilters = computed(() => Boolean(filters.value.year || filters.value.month || filters.value.week || filters.value.region))

const destroyCharts = () => {
  if (ordersChart) {
    ordersChart.destroy()
    ordersChart = null
  }

  if (regionChart) {
    regionChart.destroy()
    regionChart = null
  }
}

const buildOrdersOverTimeData = () => {
  const monthlyCounts = new Map()

  filteredOrders.value.forEach((order) => {
    const date = new Date(order.orderDate)
    const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
    monthlyCounts.set(key, (monthlyCounts.get(key) || 0) + 1)
  })

  const sortedEntries = [...monthlyCounts.entries()].sort((a, b) => a[0].localeCompare(b[0]))

  return {
    labels: sortedEntries.map(([key]) => {
      const [year, month] = key.split('-')
      const labelDate = new Date(Number(year), Number(month) - 1, 1)
      return labelDate.toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
    }),
    values: sortedEntries.map(([, value]) => value)
  }
}

const buildRegionData = () => {
  const regionCounts = new Map()

  filteredOrders.value.forEach((order) => {
    const region = order.normalizedRegion
    regionCounts.set(region, (regionCounts.get(region) || 0) + 1)
  })

  const sortedEntries = [...regionCounts.entries()].sort((a, b) => b[1] - a[1])
  const topEntries = sortedEntries.slice(0, 6)
  const otherTotal = sortedEntries.slice(6).reduce((sum, [, value]) => sum + value, 0)

  if (otherTotal > 0) {
    topEntries.push(['Other', otherTotal])
  }

  return {
    labels: topEntries.map(([label]) => label),
    values: topEntries.map(([, value]) => value)
  }
}

const getStatusColor = (status) => {
  switch ((status || '').trim().toLowerCase()) {
    case 'shipped':
      return 'positive'
    case 'delayed':
      return 'negative'
    case 'pending':
    default:
      return 'warning'
  }
}

const renderCharts = async () => {
  await nextTick()
  destroyCharts()

  if (!ordersChartRef.value || !regionChartRef.value) return

  const ordersData = buildOrdersOverTimeData()
  const regionData = buildRegionData()

  ordersChart = new Chart(ordersChartRef.value, {
    type: 'bar',
    data: {
      labels: ordersData.labels,
      datasets: [{
        label: 'Orders',
        data: ordersData.values,
        backgroundColor: 'rgba(25, 118, 210, 0.75)',
        borderColor: 'rgba(25, 118, 210, 1)',
        borderWidth: 1,
        borderRadius: 6
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: { enabled: true }
      },
      scales: {
        x: {
          ticks: { maxRotation: 45, minRotation: 45 },
          grid: { display: false }
        },
        y: {
          beginAtZero: true,
          ticks: { precision: 0 }
        }
      }
    }
  })

  regionChart = new Chart(regionChartRef.value, {
    type: 'pie',
    data: {
      labels: regionData.labels,
      datasets: [{
        data: regionData.values,
        backgroundColor: [
          '#1976D2',
          '#26A69A',
          '#FFC107',
          '#8E24AA',
          '#FF7043',
          '#546E7A',
          '#66BB6A'
        ],
        borderColor: '#ffffff',
        borderWidth: 2
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom'
        }
      }
    }
  })
}

const clearFilters = () => {
  filters.value = {
    year: null,
    month: null,
    week: null,
    region: null
  }
}

const drawPdfHeader = (pdf, pageWidth) => {
  const leftMargin = 15
  const brandTop = 8
  const navyColor = [0, 26, 61]        // Navy blue
  const tealColor = [29, 212, 191]     // Teal accent

  // Navy background header
  pdf.setFillColor(...navyColor)
  pdf.rect(0, 0, pageWidth, 32, 'F')

  // Add logo image
  pdf.addImage('/logo.png', 'PNG', leftMargin, brandTop, 12, 12)

  // Brand text (white on navy)
  pdf.setTextColor(255, 255, 255)
  pdf.setFontSize(13)
  pdf.setFont(undefined, 'bold')
  pdf.text('NORTHWIND', leftMargin + 14, brandTop + 4)

  // "TRACK" in teal
  pdf.setTextColor(...tealColor)
  pdf.setFontSize(9)
  pdf.text('TRACK', leftMargin + 14, brandTop + 9)
  pdf.text('Orders Report', pageWidth - leftMargin, brandTop + 4, { align: 'right' })

  pdf.setFontSize(7)
  pdf.setFont(undefined, 'normal')
  pdf.text(`Generated: ${new Date().toLocaleDateString()}`, pageWidth - leftMargin, brandTop + 9.5, { align: 'right' })

  // Teal accent line
  pdf.setDrawColor(...tealColor)
  pdf.setLineWidth(1.2)
  pdf.line(leftMargin, 32, pageWidth - leftMargin, 32)

  pdf.setTextColor(0, 0, 0)
  pdf.setFont(undefined, 'normal')

  return 38
}

const loadReportData = async () => {
  loading.value = true
  try {
    orders.value = await apiService.getOrders()
    await renderCharts()
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load report data',
      caption: error.message
    })
  } finally {
    loading.value = false
  }
}

const exportToExcel = async () => {
  exporting.value = true
  try {
    // Prepare data for Excel
    const exportData = filteredOrders.value.map(order => ({
      'Order ID': order.orderID,
      'Customer': order.customerName,
      'Order Date': new Date(order.orderDate).toLocaleDateString(),
      'Required Date': order.requiredDate ? new Date(order.requiredDate).toLocaleDateString() : '',
      'Shipped Date': order.shippedDate ? new Date(order.shippedDate).toLocaleDateString() : '',
      'Status': order.status || 'Pending',
      'Employee': order.employeeName || '',
      'Shipper': order.shipperName || '',
      'Freight': order.freight,
      'Total Amount': order.totalAmount
    }))

    // Create workbook
    const ws = XLSX.utils.json_to_sheet(exportData)
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'Orders')

    // Add summary sheet
    const summaryData = [
      ['Orders Report', new Date().toLocaleDateString()],
      [],
      ['Metric', 'Value'],
      ['Total Orders', totalOrders.value],
      ['Total Revenue', `$${totalRevenue.value.toFixed(2)}`],
      ['Average Order Value', `$${averageOrderValue.value.toFixed(2)}`],
      ['Unique Customers', uniqueCustomers.value]
    ]
    const ws2 = XLSX.utils.aoa_to_sheet(summaryData)
    XLSX.utils.book_append_sheet(wb, ws2, 'Summary')

    // Download
    XLSX.writeFile(wb, `orders_report_${new Date().toISOString().split('T')[0]}.xlsx`)

    $q.notify({
      type: 'positive',
      message: 'Excel file exported successfully',
      position: 'top'
    })
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to export Excel',
      caption: error.message
    })
  } finally {
    exporting.value = false
  }
}

const exportToPDF = async () => {
  exporting.value = true
  try {
    // Create PDF
    const pdf = new jsPDF()
    const pageWidth = pdf.internal.pageSize.getWidth()
    let yPosition = drawPdfHeader(pdf, pageWidth)

    // Summary metrics section
    pdf.setFontSize(12)
    await renderCharts()

    const chartWidth = pageWidth - 30
    const chartHeight = 60
    const ordersCanvas = ordersChartRef.value
    const regionCanvas = regionChartRef.value

    if (ordersCanvas && typeof ordersCanvas.toDataURL === 'function') {
      const ordersImage = ordersCanvas.toDataURL('image/png')
      pdf.addImage(ordersImage, 'PNG', 15, yPosition, chartWidth, chartHeight)
      yPosition += chartHeight + 8
    }

    if (regionCanvas && typeof regionCanvas.toDataURL === 'function') {
      if (yPosition + chartHeight > 270) {
        pdf.addPage()
        yPosition = drawPdfHeader(pdf, pageWidth)
      }
      const regionImage = regionCanvas.toDataURL('image/png')
      pdf.addImage(regionImage, 'PNG', 15, yPosition, chartWidth, chartHeight)
      yPosition += chartHeight + 12
    }

    pdf.text('Summary Metrics', 15, yPosition)
    yPosition += 8

    pdf.setFontSize(10)
    const metrics = [
      { label: 'Total Orders', value: totalOrders.value },
      { label: 'Total Revenue', value: `$${totalRevenue.value.toFixed(2)}` },
      { label: 'Average Order Value', value: `$${averageOrderValue.value.toFixed(2)}` },
      { label: 'Unique Customers', value: uniqueCustomers.value }
    ]

    metrics.forEach(metric => {
      pdf.text(`${metric.label}: ${metric.value}`, 20, yPosition)
      yPosition += 7
    })

    yPosition += 8

    // Order details header
    pdf.setFontSize(12)
    pdf.text('Recent Orders (First 20)', 15, yPosition)
    yPosition += 8

    // Simple table header
    pdf.setFontSize(9)
    pdf.setFont(undefined, 'bold')

    const tableHeaders = ['Order ID', 'Customer', 'Date', 'Status', 'Amount', 'Employee', 'Shipper']
    const colWidths = [16, 34, 24, 24, 22, 28, 24]
    let xPosition = 15

    // Draw header row with dark background and white text
    tableHeaders.forEach((header, idx) => {
      // Set fill color to dark gray
      pdf.setFillColor(64, 64, 64)
      pdf.rect(xPosition, yPosition - 6, colWidths[idx], 8, 'F')
      // Set text color to white
      pdf.setTextColor(255, 255, 255)
      pdf.text(header, xPosition + 2, yPosition + 0.5, { maxWidth: colWidths[idx] - 3 })
      xPosition += colWidths[idx]
    })

    yPosition += 10
    pdf.setFont(undefined, 'normal')
    pdf.setTextColor(0, 0, 0)  // Reset text color to black for body

    // Table rows
    const displayOrders = filteredOrders.value.slice(0, 20)
    displayOrders.forEach((order) => {
      if (yPosition > 280) {
        pdf.addPage()
        yPosition = drawPdfHeader(pdf, pageWidth)
      }

      xPosition = 15
      const rowData = [
        order.orderID.toString(),
        (order.customerName || '').substring(0, 20),
        new Date(order.orderDate).toLocaleDateString(),
        order.status || 'Pending',
        `$${order.totalAmount?.toFixed(2) || '0.00'}`,
        (order.employeeName || '').substring(0, 18),
        (order.shipperName || '').substring(0, 18)
      ]

      rowData.forEach((cell, cellIdx) => {
        if (cellIdx === 3) {
          let statusColor = [255, 165, 0]
          switch ((cell || '').toString().trim().toLowerCase()) {
            case 'shipped':
              statusColor = [34, 139, 34]
              break
            case 'delayed':
              statusColor = [220, 20, 60]
              break
            default:
              statusColor = [255, 140, 0]
              break
          }

          pdf.setFillColor(...statusColor)
          pdf.rect(xPosition, yPosition - 5, colWidths[cellIdx], 7, 'F')
          pdf.setTextColor(255, 255, 255)
          pdf.text(cell, xPosition + 2, yPosition, { maxWidth: colWidths[cellIdx] - 3 })
          pdf.setTextColor(0, 0, 0)
        } else {
          pdf.text(cell, xPosition + 2, yPosition, { maxWidth: colWidths[cellIdx] - 3 })
        }
        xPosition += colWidths[cellIdx]
      })

      yPosition += 7
    })

    yPosition += 5

    // Footer
    pdf.setFontSize(8)
    pdf.setTextColor(150, 150, 150)
    pdf.text(`Report showing first 20 of ${totalOrders.value} total orders. For complete data, use Excel export.`, 15, yPosition)

    // Download
    pdf.save(`orders_report_${new Date().toISOString().split('T')[0]}.pdf`)

    // Success - PDF was generated and downloaded
    console.log('PDF export completed successfully')
  } catch (error) {
    console.error('PDF export error:', error)
  } finally {
    exporting.value = false
  }
}

const onRequest = () => {
  // Pagination handled by local data
}

watch(filteredOrders, () => {
  if (!loading.value) {
    renderCharts()
  }
})

onMounted(() => {
  loadReportData()
})

onBeforeUnmount(() => {
  destroyCharts()
})
</script>

<style scoped>
.q-card {
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.chart-card {
  min-height: 420px;
}

.chart-body {
  height: 320px;
}
</style>
