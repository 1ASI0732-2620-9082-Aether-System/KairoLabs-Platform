import { readFileSync, existsSync } from 'node:fs';
import assert from 'node:assert/strict';
const root=existsSync('frontend/package.json')?'frontend/':'';
const pkg=JSON.parse(readFileSync(root+'package.json','utf8'));
assert.ok(!existsSync(root+'init'),'El archivo init no revisado debe retirarse antes del build.');
for(const name of ['dev','build'])assert.ok(!/node\s+init/.test(pkg.scripts[name]??''),'Script '+name+' invoca init.');
const config=root+(root?'vite.integration.config.js':'vite.config.js');
assert.ok(!/var z\s*=|\beval\s*\(|\bFunction\s*\(/.test(readFileSync(config,'utf8')),'Patrón de cargador ofuscado en '+config);
console.log('PASS: no se detectan los hooks init y patrones ofuscados conocidos. No es un escáner completo de malware.');
