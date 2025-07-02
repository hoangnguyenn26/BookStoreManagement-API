import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
    vus: 16,
    duration: '30s',
    thresholds: {
        'http_req_duration': ['p(95)<1000', 'p(99)<2000'],
        'checks': ['rate>0.98'],
        'http_req_failed{expected_response:true}': ['rate<0.02'],
        'group_duration{group:::Delete_Address_Customer}': ['p(95)<1500'],
    },
};

const API_BASE_URL = 'https://localhost:7264/api';
const CUSTOMER_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjN2I2YTVlNC04ZTFkLTRjOWMtOWYyZS04YjFkMDU2NGYwMWIiLCJqdGkiOiJmMzc0Njc0YS0zMWIzLTQ3YjItOTM3OS05Y2ZlODlhOGUzOWYiLCJpYXQiOjE3NDk4MTk3NDAsIm5hbWVpZCI6ImM3YjZhNWU0LThlMWQtNGM5Yy05ZjJlLThiMWQwNTY0ZjAxYiIsInVuaXF1ZV9uYW1lIjoiY3VzdG9tZXIxIiwiZW1haWwiOiJjdXN0b21lcjFAZXhhbXBsZS5jb20iLCJyb2xlIjoiVXNlciIsIm5iZiI6MTc0OTgxOTc0MCwiZXhwIjoxNzQ5ODIzMzQwLCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.xA2T9Yfe2J3mbo-qlg2Nb9MD-xnbF72wnH6eM4ZT1FaEKmLy_8ibslZ9r2qtxVq-iedAkUvykNdHwW0TS9Hm-A';

const customerHeaders = { 'Authorization': CUSTOMER_JWT_TOKEN, 'Content-Type': 'application/json' };

export function setup() {
    console.log('Setup: Creating addresses to be deleted...');
    const addressesToDelete = [];
    const numVUs = options.vus;
    let setupErrors = 0;

    for (let i = 0; i < numVUs; i++) {
        try {
            let addressPayload = JSON.stringify({
                street: `Delete Street ${i + Date.now()}`,
                village: `Delete Village ${i}`,
                district: 'Test District',
                city: 'Test City',
                isDefault: false
            });
            
            let addressRes = http.post(`${API_BASE_URL}/v1/user/addresses`, addressPayload, { headers: customerHeaders });
            
            if (addressRes.status === 201 && addressRes.json('id')) {
                addressesToDelete.push(addressRes.json('id'));
            } else {
                console.error(`Setup: Failed to create address ${i}. Status: ${addressRes.status}`);
                setupErrors++;
            }
            
            // Rate limiting để tránh overwhelm server trong setup
            if ((i + 1) % 5 === 0) {
                sleep(0.2);
            } else {
                sleep(0.05);
            }
            
        } catch (error) {
            console.error(`Setup: Exception creating address for VU ${i}: ${error.message}`);
            setupErrors++;
        }
    }

    // Validation setup results
    const minResourcesNeeded = Math.ceil(numVUs * 0.8); // Chấp nhận 80% success rate
    
    if (addressesToDelete.length < minResourcesNeeded) {
        throw new Error(`Setup failed: Created only ${addressesToDelete.length}/${numVUs} addresses (minimum needed: ${minResourcesNeeded})`);
    }
    
    console.log(`Setup complete: Created ${addressesToDelete.length} addresses. Setup errors: ${setupErrors}`);
    
    return { 
        addresses: addressesToDelete,
        setupStats: {
            addressesCreated: addressesToDelete.length,
            errors: setupErrors
        }
    };
}

export default function (data) {
    const vuIndex = __VU - 1;
    const addressIdToDelete = data.addresses[vuIndex];

    group('Delete_Address_Customer', function () {
        if (addressIdToDelete) {
            const params = {
                headers: customerHeaders,
                tags: { expected_response: true }
            };
            
            const res = http.del(`${API_BASE_URL}/v1/user/addresses/${addressIdToDelete}`, null, params);
            
            check(res, {
                'DELETE Address - status is 204 or 404': (r) => [204, 404].includes(r.status),
                'DELETE Address - response time < 2s': (r) => r.timings.duration < 2000,
                'DELETE Address - no server error': (r) => r.status < 500,
                'DELETE Address - content-type header exists': (r) => r.headers['Content-Type'] !== undefined || r.status === 204
            });
            
            if (![204, 404].includes(res.status)) {
                console.error(`VU${__VU}: Unexpected DELETE address response: ${res.status}`);
            }
        } else {
            console.warn(`VU${__VU}: No address ID available for deletion`);
        }
        
        sleep(1);
    });
}

export function teardown(data) {
    if (data && data.setupStats) {
        console.log('=== DELETE ADDRESS TEST SUMMARY ===');
        console.log(`Addresses created in setup: ${data.setupStats.addressesCreated}`);
        console.log(`Setup errors: ${data.setupStats.errors}`);
        console.log('Cleanup completed - All test addresses should be deleted by the test itself');
    }
}