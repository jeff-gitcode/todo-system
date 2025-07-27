import { chromium } from '@playwright/test';
import { exec } from 'child_process';
import { promisify } from 'util';

const execAsync = promisify(exec);
let jsonServerProcess: any = null;

// Export a single function as the main setup function
async function setup() {
  // Start json-server if needed
  try {
    console.log('Starting json-server...');
    jsonServerProcess = exec('yarn json-server', {
      cwd: process.cwd(),
    });

    // Wait for json-server to start
    await new Promise(resolve => setTimeout(resolve, 3000));

    // Verify json-server is running
    try {
      await execAsync('curl http://localhost:3001/todos');
      console.log('json-server is running');
    } catch (error) {
      console.error('json-server is not running correctly', error);
      throw error;
    }

    // Return a function to be called during teardown
    return async () => {
      // Clean up json-server process if we started it
      if (jsonServerProcess) {
        console.log('Stopping json-server...');
        jsonServerProcess.kill();
      }
    };
  } catch (error) {
    console.error('Failed to start json-server', error);
    throw error;
  }
}

// Export a single function as required by Playwright
export default setup;