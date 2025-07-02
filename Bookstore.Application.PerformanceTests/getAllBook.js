import http from 'k6/http';
import { check, group, sleep } from 'k6';

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

// --- Hàm Setup: Chạy một lần duy nhất trước khi test bắt đầu ---
export function setup() {
    console.log('Setup: Fetching initial data for testing...');
    
    const res = http.get(`${API_BASE_URL}/v1/books`);
    
    if (res.status !== 200 || !res.body || res.json().length === 0) {
        throw new Error('Setup failed: Could not fetch initial book data or no books found.');
    }
    
    const books = res.json();

    const validBookData = books
        .filter(book => 
            book.category &&
            book.category.id &&  
            book.category.id !== '00000000-0000-0000-0000-000000000000'
        )
        .map(book => ({
            id: book.id,
            categoryId: book.category.id 
        }));

    if (validBookData.length === 0) {
         throw new Error('Setup failed: No books with valid Category IDs were found in the API response.');
    }

    console.log(`Setup complete: Found ${validBookData.length} valid books to use for testing.`);
    
    return { books: validBookData };
}

// --- Kịch bản Test Chính ---
// 'data' là kết quả trả về từ hàm setup()
export default function (data) {
    const randomBook = data.books[Math.floor(Math.random() * data.books.length)];
    
    if (!randomBook || !randomBook.id) {
        console.error("Critical error: No valid book data available for this iteration.");
        return; 
    }

    // === NHÓM 1: Lấy tất cả sách ===
    group('Get_All_Books_Public', function () {
        const res = http.get(`${API_BASE_URL}/v1/books`);
        check(res, { 'GET All Books - status is 200': (r) => r.status === 200 });
        sleep(1);
    });
}