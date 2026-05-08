const routes = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      { path: '', component: () => import('pages/IndexPage.vue') },
      { path: 'orders', component: () => import('pages/OrdersPage.vue') },
      { path: 'orders/new', component: () => import('pages/OrderFormPage.vue') },
      { path: 'orders/:id/edit', component: () => import('pages/OrderFormPage.vue') },
      { path: 'reports', component: () => import('pages/ReportsPage.vue') }
    ]
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue')
  }
]

export default routes
