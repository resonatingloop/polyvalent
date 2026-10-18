import { readFile, readdir } from "node:fs/promises";
import { extname } from "node:path";

const packageJson = JSON.parse(await readFile(new URL("../package.json", import.meta.url), "utf8"));
const runtimeDependencies = Object.keys(packageJson.dependencies ?? {}).sort();
const expectedRuntimeDependencies = ["react", "react-dom"];
if (JSON.stringify(runtimeDependencies) !== JSON.stringify(expectedRuntimeDependencies)) {
  throw new Error(`unexpected public runtime dependencies: ${runtimeDependencies.join(", ")}`);
}

const files = await sourceFiles(new URL("../src/", import.meta.url));
const forbidden = [
  /microsoft\.data\.sqlite/i,
  /better-sqlite3/i,
  /\bsqlite3?\b/i,
  /polyvalent\.data/i,
  /databasepathresolver/i,
  /privatenotes/i,
  /sessionstorage/i,
  /localstorage/i,
  /\.env\b/i,
];
const fetchTargets = [];
for (const file of files) {
  const source = await readFile(file, "utf8");
  for (const pattern of forbidden) {
    if (pattern.test(source)) throw new Error(`${file.pathname} contains forbidden public-boundary token ${pattern}`);
  }
  for (const match of source.matchAll(/fetch\(\s*["']([^"']+)["']/g)) fetchTargets.push(match[1]);
}

const allowedFetchTargets = new Set(["/data/data.json", "/data/manifest.json"]);
for (const target of fetchTargets) {
  if (!allowedFetchTargets.has(target)) throw new Error(`unexpected public runtime fetch target: ${target}`);
}
if (fetchTargets.length !== allowedFetchTargets.size) {
  throw new Error(`expected exactly ${allowedFetchTargets.size} local release fetches, found ${fetchTargets.length}`);
}

console.log(`public boundary ok: ${files.length} source files, ${runtimeDependencies.length} runtime dependencies, ${fetchTargets.length} local fetches`);

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const nested = await Promise.all(entries.map(async (entry) => {
    const child = new URL(`${entry.name}${entry.isDirectory() ? "/" : ""}`, directory);
    if (entry.isDirectory()) return sourceFiles(child);
    return [child];
  }));
  return nested.flat().filter((file) => [".ts", ".tsx"].includes(extname(file.pathname)));
}
