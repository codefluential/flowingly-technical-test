#!/usr/bin/env node
/**
 * Cross-platform script runner
 * Detects OS and runs appropriate shell script (.sh for Unix, .ps1 for Windows)
 */

const { spawn } = require('child_process');
const path = require('path');
const os = require('os');

// Get script name from command line
const scriptName = process.argv[2];
if (!scriptName) {
  console.error('❌ Error: Script name required');
  console.error('   Usage: node run-script.js <dev|test|build|clean>');
  process.exit(1);
}

// Determine platform and script
const isWindows = os.platform() === 'win32';
const scriptExt = isWindows ? '.ps1' : '.sh';
const scriptPath = path.join(__dirname, `${scriptName}${scriptExt}`);

// Determine shell command
let command, args;
if (isWindows) {
  command = 'powershell';
  args = ['-ExecutionPolicy', 'Bypass', '-File', scriptPath];
} else {
  command = scriptPath;
  args = [];
}

console.log(`🔧 Running ${scriptName}${scriptExt} on ${os.platform()}...`);
console.log('');

// Spawn process with inherited stdio (so output appears in real-time)
const proc = spawn(command, args, {
  stdio: 'inherit',
  shell: isWindows, // Windows needs shell for PowerShell
  cwd: path.join(__dirname, '..')
});

// Handle exit
proc.on('exit', (code) => {
  process.exit(code || 0);
});

// Handle errors
proc.on('error', (err) => {
  console.error(`❌ Error running ${scriptName}${scriptExt}:`, err.message);
  process.exit(1);
});
