import http from 'k6/http';
import { check, group, sleep } from 'k6';

export const options = {
    vus: 16,
    duration: '30s',
    thresholds: {
        'http_req_duration': ['p(95)<1000', 'p(99)<2000'],
        'checks': ['rate>0.98'],
        'http_req_failed{expected_response:true}': ['rate<0.02'],
        'group_duration{group:::Delete_Author_Admin}': ['p(95)<1500'],
    },
};

const API_BASE_URL = 'https://localhost:7264/api';
const ADMIN_JWT_TOKEN = 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNTQ1MjdlYi1mODA2LTQwZGItYmY3Ni1jN2IwZTVmYTZkMzkiLCJqdGkiOiI3MDQ0NjdlYS1lZTgzLTQwZDktOTY5ZS03OWEzOTA2MGU0MDEiLCJpYXQiOjE3NDk4MTk3NTcsIm5hbWVpZCI6ImY1NDUyN2ViLWY4MDYtNDBkYi1iZjc2LWM3YjBlNWZhNmQzOSIsInVuaXF1ZV9uYW1lIjoiYWRtaW51c2VyIiwiZW1haWwiOiJhZG1pbkBleGFtcGxlLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc0OTgxOTc1NywiZXhwIjoxNzQ5ODIzMzU3LCJpc3MiOiJCb29rc3RvcmVNYW5hZ2VtZW50QXBpIiwiYXVkIjoiQm9va3N0b3JlTWFuYWdlbWVudEFwaUNsaWVudCJ9.FYm55UhFa_eQMi2_6dDqoXHhnnJwrh2Dz5P4B_YV5RBDwYtgVoYpUqyMD0A9CrYOgObzPBQNMhhwINk0ixO6RQ';

const adminHeaders = { 'Authorization': ADMIN_JWT_TOKEN, 'Content-Type': 'application/json' };

export function setup() {
    console.log('Setup: Creating authors to be deleted...');
    const authorsToDelete = [];
    const numVUs = options.vus;
    let setupErrors = 0;

    for (let i = 0; i < numVUs; i++) {
        try {
            let authorPayload = JSON.stringify({
                name: `Deletable Author ${i + Date.now()}`,
                biography: 'This author is created for deletion testing.'
            });
            
            let authorRes = http.post(`${API_BASE_URL}/admin/authors`, authorPayload, { headers: adminHeaders });
            
            if (authorRes.status === 201 && authorRes.json('id')) {
                authorsToDelete.push(authorRes.json('id'));
            } else {
                console.error(`Setup: Failed to create author ${i}. Status: ${authorRes.status}`);
                setupErrors++;
            }
            
            // Rate limiting để tránh overwhelm server trong setup
            if ((i + 1) % 5 === 0) {
                sleep(0.2);
            } else {
                sleep(0.05);
            }
            
        } catch (error) {
            console.error(`Setup: Exception creating author for VU ${i}: ${error.message}`);
            setupErrors++;
        }
    }

    // Validation setup results
    const minResourcesNeeded = Math.ceil(numVUs * 0.8); // Chấp nhận 80% success rate
    
    if (authorsToDelete.length < minResourcesNeeded) {
        throw new Error(`Setup failed: Created only ${authorsToDelete.length}/${numVUs} authors (minimum needed: ${minResourcesNeeded})`);
    }
    
    console.log(`Setup complete: Created ${authorsToDelete.length} authors. Setup errors: ${setupErrors}`);
    
    return { 
        authors: authorsToDelete,
        setupStats: {
            authorsCreated: authorsToDelete.length,
            errors: setupErrors
        }
    };
}

export default function (data) {
    const vuIndex = __VU - 1;
    const authorIdToDelete = data.authors[vuIndex];

    group('Delete_Author_Admin', function () {
        if (authorIdToDelete) {
            const params = {
                headers: adminHeaders,
                tags: { expected_response: true }
            };
            
            const res = http.del(`${API_BASE_URL}/admin/authors/${authorIdToDelete}`, null, params);
            
            check(res, {
                'DELETE Author - status is 204 or 404': (r) => [204, 404].includes(r.status),
                'DELETE Author - response time < 2s': (r) => r.timings.duration < 2000,
                'DELETE Author - no server error': (r) => r.status < 500,
                'DELETE Author - content-type header exists': (r) => r.headers['Content-Type'] !== undefined || r.status === 204
            });
            
            if (![204, 404].includes(res.status)) {
                console.error(`VU${__VU}: Unexpected DELETE author response: ${res.status}`);
            }
        } else {
            console.warn(`VU${__VU}: No author ID available for deletion`);
        }
        
        sleep(1);
    });
}

export function teardown(data) {
    if (data && data.setupStats) {
        console.log('=== DELETE AUTHOR TEST SUMMARY ===');
        console.log(`Authors created in setup: ${data.setupStats.authorsCreated}`);
        console.log(`Setup errors: ${data.setupStats.errors}`);
        console.log('Cleanup completed - All test authors should be deleted by the test itself');
    }

}