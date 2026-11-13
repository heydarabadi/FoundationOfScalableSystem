import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const baseUrl = __ENV.BASE_URL || 'http://127.0.0.1:15080';
const rate = Number(__ENV.RATE || 100);
const duration = __ENV.DURATION || '30s';
const batchSize = Number(__ENV.BATCH_SIZE || 50);
const preAllocatedVUs = Number(__ENV.PRE_ALLOCATED_VUS || 100);
const maxVUs = Number(__ENV.MAX_VUS || 1000);
const submitted = new Counter('basket_commands_submitted');
const productId = __ENV.PRODUCT_ID || '33333333-3333-3333-3333-333333333333';

if (!Number.isInteger(batchSize) || batchSize < 1 || batchSize > 500) {
  throw new Error('BATCH_SIZE must be an integer between 1 and 500');
}

export const options = {
  scenarios: {
    basket_command_batches: {
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
  },
};

function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = Math.floor(Math.random() * 16);
    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

export default function () {
  const items = Array.from({ length: batchSize }, () => ({
    basketId: uuid(),
    productId,
    quantity: 1,
  }));
  const response = http.post(
    `${baseUrl}/api/baskets/commands/batch`,
    JSON.stringify({ items }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: {
        name: 'basket_command_batch',
        url: `${baseUrl}/api/baskets/commands/batch`,
        operation: 'basket_command_batch',
      },
    },
  );

  const accepted = check(response, {
    'basket command batch accepted': (r) => r.status === 202,
  });
  if (accepted) submitted.add(batchSize);
}
