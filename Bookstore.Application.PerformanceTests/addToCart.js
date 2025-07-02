import http from 'k6/http';
import { check, group, sleep } from 'k6';

// --- Cấu hình kịch bản test ---
export const options = {
    vus: 16,
    duration: '30s',
    thresholds: {
        'http_req_duration': ['p(95)<2000'],
        'http_req_failed': ['rate<0.02'], 
        'checks': ['rate>0.98'],
    },
};

// --- Các biến và dữ liệu test ---
const API_BASE_URL = 'https://localhost:7264/api';
const CUSTOMER_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjN2I2YTVlNC04ZTFkLTRjOWMtOWYyZS04YjFkMDU2NGYwMWIiLCJqdGkiOiIyNTUwNjQ0MC0wNTkwLTRkMDAtYjVmNS1hMmQ4ZTgzYWY2NGQiLCJpYXQiOjE3NDk4MDk0MDAsIm5hbWVpZCI6ImM3YjZhNWU0LThlMWQtNGM5Yy05ZjJlLThiMWQwNTY0ZjAxYiIsInVuaXF1ZV9uYW1lIjoiY3VzdG9tZXIxIiwiZW1haWwiOiJjdXN0b21lcjFAZXhhbXBsZS5jb20iLCJyb2xlIjoiVXNlciIsIm5iZiI6MTc0OTgwOTQwMCwiZXhwIjoxNzQ5ODEzMDAwLCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.-E9jCflHDLOXXkGdGx2cZakswZCyvBMjWVhSQuXu3g76A4h89eSv_R75Kq2C9mmrIItcWG07mNnpxuRp3jG5OQ';
const ADMIN_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNTQ1MjdlYi1mODA2LTQwZGItYmY3Ni1jN2IwZTVmYTZkMzkiLCJqdGkiOiJlNmM5NGE2My1jODQ0LTQ4OTUtYTBiNi0yODhlYzFjYjliYTUiLCJpYXQiOjE3NDk4MDg3ODQsIm5hbWVpZCI6ImY1NDUyN2ViLWY4MDYtNDBkYi1iZjc2LWM3YjBlNWZhNmQzOSIsInVuaXF1ZV9uYW1lIjoiYWRtaW51c2VyIiwiZW1haWwiOiJhZG1pbkBleGFtcGxlLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc0OTgwODc4NCwiZXhwIjoxNzQ5ODEyMzg0LCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.S0srLAeHbtfI964XLbfzIfJs17uT6JFX-K43NsOqvnM6sSj84mcE-PzKaoXeYScau9w9377rDZAISDTNSVW1pw';

const customerHeaders = { 'Authorization': CUSTOMER_JWT_TOKEN, 'Content-Type': 'application/json' };
const adminHeaders = { 'Authorization': ADMIN_JWT_TOKEN, 'Content-Type': 'application/json' };

// --- Hàm Setup: Chạy một lần duy nhất để lấy dữ liệu cần thiết ---
export function setup() {
    console.log('Setup: Fetching initial data for write tests...');
    
    // Dùng token Admin để có thể lấy được nhiều thông tin hơn nếu cần
    const resBooks = http.get(`${API_BASE_URL}/v1/books`, { headers: adminHeaders });
    const resSuppliers = http.get(`${API_BASE_URL}/admin/suppliers`, { headers: adminHeaders });
    const resAddresses = http.get(`${API_BASE_URL}/v1/user/addresses`, { headers: customerHeaders });


    if (resBooks.status !== 200 || resSuppliers.status !== 200 ||  resAddresses.status !== 200) {
        throw new Error('Setup failed: Could not fetch all required initial data.');
    }
    
    const books = resBooks.json().filter(b => b.stockQuantity > 0); // Chỉ lấy sách còn hàng
    if (books.length === 0) {
        throw new Error('Setup failed: No books in stock available for testing.');
    }

    const data = {
        books: books,
        suppliers: resSuppliers.json(),
        addresses: resAddresses.json(),
    };
    
    console.log(`Setup complete. Found ${data.books.length} books, ${data.suppliers.length} suppliers, etc.`);
    return data;
}

// --- Kịch bản Test Chính ---
export default function (data) {
    // Lấy ngẫu nhiên dữ liệu đã chuẩn bị từ setup
    const randomBook = data.books[Math.floor(Math.random() * data.books.length)];
    const randomSupplier = data.suppliers[Math.floor(Math.random() * data.suppliers.length)];
    const shippingAddress = data.addresses[0]; // Giả sử lấy địa chỉ đầu tiên

    if (!randomBook || !randomSupplier || !shippingAddress) {
        console.error("Critical error: Missing required data for this iteration.");
        return; // Bỏ qua nếu thiếu dữ liệu
    }

    // === NHÓM 1: Thêm vào Giỏ hàng (Customer) ===
    group('Add_Item_To_Cart', function () {
        const payload = JSON.stringify({ bookId: randomBook.id, quantity: 1 });
        const res = http.post(`${API_BASE_URL}/v1/cart/items`, payload, { headers: customerHeaders });
        
        // Chấp nhận 200 (OK - cập nhật số lượng) hoặc 400 (Bad Request - ví dụ, sách đã có trong giỏ và logic API của bạn không cho cộng dồn)
        check(res, { 'POST Add to Cart - status is 200 or 400': (r) => [200, 400].includes(r.status) });
        sleep(1);
    });
}