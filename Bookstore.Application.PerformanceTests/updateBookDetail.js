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

const bookData = {
    id: 'F233A54E-3EE6-4916-AA37-12F3D1507322', // The Pragmatic Programmer
    categoryId: 'C3D7E6F5-B1D8-4A1F-9C5B-3B2A1C0D9E8A', // Tech
    authorId: '4D5E6F1A-2B3C-4D4E-9F5A-6B3C4D5E6F7A'    // Hunt
};

// --- Kịch bản Test Chính ---
export default function () {

    // === NHÓM 1: Cập nhật Sách (Admin) ===
    group('Update_Book_Details_Admin', function () {
        const updatePayload = JSON.stringify({
            title: `K6 Updated - The Pragmatic Programmer VU ${__VU} Iter ${__ITER}`, // Dữ liệu động
            description: "Updated by k6 performance test.",
            price: 355000.00,
            stockQuantity: Math.floor(Math.random() * 50) + 50, // Số lượng ngẫu nhiên
            categoryId: bookData.categoryId,
            authorId: bookData.authorId
        });
        
        const res = http.put(`${API_BASE_URL}/v1/books/${bookData.id}`, updatePayload, { headers: adminHeaders });
        
        // Chấp nhận 204 (No Content - thành công) hoặc 409 (Conflict - lỗi concurrency)
        check(res, { 'PUT Update Book - status is 204 or 409': (r) => [204, 409].includes(r.status) });
        sleep(2);
    });
}