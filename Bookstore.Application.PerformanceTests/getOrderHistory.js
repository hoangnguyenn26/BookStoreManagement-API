import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// --- Cấu hình kịch bản test ---
export const options = {
    vus: 16,
    duration: '30s',
    thresholds: {
        'http_req_duration': ['p(95)<800'],
        'http_req_failed': ['rate<0.01'],
        'checks': ['rate>0.99'],
    },
};

// --- Các biến và dữ liệu test ---
const API_BASE_URL = 'https://localhost:7264/api';
const CUSTOMER_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjN2I2YTVlNC04ZTFkLTRjOWMtOWYyZS04YjFkMDU2NGYwMWIiLCJqdGkiOiI0ODVhNGFlZC04OGU3LTQ5MTUtOGVlNS1hNjIyZDA1NDBkZDYiLCJpYXQiOjE3NDk4MDcwODAsIm5hbWVpZCI6ImM3YjZhNWU0LThlMWQtNGM5Yy05ZjJlLThiMWQwNTY0ZjAxYiIsInVuaXF1ZV9uYW1lIjoiY3VzdG9tZXIxIiwiZW1haWwiOiJjdXN0b21lcjFAZXhhbXBsZS5jb20iLCJyb2xlIjoiVXNlciIsIm5iZiI6MTc0OTgwNzA4MCwiZXhwIjoxNzQ5ODEwNjgwLCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.qpycseR2o7tHxADXiYm6ejp_0ym4zdjHnOVk1Z-oUkb8FW6gX7Yrn-UMYkLXz2_sBWYfrn2eRdLRpnUp2pMGBA';

export default function () {
    const customerHeaders = {
        'Authorization': CUSTOMER_JWT_TOKEN,
        'Content-Type': 'application/json',
    };

    // Lấy lịch sử đơn hàng 
    group('Get_User_Order_History', function () {
        const res = http.get(`${API_BASE_URL}/v1/orders`, { headers: customerHeaders });
        check(res, {
            'GET Order History - status is 200': (r) => r.status === 200,
        });
        sleep(2);
    });
}