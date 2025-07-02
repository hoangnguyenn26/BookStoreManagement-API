import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// --- Cấu hình kịch bản test ---
export const options = {
    vus: 16,
    duration: '30s',
    thresholds: {
        'http_req_duration': ['p(95)<1500'], // Ngưỡng cho phép cao hơn một chút
        'http_req_failed': ['rate<0.1'],     // Chấp nhận tỷ lệ lỗi cao hơn (10%) do concurrency
        'checks': ['rate>0.90'],             // Chấp nhận tỷ lệ check thành công trên 90%
    },
};

// --- Các biến và dữ liệu test ---
const API_BASE_URL = 'https://localhost:7264/api';
const ADMIN_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNTQ1MjdlYi1mODA2LTQwZGItYmY3Ni1jN2IwZTVmYTZkMzkiLCJqdGkiOiJlNmM5NGE2My1jODQ0LTQ4OTUtYTBiNi0yODhlYzFjYjliYTUiLCJpYXQiOjE3NDk4MDg3ODQsIm5hbWVpZCI6ImY1NDUyN2ViLWY4MDYtNDBkYi1iZjc2LWM3YjBlNWZhNmQzOSIsInVuaXF1ZV9uYW1lIjoiYWRtaW51c2VyIiwiZW1haWwiOiJhZG1pbkBleGFtcGxlLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc0OTgwODc4NCwiZXhwIjoxNzQ5ODEyMzg0LCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.S0srLAeHbtfI964XLbfzIfJs17uT6JFX-K43NsOqvnM6sSj84mcE-PzKaoXeYScau9w9377rDZAISDTNSVW1pw'; 

const adminHeaders = {
    'Authorization': ADMIN_JWT_TOKEN,
    'Content-Type': 'application/json',
};

const orderIdToUpdate = '423f1031-16b3-4dbd-8d16-6a7a730d2b90'; // Order Pending

// --- Kịch bản Test Chính ---
export default function () {
    // === NHÓM 2: Cập nhật Trạng thái Đơn hàng (Admin) ===
    group('Update_Order_Status_Admin', function () {
        // Cập nhật trạng thái của đơn hàng Pending thành Confirmed (Enum value = 1)
        const updatePayload = JSON.stringify({ newStatus: 1 });
        
        const res = http.put(`${API_BASE_URL}/admin/orders/${orderIdToUpdate}/status`, updatePayload, { headers: adminHeaders });

        // Chấp nhận 204 (No Content - thành công lần đầu) hoặc 400 (Bad Request - nếu logic API không cho update sang trạng thái giống hệt)
        check(res, { 'PUT Update Order Status - status is 204 or 400': (r) => [204, 400].includes(r.status) });
        sleep(2);
    });
}