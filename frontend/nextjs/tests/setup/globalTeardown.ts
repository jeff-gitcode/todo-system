import { exec } from 'child_process';

// This function will be called after all tests are done
async function teardown() {
    console.log('Running global teardown...');

    // Find and kill any running json-server processes
    if (process.platform === 'win32') {
        exec('taskkill /f /im node.exe /fi "WINDOWTITLE eq json-server"', (error) => {
            if (error) {
                console.log('No json-server process found to kill or error occurred');
            } else {
                console.log('json-server process terminated');
            }
        });
    } else {
        exec('pkill -f "json-server"', (error) => {
            if (error) {
                console.log('No json-server process found to kill or error occurred');
            } else {
                console.log('json-server process terminated');
            }
        });
    }
}

export default teardown;