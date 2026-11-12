import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import './App.css'

type Product = { id: string; name: string; price: number; available: number }
type BasketItem = { productId: string; quantity: number }
type Basket = { id: string; status: string; items: BasketItem[] }
type Order = { id: string; status: string; reason?: string | null; items: { productId: string; quantity: number; unitPrice: number }[] }

const api = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080').replace(/\/$/, '')
const money = new Intl.NumberFormat('fa-IR')

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${api}${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })
  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new Error(body?.detail ?? body?.message ?? `خطای سرور (${response.status})`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

function App() {
  const [products, setProducts] = useState<Product[]>([])
  const [basket, setBasket] = useState<Basket | null>(null)
  const [order, setOrder] = useState<Order | null>(null)
  const [busy, setBusy] = useState(false)
  const [updatingProductIds, setUpdatingProductIds] = useState<Set<string>>(new Set())
  const [message, setMessage] = useState('')
  const inFlightProducts = useRef(new Set<string>())

  const loadBasket = useCallback(async (id: string) => {
    const next = await request<Basket>(`/api/baskets/${id}`)
    setBasket(next)
    return next
  }, [])

  useEffect(() => {
    let active = true
    async function start() {
      try {
        const catalog = await request<Product[]>('/api/products')
        if (active) setProducts(catalog)
        let basketId = localStorage.getItem('hpt-basket-id')
        if (!basketId) {
          const created = await request<{ id: string }>('/api/baskets', { method: 'POST' })
          basketId = created.id
          localStorage.setItem('hpt-basket-id', basketId)
        }
        if (active) await loadBasket(basketId)
      } catch (error) {
        if (active) setMessage(error instanceof Error ? error.message : 'ارتباط با سرویس برقرار نشد.')
      }
    }
    void start()
    return () => { active = false }
  }, [loadBasket])

  const quantities = useMemo(() => new Map(basket?.items.map((item) => [item.productId, item.quantity]) ?? []), [basket])
  const itemCount = basket?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0
  const total = products.reduce((sum, product) => sum + product.price * (quantities.get(product.id) ?? 0), 0)
  const basketIsLocked = basket?.status === 'checkout-pending' || basket?.status === 'checkout-complete'

  async function setQuantity(product: Product, quantity: number) {
    if (!basket || basket.status === 'checkout-pending' || basket.status === 'checkout-complete' || busy || inFlightProducts.current.has(product.id)) return
    setOrder(null)
    inFlightProducts.current.add(product.id)
    setUpdatingProductIds((current) => new Set(current).add(product.id))
    setMessage('')
    const safeQuantity = Math.max(0, Math.min(quantity, 1000))
    setBasket((current) => current ? {
      ...current,
      items: [...current.items.filter((item) => item.productId !== product.id), ...(safeQuantity ? [{ productId: product.id, quantity: safeQuantity }] : [])],
    } : current)
    try {
      await request(`/api/baskets/${basket.id}/items/${product.id}`, {
        method: 'PUT', body: JSON.stringify({ quantity: safeQuantity }),
      })
      setMessage('تغییر سبد ثبت شد.')
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'ثبت تغییر سبد ناموفق بود.')
      await loadBasket(basket.id).catch(() => undefined)
    } finally {
      inFlightProducts.current.delete(product.id)
      setUpdatingProductIds((current) => {
        const next = new Set(current)
        next.delete(product.id)
        return next
      })
    }
  }

  async function checkout() {
    if (!basket || itemCount === 0 || busy || inFlightProducts.current.size > 0) return
    setBusy(true)
    setOrder(null)
    setMessage('سفارش ثبت شد؛ موجودی در حال بررسی است…')
    const keyName = `hpt-checkout-key:${basket.id}`
    const idempotencyKey = sessionStorage.getItem(keyName) ?? crypto.randomUUID()
    sessionStorage.setItem(keyName, idempotencyKey)
    try {
      const accepted = await request<{ orderId: string }>(`/api/baskets/${basket.id}/checkout`, {
        method: 'POST', body: JSON.stringify({ idempotencyKey }),
      })
      setBasket((current) => current ? { ...current, status: 'checkout-pending' } : current)
      localStorage.setItem('hpt-last-order-id', accepted.orderId)
      const started = Date.now()
      let receivedFinal = false
      while (Date.now() - started < 45000) {
        await new Promise((resolve) => window.setTimeout(resolve, 900))
        try {
          const result = await request<Order>(`/api/orders/${accepted.orderId}`)
          setOrder(result)
          if (result.status === 'inventory-reserved') {
            setMessage('موجودی رزرو شد و سفارش تأیید شد.')
            setBasket((current) => current ? { ...current, status: 'checkout-complete' } : current)
            sessionStorage.removeItem(keyName)
            receivedFinal = true
            break
          }
          if (result.status === 'rejected') {
            setMessage(result.reason ?? 'موجودی کافی نیست؛ سفارش رد شد.')
            setBasket((current) => current ? { ...current, status: 'checkout-rejected' } : current)
            sessionStorage.removeItem(keyName)
            receivedFinal = true
            break
          }
        } catch {
          // Order projection may not exist until the Kafka consumers finish this event.
        }
      }
      if (!receivedFinal) setMessage('سفارش هنوز در صف پردازش است؛ شناسهٔ سفارش ذخیره شده است.')
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'ثبت سفارش ناموفق بود.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="page-shell" dir="rtl">
      <header className="topbar">
        <a className="brand" href="#top" aria-label="خانهٔ تراکنش‌یار">
          <span className="brand-mark">ت</span>
          <span><strong>تراکنش‌یار</strong><small>فروشگاه نمونه</small></span>
        </a>
        <div className="system-state"><span className="pulse" />پردازش سفارش با Kafka</div>
      </header>

      <section className="hero" id="top">
        <div>
          <p className="eyebrow">فروشگاه آزمایشی · رویدادمحور</p>
          <h1>انتخاب کن،<br /><span>سفارش بده.</span></h1>
          <p className="hero-copy">سبد شما سریع ثبت می‌شود؛ رزرو موجودی و نتیجهٔ سفارش در پس‌زمینه انجام می‌شود.</p>
          <a className="text-link" href="#catalog">دیدن کالاها <span aria-hidden="true">←</span></a>
        </div>
        <div className="hero-card" aria-label="نمای خلاصهٔ پردازش سفارش">
          <div className="hero-card-top"><span>جریان سفارش</span><span className="live-badge">LIVE</span></div>
          <div className="flow-step"><i>۱</i><span><b>سبد خرید</b><small>فرمان در Kafka ثبت می‌شود</small></span><em>✓</em></div>
          <div className="flow-line" />
          <div className="flow-step"><i>۲</i><span><b>کنترل موجودی</b><small>رزرو اتمیک کالاها</small></span><em className="flow-dots">•••</em></div>
          <div className="flow-line" />
          <div className="flow-step"><i>۳</i><span><b>نتیجه سفارش</b><small>تأیید یا رد موجودی</small></span><em className="flow-wait">—</em></div>
        </div>
        <div className="hero-orb orb-one" /><div className="hero-orb orb-two" />
      </section>

      <section className="catalog-section" id="catalog">
        <div className="section-heading">
          <div><p className="eyebrow">فروشگاه</p><h2>کالاهای منتخب</h2></div>
          <div className="count-pill">{money.format(products.length)} کالا</div>
        </div>
        {message && <div className="notice" role="status">{message}</div>}
        {products.length === 0 ? (
          <div className="empty-catalog"><span>□</span><b>فعلاً کالایی نمایش داده نمی‌شود</b><small>اتصال API و پایگاه داده را بررسی کنید.</small></div>
        ) : (
          <div className="shop-layout">
            <div className="product-grid">
              {products.map((product, index) => {
                const quantity = quantities.get(product.id) ?? 0
                return (
                  <article className="product-card" key={product.id}>
                    <div className={`product-art art-${index % 4}`}><span>{['۰' + (index + 1), 'پیشنهاد ویژه', 'ارسال سریع', 'منتخب'][index % 4]}</span><b>{product.name.slice(0, 1)}</b></div>
                    <div className="product-info">
                      <div><h3>{product.name}</h3><p>موجودی: {money.format(product.available)}</p></div>
                      <strong className="price">{money.format(product.price)} <small>تومان</small></strong>
                    </div>
                    <div className="card-action">
                      {quantity === 0 ? (
                        <button className="add-button" onClick={() => void setQuantity(product, 1)} disabled={!basket || basketIsLocked || busy || updatingProductIds.has(product.id)}>افزودن به سبد <span>＋</span></button>
                      ) : (
                        <div className="quantity-control" aria-label={`تعداد ${product.name}`}>
                          <button onClick={() => void setQuantity(product, quantity + 1)} aria-label="افزایش" disabled={basketIsLocked || busy || updatingProductIds.has(product.id)}>＋</button>
                          <span>{money.format(quantity)}</span>
                          <button onClick={() => void setQuantity(product, quantity - 1)} aria-label="کاهش" disabled={basketIsLocked || busy || updatingProductIds.has(product.id)}>−</button>
                        </div>
                      )}
                      {quantity > product.available && <small className="stock-warning">از موجودی بیشتر است؛ checkout رد می‌شود.</small>}
                    </div>
                  </article>
                )
              })}
            </div>

            <aside className="basket-panel">
              <div className="basket-title"><div><p className="eyebrow">سبد خرید</p><h2>سبد شما</h2></div><span className="basket-count">{money.format(itemCount)}</span></div>
              {!basket || itemCount === 0 ? (
                <div className="basket-empty"><span>🛒</span><b>سبد هنوز خالی است</b><small>یک کالا انتخاب کنید تا اینجا ببینید.</small></div>
              ) : (
                <>
                  <div className="basket-lines">
                    {basket.items.map((item) => {
                      const product = products.find((candidate) => candidate.id === item.productId)
                      if (!product) return null
                      return <div className="basket-line" key={item.productId}><span>{product.name}<small>تعداد {money.format(item.quantity)}</small></span><b>{money.format(product.price * item.quantity)} <small>تومان</small></b></div>
                    })}
                  </div>
                  <div className="basket-total"><span>جمع کالاها</span><b>{money.format(total)} <small>تومان</small></b></div>
                  <button className="checkout-button" onClick={() => void checkout()} disabled={busy || updatingProductIds.size > 0 || basket.status === 'checkout-complete'}>
                    {busy ? 'در حال بررسی…' : basket.status === 'checkout-pending' ? 'ادامهٔ پیگیری' : 'ثبت سفارش'} <span>←</span>
                  </button>
                  <p className="basket-footnote">پرداخت در این نمونه انجام نمی‌شود؛ فقط رزرو موجودی بررسی می‌شود.</p>
                </>
              )}
              {order && <div className={`order-status ${order.status === 'rejected' ? 'rejected' : ''}`}><small>وضعیت سفارش</small><b>{order.status === 'inventory-reserved' ? 'تأیید شد' : order.status === 'rejected' ? 'رد شد' : 'در حال پردازش'}</b><code>{order.id.slice(0, 8)}</code></div>}
            </aside>
          </div>
        )}
      </section>
      <footer className="footer"><span>تراکنش‌یار</span><span>نمونهٔ آموزشی برای پردازش سبد و سفارش</span><span>Kafka · .NET · Redis</span></footer>
    </main>
  )
}

export default App
