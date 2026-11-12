import http from 'k6/http';
import { check } from 'k6';

const baseUrl = __ENV.BASE_URL || 'http://127.0.0.1:15080';
const rate = Number(__ENV.RATE || 100);
const duration = __ENV.DURATION || '60s';
const preAllocatedVUs = Number(__ENV.PRE_ALLOCATED_VUS || 100);
const maxVUs = Number(__ENV.MAX_VUS || 1000);
const productId = __ENV.PRODUCT_ID || '33333333-3333-3333-3333-333333333333';

export const options = {
  scenarios: {
    basket_commands: {
      executor: 'constant-arrival-rate',
      rate,
      timeUnit: '1s',
      duration,
      preAllocatedVUs,
      maxVUs,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<250', 'p(99)<1000'],
  },
};

function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = Math.floor(Math.random() * 16);
    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

export default function () {
  const basketId = uuid();
  const response = http.put(
    `${baseUrl}/api/baskets/${basketId}/items/${productId}`,
    JSON.stringify({ quantity: 1 }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: {
        name: 'basket_item_set',
        url: `${baseUrl}/api/baskets/{basketId}/items/{productId}`,
        operation: 'basket_item_set',
      },
    },
  );

  check(response, {
    'basket command accepted': (r) => r.status === 202,
  });
}
