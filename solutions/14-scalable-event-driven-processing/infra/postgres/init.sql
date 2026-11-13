CREATE TABLE IF NOT EXISTS inventory_products (
    product_id uuid PRIMARY KEY,
    name text NOT NULL,
    unit_price numeric(18, 2) NOT NULL CHECK (unit_price >= 0),
    available integer NOT NULL CHECK (available >= 0),
    reserved integer NOT NULL DEFAULT 0 CHECK (reserved >= 0)
);

CREATE TABLE IF NOT EXISTS orders (
    order_id uuid PRIMARY KEY,
    basket_id uuid NOT NULL,
    status text NOT NULL,
    reason text NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS order_items (
    order_id uuid NOT NULL REFERENCES orders(order_id),
    product_id uuid NOT NULL REFERENCES inventory_products(product_id),
    quantity integer NOT NULL CHECK (quantity > 0),
    unit_price numeric(18, 2) NOT NULL CHECK (unit_price >= 0),
    PRIMARY KEY (order_id, product_id)
);

CREATE TABLE IF NOT EXISTS processed_events (
    event_id uuid PRIMARY KEY,
    order_id uuid NOT NULL REFERENCES orders(order_id),
    processed_at timestamptz NOT NULL
);

INSERT INTO inventory_products(product_id, name, unit_price, available) VALUES
    ('11111111-1111-1111-1111-111111111111', 'هدفون بی‌سیم', 2450000, 18),
    ('22222222-2222-2222-2222-222222222222', 'کیبورد مکانیکی', 3890000, 12),
    ('33333333-3333-3333-3333-333333333333', 'ماوس ارگونومیک', 1570000, 25),
    ('44444444-4444-4444-4444-444444444444', 'پایه لپ‌تاپ', 980000, 30)
ON CONFLICT (product_id) DO NOTHING;
