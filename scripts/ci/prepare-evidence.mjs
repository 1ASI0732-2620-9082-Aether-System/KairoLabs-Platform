import { rmSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import assert from 'node:assert/strict';
assert.equal(process.env.GITHUB_ACTIONS,'true','Solo se limpia evidencia en el checkout descartable de Actions.');
const target=resolve('evidence');assert.equal(dirname(target),resolve('.'));
rmSync(target,{recursive:true,force:true});
console.log('Evidencia histórica retirada del checkout CI; solo se publicarán resultados de esta ejecución.');
