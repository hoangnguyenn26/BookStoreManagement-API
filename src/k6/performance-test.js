import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { SharedArray } from 'k6/data';
import { Rate, Trend } from 'k6/metrics';

// --- Custom Metrics ---
const bookSearchDuration = new Trend('book_search_duration');
const orderCreationDuration = new Trend('order_creation_duration');
const failedRequests = new Rate('failed_requests');

// --- Configuration ---
const config = {
    baseUrl: __ENV.API_BASE_URL || 'https://localhost:7264/api',
    customerEmail: __ENV.CUSTOMER_EMAIL || 'customer1@example.com',
    customerPassword: __ENV.CUSTOMER_PASSWORD || 'Test123!',
    adminEmail: __ENV.ADMIN_EMAIL || 'admin@example.com',
    adminPassword: __ENV.ADMIN_PASSWORD || 'Admin123!',
};

// --- Test Configuration ---
export const options = {
    scenarios: {
        // Regular user load
        regular_user: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 10 },
                { duration: '1m', target: 10 },
                { duration: '30s', target: 0 },
            ],
            gracefulRampDown: '30s',
        },
        // Admin user load
        admin_user: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 5 },
                { duration: '1m', target: 5 },
                { duration: '30s', target: 0 },
            ],
            gracefulRampDown: '30s',
        },
    },
    thresholds: {
        'http_req_duration': ['p(95)<800'],
        'http_req_failed': ['rate<0.01'],
        'checks': ['rate>0.99'],
        'failed_requests': ['rate<0.01'],
        'book_search_duration': ['p(95)<500'],
        'order_creation_duration': ['p(95)<1000'],
    },
};

// --- Shared Data ---
const testData = new SharedArray('test_data', function () {
    return [
        { searchTerm: 'fiction' },
        { searchTerm: 'science' },
        { searchTerm: 'history' },
        { searchTerm: 'technology' },
    ];
});

// --- Helper Functions ---
function getAuthToken(email, password) {
    const loginRes = http.post(`${config.baseUrl}/v1/auth/login`, JSON.stringify({
        email: email,
        password: password
    }), {
        headers: { 'Content-Type': 'application/json' }
    });

    check(loginRes, {
        'login successful': (r) => r.status === 200,
        'has token': (r) => r.json('token') !== undefined,
    });

    return loginRes.json('token');
}

function createHeaders(token) {
    return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
    };
}

// --- Setup Function ---
export function setup() {
    console.log('Setup: Initializing test data...');
    
    // Get initial data
    const booksRes = http.get(`${config.baseUrl}/v1/books`);
    if (booksRes.status !== 200) {
        throw new Error('Failed to fetch initial book data');
    }

    const books = booksRes.json();
    const validBooks = books.filter(book => 
        book.category && 
        book.category.id && 
        book.category.id !== '00000000-0000-0000-0000-000000000000'
    );

    if (validBooks.length === 0) {
        throw new Error('No valid books found for testing');
    }

    // Get auth tokens
    const customerToken = getAuthToken(config.customerEmail, config.customerPassword);
    const adminToken = getAuthToken(config.adminEmail, config.adminPassword);

    return {
        books: validBooks,
        customerToken,
        adminToken
    };
}

// --- Test Scenarios ---
export default function (data) {
    const { books, customerToken, adminToken } = data;
    const customerHeaders = createHeaders(customerToken);
    const adminHeaders = createHeaders(adminToken);

    const randomBook = books[Math.floor(Math.random() * books.length)];
    const randomSearchTerm = testData[Math.floor(Math.random() * testData.length)].searchTerm;

    // === Public Endpoints ===
    group('Public_Endpoints', function () {
        // Get all books
        group('Get_All_Books', function () {
            const res = http.get(`${config.baseUrl}/v1/books`);
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has books': (r) => r.json().length > 0,
            });
            sleep(1);
        });

        // Get book details
        group('Get_Book_Details', function () {
            const res = http.get(`${config.baseUrl}/v1/books/${randomBook.id}`);
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has correct book': (r) => r.json('id') === randomBook.id,
            });
            sleep(1);
        });

        // Search books
        group('Search_Books', function () {
            const startTime = new Date();
            const res = http.get(`${config.baseUrl}/v1/books/search?query=${randomSearchTerm}`);
            bookSearchDuration.add(new Date() - startTime);
            
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has results': (r) => r.json().length >= 0,
            });
            sleep(1);
        });

        // Filter by category
        group('Filter_By_Category', function () {
            const res = http.get(`${config.baseUrl}/v1/books?categoryId=${randomBook.category.id}`);
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has category books': (r) => r.json().length >= 0,
            });
            sleep(1);
        });
    });

    // === Customer Endpoints ===
    group('Customer_Endpoints', function () {
        // Get order history
        group('Get_Order_History', function () {
            const res = http.get(`${config.baseUrl}/v1/orders`, { headers: customerHeaders });
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has orders': (r) => Array.isArray(r.json()),
            });
            sleep(2);
        });

        // Create order
        group('Create_Order', function () {
            const startTime = new Date();
            const orderData = {
                items: [{
                    bookId: randomBook.id,
                    quantity: 1
                }],
                shippingAddress: {
                    street: "123 Test St",
                    village: "Test Village",
                    district: "Test District",
                    city: "Test City"
                }
            };

            const res = http.post(
                `${config.baseUrl}/v1/orders`,
                JSON.stringify(orderData),
                { headers: customerHeaders }
            );

            orderCreationDuration.add(new Date() - startTime);
            
            check(res, {
                'status is 201': (r) => r.status === 201,
                'has order id': (r) => r.json('id') !== undefined,
            });
            sleep(2);
        });
    });

    // === Admin Endpoints ===
    group('Admin_Endpoints', function () {
        // Get all orders
        group('Get_All_Orders', function () {
            const res = http.get(`${config.baseUrl}/v1/orders/all`, { headers: adminHeaders });
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has orders': (r) => Array.isArray(r.json()),
            });
            sleep(2);
        });

        // Get book statistics
        group('Get_Book_Statistics', function () {
            const res = http.get(`${config.baseUrl}/v1/books/statistics`, { headers: adminHeaders });
            check(res, {
                'status is 200': (r) => r.status === 200,
                'has statistics': (r) => r.json('totalBooks') !== undefined,
            });
            sleep(1);
        });
    });

    // === Error Scenarios ===
    group('Error_Scenarios', function () {
        // Invalid book ID
        group('Invalid_Book_ID', function () {
            const res = http.get(`${config.baseUrl}/v1/books/invalid-id`);
            check(res, {
                'status is 404': (r) => r.status === 404,
            });
            sleep(1);
        });

        // Unauthorized access
        group('Unauthorized_Access', function () {
            const res = http.get(`${config.baseUrl}/v1/orders`, {
                headers: { 'Authorization': 'Bearer invalid-token' }
            });
            check(res, {
                'status is 401': (r) => r.status === 401,
            });
            sleep(1);
        });
    });
} 